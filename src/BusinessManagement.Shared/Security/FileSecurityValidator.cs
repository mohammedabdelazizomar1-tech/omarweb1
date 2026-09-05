using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;

namespace BusinessManagement.Shared.Security;

public static class FileSecurityValidator
{
    private static readonly Dictionary<string, byte[]> MagicNumbers = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        { ".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // standard zip/office magic
        { ".xls", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        { ".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }
    };

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".xlsx", ".xls", ".zip" };
    private static readonly string[] AllowedMimeTypes = { 
        "image/jpeg", "image/png", "application/pdf", 
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        "application/vnd.ms-excel", "application/zip",
        "application/octet-stream", "application/x-zip-compressed",
        "application/x-excel", "application/x-msexcel", "application/excel"
    };

    public static (bool IsValid, string ErrorMessage) ValidateFile(Stream fileStream, string fileName, string contentType, long lengthBytes)
    {
        // 1. Max Size Check (5MB)
        const long maxSizeBytes = 5 * 1024 * 1024;
        if (lengthBytes > maxSizeBytes)
        {
            return (false, "حجم الملف يتجاوز الحد الأقصى المسموح به وهو 5 ميجابايت.");
        }

        // 2. Strict Extension Check
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return (false, "امتداد الملف غير مسموح به.");
        }

        // 3. Strict MIME-type Check
        if (!AllowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            return (false, "نوع محتوى الملف (MIME-type) غير مدعوم.");
        }

        // 4. Magic Number Signature Validation
        byte[] buffer = new byte[8];
        fileStream.Position = 0;
        int read = fileStream.Read(buffer, 0, buffer.Length);
        fileStream.Position = 0; // Reset position

        if (MagicNumbers.TryGetValue(ext, out var signature))
        {
            if (read < signature.Length || !buffer.Take(signature.Length).SequenceEqual(signature))
            {
                return (false, "فشل التحقق من صحة توقيع الملف (Magic Number). قد يكون الملف تالفاً أو تم تزييف امتداده.");
            }
        }

        // 5. ZIP Bomb / Compression Ratio Check (Point 6/v2 & Point 5/v4)
        if (ext == ".zip" || ext == ".xlsx")
        {
            try
            {
                using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
                long totalUncompressedSize = 0;
                const long maxUncompressedLimit = 100 * 1024 * 1024; // 100MB
                const double maxCompressionRatio = 100.0; // max 100x ratio

                foreach (var entry in archive.Entries)
                {
                    if (entry.Length > maxUncompressedLimit)
                    {
                        return (false, "تحذير أمني: تم اكتشاف محاولة رفع قنبلة ضغط (ZIP Bomb) - حجم ملف غير مضغوط كبير جداً.");
                    }

                    // Check compression ratio
                    if (entry.Length > 0 && entry.CompressedLength > 0)
                    {
                        double ratio = (double)entry.Length / entry.CompressedLength;
                        if (ratio > maxCompressionRatio)
                        {
                            return (false, "تحذير أمني: تم اكتشاف محاولة رفع قنبلة ضغط (ZIP Bomb) - نسبة ضغط غير طبيعية.");
                        }
                    }
                    totalUncompressedSize += entry.Length;
                }

                if (totalUncompressedSize > maxUncompressedLimit)
                {
                    return (false, "حجم الملفات غير المضغوطة يتجاوز الحد المسموح به.");
                }
            }
            catch
            {
                return (false, "فشل قراءة الملف المضغوط. قد يكون الملف تالفاً.");
            }
            finally
            {
                fileStream.Position = 0; // Always reset stream
            }
        }

        // 6. Antivirus Stub Hook (Point 6/v2 & Point 4/v4)
        bool virusScanPassed = RunAntivirusScanStub(fileStream);
        if (!virusScanPassed)
        {
            return (false, "تحذير أمني: تم اكتشاف تهديد برمجيات خبيثة في الملف المرفوع.");
        }

        return (true, string.Empty);
    }

    public static async Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(Stream fileStream, string fileName, string contentType, long lengthBytes, CancellationToken cancellationToken = default)
    {
        var syncResult = ValidateFile(fileStream, fileName, contentType, lengthBytes);
        if (!syncResult.IsValid)
        {
            return syncResult;
        }

        bool virusScanPassed = await ScanWithClamAvAsync(fileStream, cancellationToken);
        if (!virusScanPassed)
        {
            return (false, "تحذير أمني: تم اكتشاف تهديد برمجيات خبيثة في الملف المرفوع.");
        }

        return (true, string.Empty);
    }

    private static bool RunAntivirusScanStub(Stream stream)
    {
        try
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            string content = reader.ReadToEnd();
            stream.Position = 0;

            if (content.Contains("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"))
            {
                return false; // Virus detected!
            }
        }
        catch
        {
            // Suppress and continue if binary file reading fails text-decode
        }
        return true;
    }

    public static async Task<bool> ScanWithClamAvAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // Enforce ClamAV scan via TCP socket standard INSTREAM command protocol (Point 4/Review)
        string host = Environment.GetEnvironmentVariable("CLAMAV_HOST") ?? "localhost";
        int port = int.TryParse(Environment.GetEnvironmentVariable("CLAMAV_PORT"), out var p) ? p : 3310;

        try
        {
            using var tcpClient = new System.Net.Sockets.TcpClient();
            
            // Connect with timeout (5 seconds)
            var connectTask = tcpClient.ConnectAsync(host, port, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(5000, cancellationToken);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask || !tcpClient.Connected)
            {
                // Timeout or offline: check fail-closed policy
                bool failClosed = Environment.GetEnvironmentVariable("CLAMAV_FAIL_CLOSED") == "true";
                return !failClosed; // if failClosed is true, return false (reject); otherwise return true (fail-open)
            }

            using var netStream = tcpClient.GetStream();
            
            // Send INSTREAM command
            byte[] instreamCommand = System.Text.Encoding.ASCII.GetBytes("zINSTREAM\0");
            await netStream.WriteAsync(instreamCommand, 0, instreamCommand.Length, cancellationToken);

            byte[] buffer = new byte[8192];
            stream.Position = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                // Format: <chunk-size-big-endian><chunk-data>
                byte[] chunkSize = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(bytesRead));
                await netStream.WriteAsync(chunkSize, 0, chunkSize.Length, cancellationToken);
                await netStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }

            // Terminate chunk stream with zero-size chunk
            byte[] zeroChunk = BitConverter.GetBytes(0);
            await netStream.WriteAsync(zeroChunk, 0, zeroChunk.Length, cancellationToken);
            await netStream.FlushAsync(cancellationToken);

            // Read response
            using var reader = new StreamReader(netStream, System.Text.Encoding.ASCII);
            string response = await reader.ReadToEndAsync(cancellationToken);

            if (response.Contains("FOUND"))
            {
                return false; // Malware detected!
            }
        }
        catch
        {
            bool failClosed = Environment.GetEnvironmentVariable("CLAMAV_FAIL_CLOSED") == "true";
            if (failClosed) return false;
        }
        finally
        {
            stream.Position = 0;
        }

        return true;
    }

    public static byte[] StripImageMetadata(byte[] imageBytes, string extension)
    {
        // Strips EXIF metadata from JPEG to protect privacy and remove geotags (Point 6/v2 & Point 5/v4)
        if (extension == ".jpg" || extension == ".jpeg")
        {
            try
            {
                using var msInput = new MemoryStream(imageBytes);
                using var msOutput = new MemoryStream();
                
                int b1 = msInput.ReadByte();
                int b2 = msInput.ReadByte();
                
                if (b1 == 0xFF && b2 == 0xD8) // Valid JPEG SOI (Start of Image)
                {
                    msOutput.WriteByte((byte)b1);
                    msOutput.WriteByte((byte)b2);
                    
                    while (true)
                    {
                        int marker1 = msInput.ReadByte();
                        if (marker1 == -1) break;
                        if (marker1 != 0xFF)
                        {
                            msOutput.WriteByte((byte)marker1);
                            continue;
                        }
                        
                        int marker2 = msInput.ReadByte();
                        if (marker2 == -1) break;
                        
                        if (marker2 == 0xD9) // EOI (End of Image)
                        {
                            msOutput.WriteByte(0xFF);
                            msOutput.WriteByte((byte)marker2);
                            break;
                        }
                        
                        // Read segment size
                        int sizeH = msInput.ReadByte();
                        int sizeL = msInput.ReadByte();
                        if (sizeH == -1 || sizeL == -1) break;
                        int size = (sizeH << 8) + sizeL;
                        
                        // Skip APP1-APP15 markers (0xE1 - 0xEF) which hold EXIF metadata, GPS, XML
                        if (marker2 >= 0xE1 && marker2 <= 0xEF)
                        {
                            msInput.Seek(size - 2, SeekOrigin.Current); // Skip segment data
                        }
                        else
                        {
                            msOutput.WriteByte(0xFF);
                            msOutput.WriteByte((byte)marker2);
                            msOutput.WriteByte((byte)sizeH);
                            msOutput.WriteByte((byte)sizeL);
                            
                            byte[] buffer = new byte[size - 2];
                            int read = msInput.Read(buffer, 0, buffer.Length);
                            msOutput.Write(buffer, 0, read);
                        }
                    }
                    return msOutput.ToArray();
                }
            }
            catch
            {
                return imageBytes;
            }
        }
        return imageBytes;
    }
}

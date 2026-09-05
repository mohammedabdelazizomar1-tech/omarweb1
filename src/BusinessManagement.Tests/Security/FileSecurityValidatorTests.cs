using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using BusinessManagement.Shared.Security;
using Xunit;

namespace BusinessManagement.Tests.Security;

public class FileSecurityValidatorTests
{
    [Fact]
    public void ValidateFile_WithMaxSizeExceeded_ReturnsInvalid()
    {
        // Arrange
        byte[] dummyBytes = new byte[6 * 1024 * 1024]; // 6MB
        using var stream = new MemoryStream(dummyBytes);

        // Act
        var result = FileSecurityValidator.ValidateFile(stream, "test.pdf", "application/pdf", dummyBytes.Length);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("الحد الأقصى", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFile_WithInvalidExtension_ReturnsInvalid()
    {
        // Arrange
        byte[] dummyBytes = new byte[100];
        using var stream = new MemoryStream(dummyBytes);

        // Act
        var result = FileSecurityValidator.ValidateFile(stream, "dangerous.exe", "application/x-msdownload", dummyBytes.Length);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("غير مسموح به", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFile_WithSpoofedExtension_ReturnsInvalid()
    {
        // Arrange
        byte[] fakePdfBytes = Encoding.UTF8.GetBytes("This is actually a plain text file spoofed as pdf");
        using var stream = new MemoryStream(fakePdfBytes);

        // Act
        var result = FileSecurityValidator.ValidateFile(stream, "spoofed.pdf", "application/pdf", fakePdfBytes.Length);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Magic Number", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFile_WithZipBomb_ReturnsInvalid()
    {
        // Arrange: Create a highly compressed zip bomb payload
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("bomb.txt");
            using var writer = new StreamWriter(entry.Open());
            // Write a highly compressed repeating string (e.g. 200,000 'A' characters)
            writer.Write(new string('A', 200000));
        }
        ms.Position = 0;

        // Act
        var result = FileSecurityValidator.ValidateFile(ms, "bomb.zip", "application/zip", ms.Length);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("ZIP Bomb", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFile_WithAntivirusTestString_ReturnsInvalid()
    {
        // Arrange: EICAR test string
        byte[] eicarBytes = Encoding.UTF8.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");
        // Prepend PDF magic bytes to satisfy magic signature validation
        byte[] pdfHeader = { 0x25, 0x50, 0x44, 0x46 };
        byte[] fileBytes = new byte[pdfHeader.Length + eicarBytes.Length];
        Buffer.BlockCopy(pdfHeader, 0, fileBytes, 0, pdfHeader.Length);
        Buffer.BlockCopy(eicarBytes, 0, fileBytes, pdfHeader.Length, eicarBytes.Length);

        using var stream = new MemoryStream(fileBytes);

        // Act
        var result = FileSecurityValidator.ValidateFile(stream, "eicar.pdf", "application/pdf", fileBytes.Length);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("برمجيات خبيثة", result.ErrorMessage);
    }

    [Fact]
    public void StripImageMetadata_WithApp1MetadataSegment_ShouldRemoveSegment()
    {
        // Arrange: Construct a dummy JPEG bytes payload with APP1 segment (0xFFE1)
        byte[] jpegHeader = { 0xFF, 0xD8 }; // SOI
        byte[] app1Marker = { 0xFF, 0xE1, 0x00, 0x0A, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00, 0x11, 0x22 }; // APP1 EXIF segment (10 bytes size+payload)
        byte[] dqtMarker = { 0xFF, 0xDB, 0x00, 0x05, 0x01, 0x02, 0x03 }; // DQT segment
        byte[] jpegFooter = { 0xFF, 0xD9 }; // EOI

        byte[] originalImage = new byte[jpegHeader.Length + app1Marker.Length + dqtMarker.Length + jpegFooter.Length];
        int pos = 0;
        Buffer.BlockCopy(jpegHeader, 0, originalImage, pos, jpegHeader.Length); pos += jpegHeader.Length;
        Buffer.BlockCopy(app1Marker, 0, originalImage, pos, app1Marker.Length); pos += app1Marker.Length;
        Buffer.BlockCopy(dqtMarker, 0, originalImage, pos, dqtMarker.Length); pos += dqtMarker.Length;
        Buffer.BlockCopy(jpegFooter, 0, originalImage, pos, jpegFooter.Length);

        // Act
        byte[] strippedImage = FileSecurityValidator.StripImageMetadata(originalImage, ".jpg");

        // Assert
        Assert.NotNull(strippedImage);
        Assert.True(strippedImage.Length < originalImage.Length);
        // Verify APP1 marker (FFE1) is gone, but DQT (FFDB) is preserved
        string originalHex = BitConverter.ToString(originalImage);
        string strippedHex = BitConverter.ToString(strippedImage);

        Assert.Contains("FF-E1", originalHex);
        Assert.DoesNotContain("FF-E1", strippedHex);
        Assert.Contains("FF-DB", strippedHex);
    }
}

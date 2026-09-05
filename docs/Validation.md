# Input & File Validation Specifications

## 1. File Upload Security Constraints
Implemented inside `FileSecurityValidator.cs`:
- **Maximum File Size**: Strict **5 MB** limit (`5 * 1024 * 1024` bytes).
- **Allowed Extensions**: `.jpg`, `.jpeg`, `.png`, `.pdf`, `.xlsx`, `.xls`, `.zip`.
- **Allowed MIME-Types**:
  - `image/jpeg`, `image/png`
  - `application/pdf`
  - `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, `application/vnd.ms-excel`
  - `application/zip`

## 2. Magic Numbers (Signature Verification)
Validates the actual starting bytes to prevent renaming malicious executables:
- `.png` -> `89 50 4E 47 0D 0A 1A 0A`
- `.pdf` -> `25 50 44 46`
- `.xlsx` / `.zip` -> `50 4B 03 04`

## 3. ZIP Bomb Protection
Scans zip archives and Excel files during extraction:
- **Max Uncompressed Limit**: Rejects files if any entry or the total uncompressed files exceed **100 MB**.
- **Max Compression Ratio**: Rejects files if the compression ratio is greater than **100x**.

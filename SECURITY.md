# System Security hardened Implementations

## 1. Malware Scanning (ClamAV)
The system connects to ClamAV socket scanner:
- Uses `CLAMAV_HOST` (default `localhost`) and `CLAMAV_PORT` (default `3310`) environment variables.
- Connects using a TCP client with a strict **5-second timeout**.
- Sends file streams using the `zINSTREAM` command format.
- Checks `CLAMAV_FAIL_CLOSED` env: if `"true"`, it rejects uploads when ClamAV is offline (Fail-Closed). If false, it allows uploads (Fail-Open).

## 2. EXIF Metadata Stripping
To protect traveler privacy, the system strips EXIF metadata blocks (APP1-APP15 markers, `0xE1` to `0xEF`) from JPEG images.

## 3. Cookie Session Hardening
Configured in `Program.cs`:
- Cookie Name: `BusinessManagement.SessionCookie`
- SameSite: `Strict`
- HttpOnly: `true`
- ExpireTimeSpan: `8 Hours`
- SlidingExpiration: `true`

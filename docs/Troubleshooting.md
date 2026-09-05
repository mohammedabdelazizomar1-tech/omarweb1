# Troubleshooting & Support Guide

Common runtime and compilation issues and their fixes:

## 1. ClamAV Connection Failures
- **Symptom**: File uploads fail with a socket exception: "Could not connect to ClamAV socket scanner".
- **Fix**: Verify the ClamAV docker container is running. If not using ClamAV locally, toggle off antivirus enforcement in your developer properties or `appsettings.Development.json`.

## 2. NuGet EPPlus Restore Warning
- **Symptom**: Compilation warning about EPPlus assembly conflicts.
- **Fix**: Clear local package cache:
  ```bash
  dotnet nuget locals all --clear
  ```
  Ensure all projects (Application, Web) reference EPPlus version `7.0.0` to prevent DLL mismatches.

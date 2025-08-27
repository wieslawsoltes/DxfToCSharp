# Build Scripts

This directory contains build scripts that simplify the CI/CD workflow and can be used for local development.

## Scripts Overview

### macOS Build
- **Script**: `build-macos.sh`
- **Purpose**: Builds and packages the macOS application as an unsigned app bundle
- **Output**: `DxfToCSharp-macOS.zip` with installation instructions
- **Requirements**: macOS with .NET SDK installed

### Windows x64 Build
- **Script**: `build-windows-x64.ps1` (PowerShell) or `build-windows-x64.sh` (Bash)
- **Purpose**: Builds and packages the Windows x64 application
- **Output**: `DxfToCSharp-Windows-x64.zip`
- **Requirements**: Windows with .NET SDK installed

### Windows x86 Build
- **Script**: `build-windows-x86.ps1` (PowerShell) or `build-windows-x86.sh` (Bash)
- **Purpose**: Builds and packages the Windows x86 application
- **Output**: `DxfToCSharp-Windows-x86.zip`
- **Requirements**: Windows with .NET SDK installed

## Usage

### Local Development

To build locally, run the appropriate script for your platform:

```bash
# macOS
./scripts/build-macos.sh

# Windows (PowerShell)
./scripts/build-windows-x64.ps1
./scripts/build-windows-x86.ps1

# Windows (Bash/WSL)
./scripts/build-windows-x64.sh
./scripts/build-windows-x86.sh
```

### CI/CD Integration

The scripts are automatically used by the GitHub Actions workflow defined in `.github/workflows/ci-cd.yml`. The workflow:

1. Sets up the .NET environment
2. Makes scripts executable (for Unix-like systems)
3. Runs the appropriate build script
4. Uploads the generated artifacts

## Script Features

- **Error Handling**: All scripts use `set -e` (Bash) or proper error handling (PowerShell) to fail fast on errors
- **Clean Builds**: Scripts clean previous build outputs before starting
- **Dependency Management**: Scripts handle dependency restoration automatically
- **Consistent Output**: All scripts produce ZIP archives with predictable names
- **Logging**: Scripts provide clear progress messages during execution

## Benefits

1. **Maintainability**: Build logic is centralized in scripts rather than scattered in YAML
2. **Local Testing**: Developers can run the same build process locally
3. **Debugging**: Easier to debug build issues by running scripts individually
4. **Consistency**: Same build process across different environments
5. **Flexibility**: Scripts can be easily modified without touching CI/CD configuration

## Troubleshooting

### Permission Issues (macOS/Linux)
If you get permission denied errors:
```bash
chmod +x scripts/*.sh
```

### PowerShell Execution Policy (Windows)
If PowerShell scripts are blocked:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Missing Dependencies
Ensure you have the .NET SDK installed:
```bash
dotnet --version
```

The scripts expect to be run from the repository root directory.
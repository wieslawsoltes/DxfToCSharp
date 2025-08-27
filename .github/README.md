# CI/CD Pipeline Documentation

This repository uses GitHub Actions for continuous integration and deployment. The pipeline automatically builds, tests, and publishes the DxfToCSharp application and NuGet packages.

## Workflows

### Main CI/CD Pipeline (`ci-cd.yml`)

This is the primary workflow that handles:

- **Testing**: Runs unit tests on Ubuntu
- **macOS App Building**: Creates a macOS app bundle and DMG installer
- **Windows App Building**: Creates Windows single executables for x64 and x86
- **NuGet Package Publishing**: Builds and publishes NuGet packages and symbols
- **Release Asset Management**: Uploads all artifacts to GitHub releases

#### Triggers

- **Push** to `main` or `develop` branches
- **Pull requests** to `main` branch
- **Release** events (when a new release is published)

#### Jobs

1. **test**: Runs on Ubuntu, executes unit tests
2. **build-macos**: Runs on macOS, creates app bundle and DMG
3. **build-windows**: Runs on Windows, creates single executables
4. **build-nuget**: Runs on Ubuntu, builds and publishes NuGet packages
5. **release**: Runs on Ubuntu, uploads all artifacts to GitHub releases

### Validation Workflow (`validate.yml`)

A utility workflow that validates the syntax of all GitHub Actions workflow files.

#### Triggers

- **Manual dispatch** (workflow_dispatch)
- **Push** events that modify workflow files

## Artifacts

The pipeline produces the following artifacts:

### Application Packages

- **macOS**: `DxfToCSharp-macOS.dmg` - DMG installer for macOS
- **Windows x64**: `DxfToCSharp-Windows-x64.zip` - Single executable for Windows 64-bit
- **Windows x86**: `DxfToCSharp-Windows-x86.zip` - Single executable for Windows 32-bit

### NuGet Packages

- **DxfToCSharp.Core**: Core library for DXF to C# code generation
- **DxfToCSharp.Compilation**: Compilation services
- **DxfToCSharp.Tool**: Command-line tool (dotnet tool)

### Symbol Packages

- Symbol packages (`.snupkg`) for debugging support

## Required Secrets

To enable full functionality, configure these repository secrets:

### Required

- `NUGET_API_KEY`: API key for publishing packages to NuGet.org
  - Get this from your NuGet.org account settings
  - Required for automatic NuGet package publishing on releases

### Automatically Provided

- `GITHUB_TOKEN`: Automatically provided by GitHub Actions
  - Used for uploading release assets
  - No configuration needed

## Release Process

### Automatic Release

1. Create a new tag following semantic versioning (e.g., `v1.0.0`)
2. Push the tag to GitHub: `git push origin v1.0.0`
3. Create a GitHub release from the tag
4. The CI/CD pipeline will automatically:
   - Run tests
   - Build applications for all platforms
   - Create NuGet packages
   - Upload all artifacts to the release
   - Publish NuGet packages to NuGet.org

### Manual Release

1. Go to GitHub repository → Releases
2. Click "Create a new release"
3. Choose or create a new tag
4. Fill in release notes
5. Publish the release
6. The pipeline will trigger automatically

## Platform-Specific Notes

### macOS

- Creates a proper macOS app bundle (`.app`)
- Packages into a DMG installer for easy distribution
- Supports macOS 10.15 (Catalina) and later
- Uses self-contained deployment with trimming for smaller size

### Windows

- Creates single-file executables for both x64 and x86 architectures
- Self-contained deployment includes .NET runtime
- Uses trimming to reduce file size
- No installation required - just run the executable

### NuGet Packages

- Automatically versioned based on `Directory.Build.props`
- Includes source link for debugging
- Symbol packages published for enhanced debugging experience
- Supports .NET Standard 2.0 for maximum compatibility

## Development Workflow

### Pull Requests

- All PRs to `main` trigger the test job
- Ensures code quality before merging
- No artifacts are built for PRs (saves resources)

### Main Branch

- Pushes to `main` trigger full build pipeline
- Creates artifacts but doesn't publish to NuGet
- Useful for testing the build process

### Releases

- Only releases trigger NuGet publishing
- All artifacts are uploaded to the GitHub release
- Provides complete distribution packages

## Troubleshooting

### Common Issues

1. **NuGet Publishing Fails**
   - Check that `NUGET_API_KEY` secret is set correctly
   - Verify the API key has permission to publish packages
   - Ensure package versions are unique (no duplicates)

2. **macOS Build Fails**
   - Check that the app manifest is valid
   - Verify Avalonia dependencies are compatible with macOS
   - Ensure create-dmg tool installation succeeds

3. **Windows Build Fails**
   - Check that all dependencies support the target runtime
   - Verify single-file publishing settings are correct
   - Ensure trimming doesn't remove required assemblies

### Debugging

1. Check the Actions tab in the GitHub repository
2. Review logs for failed jobs
3. Use the validation workflow to check syntax issues
4. Test locally using the same dotnet commands from the workflow

## Local Testing

To test the build process locally:

```bash
# Test the solution build
dotnet restore DxfToCSharp.sln
dotnet build DxfToCSharp.sln --configuration Release
dotnet test DxfToCSharp.sln --configuration Release

# Test macOS app publishing (on macOS)
dotnet publish DxfToCSharp/DxfToCSharp.csproj \
  --configuration Release \
  --runtime osx-x64 \
  --self-contained true \
  --output ./publish/macos-x64 \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true

# Test Windows app publishing (on Windows or with cross-compilation)
dotnet publish DxfToCSharp/DxfToCSharp.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish/windows-x64 \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true

# Test NuGet package creation
dotnet pack DxfToCSharp.Core/DxfToCSharp.Core.csproj --configuration Release --output ./packages
dotnet pack DxfToCSharp.Tool/DxfToCSharp.Tool.csproj --configuration Release --output ./packages
```

## Contributing

When contributing to this project:

1. Ensure your changes don't break the build pipeline
2. Test locally before submitting PRs
3. Update this documentation if you modify the CI/CD process
4. Follow semantic versioning for releases
# Build script for Windows x86 application
Write-Host "Building DxfToCSharp for Windows x86..."

# Configuration
$PROJECT_PATH = "DxfToCSharp/DxfToCSharp.csproj"
$OUTPUT_DIR = "./publish/windows-x86"
$APP_NAME = "DxfToCSharp"
$ZIP_NAME = "DxfToCSharp-Windows-x86.zip"

# Clean previous builds
Write-Host "Cleaning previous builds..."
if (Test-Path $OUTPUT_DIR) { Remove-Item -Recurse -Force $OUTPUT_DIR }
if (Test-Path $ZIP_NAME) { Remove-Item -Force $ZIP_NAME }

# Restore dependencies
Write-Host "Restoring dependencies..."
dotnet restore

# Publish the application
Write-Host "Publishing application..."
dotnet publish $PROJECT_PATH `
  --configuration Release `
  --runtime win-x86 `
  --self-contained true `
  --output $OUTPUT_DIR `
  -p:PublishSingleFile=true

# Create ZIP archive
Write-Host "Creating ZIP archive..."
Compress-Archive -Path "$OUTPUT_DIR/*" -DestinationPath $ZIP_NAME -Force

Write-Host "Windows x86 build completed successfully!"
Write-Host "Output: $ZIP_NAME"
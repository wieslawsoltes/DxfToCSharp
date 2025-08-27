#!/bin/bash
set -e

# Build script for Windows x64 application
echo "Building DxfToCSharp for Windows x64..."

# Configuration
PROJECT_PATH="DxfToCSharp/DxfToCSharp.csproj"
OUTPUT_DIR="./publish/windows-x64"
APP_NAME="DxfToCSharp"
ZIP_NAME="DxfToCSharp-Windows-x64.zip"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
rm -f "$ZIP_NAME"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Publish the application
echo "Publishing application..."
dotnet publish "$PROJECT_PATH" \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output "$OUTPUT_DIR" \
  -p:PublishSingleFile=true

# Create ZIP archive
echo "Creating ZIP archive..."
cd "$OUTPUT_DIR"
zip -r "../../$ZIP_NAME" .
cd ../..

echo "Windows x64 build completed successfully!"
echo "Output: $ZIP_NAME"
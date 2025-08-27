#!/bin/bash
set -e

# Build script for macOS application
echo "Building DxfToCSharp for macOS..."

# Configuration
PROJECT_PATH="DxfToCSharp/DxfToCSharp.csproj"
OUTPUT_DIR="./publish/macos-x64"
APP_NAME="DxfToCSharp"
APP_BUNDLE="${APP_NAME}.app"
ZIP_NAME="DxfToCSharp-macOS.zip"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
rm -f "$ZIP_NAME"
rm -f "INSTALL_MACOS.txt"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Publish the application
echo "Publishing application..."
dotnet publish "$PROJECT_PATH" \
  --configuration Release \
  --runtime osx-x64 \
  --self-contained true \
  --output "$OUTPUT_DIR" \
  -p:PublishSingleFile=false \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -p:StripSymbols=true \
  -p:IncludeNativeLibrariesForSelfExtract=true

# Create app bundle structure
echo "Creating app bundle..."
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS"
mkdir -p "$OUTPUT_DIR/$APP_BUNDLE/Contents/Resources"

# Copy all files from publish directory to app bundle
cp -R "$OUTPUT_DIR/"* "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/" 2>/dev/null || true
# Remove the app bundle from the copy source to avoid recursion
rm -rf "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/$APP_BUNDLE" 2>/dev/null || true

# Make executable
chmod +x "$OUTPUT_DIR/$APP_BUNDLE/Contents/MacOS/$APP_NAME"

# Create Info.plist
cat > "$OUTPUT_DIR/$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>com.dxftocsharp.app</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# Create installation instructions
cat > "INSTALL_MACOS.txt" << EOF
DxfToCSharp macOS Installation Instructions
==========================================

This application is unsigned and will trigger macOS Gatekeeper warnings.
Here's how to install and run it:

Method 1: Using Finder (Recommended)
1. Extract the ZIP file
2. Right-click on DxfToCSharp.app
3. Select "Open" from the context menu
4. Click "Open" in the security dialog

Method 2: Using Terminal
1. Extract the ZIP file
2. Open Terminal and navigate to the extracted folder
3. Run: xattr -d com.apple.quarantine DxfToCSharp.app
4. Double-click DxfToCSharp.app to run

Method 3: System Preferences
1. Try to open the app (it will be blocked)
2. Go to System Preferences > Security & Privacy > General
3. Click "Open Anyway" next to the blocked app message

Troubleshooting:
- If you see "damaged" errors, use Method 2
- The app may take a moment to start on first launch
- Ensure you have .NET runtime requirements met

Security Note:
This is an unsigned application. Only run if you trust the source.
EOF

# Create ZIP archive
echo "Creating ZIP archive..."
# Ensure publish directory exists
mkdir -p "./publish"
cd "$OUTPUT_DIR"
zip -r "../../publish/$ZIP_NAME" "$APP_BUNDLE"
cd ../..
zip -u "./publish/$ZIP_NAME" "INSTALL_MACOS.txt"
# Copy INSTALL_MACOS.txt to publish directory
cp "INSTALL_MACOS.txt" "./publish/"

echo "macOS build completed successfully!"
echo "Output: ./publish/$ZIP_NAME"
echo "Installation instructions: ./publish/INSTALL_MACOS.txt"
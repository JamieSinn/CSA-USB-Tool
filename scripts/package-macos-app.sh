#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_REL="CSAUSBTool.CrossPlatform.Desktop/CSAUSBTool.CrossPlatform.Desktop.csproj"
PROJECT_PATH="$ROOT_DIR/$PROJECT_REL"
SOLUTION_PATH="$ROOT_DIR/CSAUSBTool.sln"
APP_NAME="CSAUSBTool"
BUNDLE_ID="com.csausbtool.desktop"
FRAMEWORK="net8.0"
CONFIGURATION="Release"
RID=""
ALL_RIDS=0
CREATE_DMG=0

usage() {
  cat <<'EOF'
Usage: scripts/package-macos-app.sh [--rid osx-arm64|osx-x64] [--all] [--configuration Release|Debug] [--dmg]

Builds and packages a macOS .app bundle for CSAUSBTool from the desktop project.

Options:
  --rid            Runtime identifier (default: derived from host CPU, ignored with --all)
  --all            Build and package both osx-arm64 and osx-x64
  --configuration  Build configuration (default: Release)
  --dmg            Also create a .dmg in dist/ for each built runtime
  -h, --help       Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --rid)
      RID="${2:-}"
      shift 2
      ;;
    --all)
      ALL_RIDS=1
      shift
      ;;
    --configuration)
      CONFIGURATION="${2:-}"
      shift 2
      ;;
    --dmg)
      CREATE_DMG=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ "$ALL_RIDS" -eq 1 && -n "$RID" ]]; then
  echo "Use either --all or --rid, not both." >&2
  exit 1
fi

if [[ "$ALL_RIDS" -eq 0 && -z "$RID" ]]; then
  CPU="$(uname -m)"
  case "$CPU" in
    arm64|aarch64) RID="osx-arm64" ;;
    x86_64) RID="osx-x64" ;;
    *)
      echo "Unsupported macOS architecture: $CPU" >&2
      echo "Pass --rid osx-arm64 or --rid osx-x64 explicitly." >&2
      exit 1
      ;;
  esac
fi

DIST_DIR="$ROOT_DIR/dist"
EXECUTABLE_NAME="CSAUSBTool.CrossPlatform.Desktop"
mkdir -p "$DIST_DIR"

if [[ "$ALL_RIDS" -eq 1 ]]; then
  RIDS=("osx-arm64" "osx-x64")
else
  RIDS=("$RID")
fi

echo "Restoring dependencies..."
dotnet restore "$SOLUTION_PATH"

echo "Building solution ($CONFIGURATION)..."
dotnet build "$SOLUTION_PATH" -c "$CONFIGURATION" --no-restore

for target_rid in "${RIDS[@]}"; do
  PUBLISH_DIR="$ROOT_DIR/CSAUSBTool.CrossPlatform.Desktop/bin/$CONFIGURATION/$FRAMEWORK/$target_rid/publish"
  APP_DIR="$DIST_DIR/$APP_NAME-$target_rid.app"
  CONTENTS_DIR="$APP_DIR/Contents"
  MACOS_DIR="$CONTENTS_DIR/MacOS"
  RESOURCES_DIR="$CONTENTS_DIR/Resources"
  EXECUTABLE_PATH="$MACOS_DIR/$EXECUTABLE_NAME"

  echo "Publishing $PROJECT_REL for $target_rid ($CONFIGURATION)..."
  dotnet publish "$PROJECT_PATH" \
    -c "$CONFIGURATION" \
    -r "$target_rid" \
    --self-contained true \
    -p:PublishSingleFile=false

  echo "Creating app bundle at $APP_DIR..."
  rm -rf "$APP_DIR"
  mkdir -p "$MACOS_DIR" "$RESOURCES_DIR"
  cp -R "$PUBLISH_DIR"/. "$MACOS_DIR"/

  cat > "$CONTENTS_DIR/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>
  <key>CFBundleDisplayName</key>
  <string>$APP_NAME</string>
  <key>CFBundleExecutable</key>
  <string>$EXECUTABLE_NAME</string>
  <key>CFBundleIdentifier</key>
  <string>$BUNDLE_ID</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>$APP_NAME</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>11.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
EOF

  chmod +x "$EXECUTABLE_PATH"

  if command -v codesign >/dev/null 2>&1; then
    echo "Applying ad-hoc code signature ($target_rid)..."
    # Ad-hoc signing improves launch reliability for locally built bundles.
    codesign --force --deep --sign - "$APP_DIR"
    codesign --verify --deep --strict "$APP_DIR"
    echo "Code signature verified ($target_rid)."
  fi

  if [[ "$CREATE_DMG" -eq 1 ]]; then
    DMG_PATH="$DIST_DIR/$APP_NAME-$target_rid.dmg"
    echo "Creating DMG at $DMG_PATH..."
    rm -f "$DMG_PATH"
    hdiutil create -volname "$APP_NAME" -srcfolder "$APP_DIR" -ov -format UDZO "$DMG_PATH"
    echo "DMG created: $DMG_PATH"
  fi
done

echo
echo "Done."
for target_rid in "${RIDS[@]}"; do
  echo "App bundle: $DIST_DIR/$APP_NAME-$target_rid.app"
  echo "Run it with: open \"$DIST_DIR/$APP_NAME-$target_rid.app\""
done

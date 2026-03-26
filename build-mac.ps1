# --- Configuration ---
$projectName = "MojiGotchi" 
$appName = "MojiGotchi"
$publishDir = "./publish/mac_raw"
$appFolder = "./publish/$appName.app"
$architecture = "osx-arm64" 

Write-Host "Building Optimized Bundle for $architecture..." -ForegroundColor Cyan

# Clean previous build artifacts
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
if (Test-Path $appFolder) { Remove-Item -Recurse -Force $appFolder }

# --- 1. Execute Build ---
# Target the .csproj specifically to ensure the output directory works 
dotnet publish "$projectName.csproj" -r $architecture -c Release --self-contained true `
	-p:PublishSingleFile=true `
	-p:PublishTrimmed=true `
	-p:IncludeNativeLibrariesForSelfExtract=true `
	-o $publishDir

if (-not $?) { 
	Write-Host "Build Failed. Check if $projectName.csproj exists." -ForegroundColor Red
	Read-Host "Press Enter to exit..."
	exit 
}

# --- 2. Create .app Structure ---
$macOSDir = "$appFolder/Contents/MacOS"
New-Item -ItemType Directory -Path $macOSDir -Force | Out-Null

# --- 3. Create the Launcher Script (play.command) ---
# Use backticks (`) to escape the $ signs so PowerShell doesn't try to run them
$commandContent = @"
#!/bin/sh
HERE=`$(dirname "`$0")
chmod +x "`$HERE/$appName"
"`$HERE/$appName"
"@
$commandContent | Out-File "$macOSDir/play.command" -Encoding ascii

# --- 4. Move Binary and Assets ---
# Move everything from publish to the MacOS folder, excluding .pdb files
Get-ChildItem -Path $publishDir -Exclude "*.pdb", "*.ds_store" | Move-Item -Destination $macOSDir

# --- 5. Create Info.plist ---
# CFBundleExecutable is set to play.command so it triggers the fix on launch
$plistContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>play.command</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourname.$appName</string>
    <key>CFBundleName</key>
    <string>$appName</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.12</string>
</dict>
</plist>
"@
$plistContent | Out-File "$appFolder/Contents/Info.plist" -Encoding utf8

# Clean up raw artifacts
# if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }

Write-Host ""
Write-Host "Success! Bundle created at: $appFolder" -ForegroundColor Green
Write-Host "Instructions for your friend:" -ForegroundColor Yellow
Write-Host "1. Open the DMG/Zip you create."
Write-Host "2. Right-click $appName.app and select 'Open' to bypass the developer warning."
Write-Host ""
Read-Host "Press Enter to exit..."
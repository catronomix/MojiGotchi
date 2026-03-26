# --- Configuration ---
$projectName = "MojiGotchi"
$publishDir = "./publish/linux_raw"
$architecture = "linux-x64"
$outputArchive = "./publish/$projectName-linux.tar.gz"

Write-Host "Starting Optimized Linux Build for $architecture..." -ForegroundColor Cyan

# Clean previous builds
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
if (Test-Path $outputArchive) { Remove-Item -Force $outputArchive }

# --- 1. Execute Build ---
dotnet publish "$projectName.csproj" -r $architecture -c Release --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -p:TrimMode=link `
    -p:EnableCompressionInSingleFile=true `
    -o $publishDir

if (-not $?) {
    Write-Host "Build Failed." -ForegroundColor Red
    Read-Host "Press Enter to exit..."
    exit
}

# --- 2. Create the Linux Launcher (start.sh) ---
# Use backticks (`) to escape the $ signs so PowerShell writes them as text
$shContent = @"
#!/bin/bash
# Get the directory where the script is located
HERE=`$(dirname "`$(readlink -f "`$0")")
cd "`$HERE"
# Ensure the game has execution permissions
chmod +x "./$projectName"
# Run the game
"./$projectName"
"@
$shContent | Out-File "$publishDir/start.sh" -Encoding ascii

# --- 3. Create the Tarball (Excluding Debug Files) ---
Write-Host "Packing into .tar.gz (excluding .pdb)..." -ForegroundColor Cyan

Push-Location $publishDir
# Create tarball excluding the .pdb file
# We use '*' to include all assets, but 'tar' on Windows handles the exclude flag
tar -czf "../../$outputArchive" --exclude="*.pdb" *
Pop-Location

# --- 4. Summary ---
if (Test-Path $outputArchive) {
    $file = Get-Item $outputArchive
    $sizeMB = [math]::Round($file.Length / 1MB, 2)
    
    # Remove-Item -Recurse -Force $publishDir

    Write-Host ""
    Write-Host "Linux Build & Pack Successful!" -ForegroundColor Green
    Write-Host "Archive: $outputArchive"
    Write-Host "Final Size: $sizeMB MB"
    Write-Host "Note: Your friend should run 'bash start.sh' or double-click it." -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Press Enter to exit..."
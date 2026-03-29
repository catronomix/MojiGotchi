# --- Configuration ---
$projectName = "MojiGotchi" 
$outputDir = "./publish/windows"
$architecture = "win-x64"
$zipFile = "./publish/$projectName-windows.zip"

Write-Host "Starting Optimized Windows Build..." -ForegroundColor Cyan

if (Test-Path $outputDir) { Remove-Item -Recurse -Force $outputDir }

# --- Execute Build ---
dotnet publish -r $architecture -c Release --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -p:EnableCompressionInSingleFile=true -o $outputDir

# --- Create Zip File ---
if ($?) {
    # Remove existing zip file if it exists
    if (Test-Path $zipFile) { Remove-Item -Force $zipFile }

    # Create the zip file excluding debug files
    Write-Host "Creating zip file..." -ForegroundColor Cyan
    Compress-Archive -Path "$outputDir/*" -DestinationPath $zipFile -CompressionLevel Optimal -Force

    Write-Host "Zip file created: $zipFile" -ForegroundColor Green
}

# --- Summary ---
if ($?) {
	$file = Get-Item "$outputDir/$projectName.exe"
	$sizeMB = [math]::Round($file.Length / 1MB, 2)
	Write-Host ""
	Write-Host "Windows Build Successful!" -ForegroundColor Green
	Write-Host "Size: $sizeMB MB"
	Write-Host "Location: $outputDir/$projectName.exe"
}
else {
	Write-Host ""
	Write-Host "Build Failed." -ForegroundColor Red
}

Write-Host ""
Read-Host "Press Enter to exit..."
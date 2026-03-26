# --- Configuration ---
$projectName = "MojiGotchi" 
$outputDir = "./publish/windows"
$architecture = "win-x64"

Write-Host "Starting Optimized Windows Build..." -ForegroundColor Cyan

if (Test-Path $outputDir) { Remove-Item -Recurse -Force $outputDir }

# --- Execute Build ---
dotnet publish -r $architecture -c Release --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -p:EnableCompressionInSingleFile=true -o $outputDir

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
# Script to add Windows Defender exclusions for Android Studio
# Run this script as Administrator

Write-Host "Adding Windows Defender exclusions for Android Studio..." -ForegroundColor Cyan

$exclusionPaths = @(
    "C:\Users\DELL\.gradle",
    "C:\Users\DELL\AppData\Local\Android\Sdk",
    "C:\Users\DELL\AppData\Local\Google\AndroidStudio2024.2",
    "C:\Users\DELL\Documents\GitHub\20_HTQLGH\ShipperAndroidApp"
)

foreach ($path in $exclusionPaths) {
    try {
        Add-MpPreference -ExclusionPath $path
        Write-Host "✓ Added exclusion: $path" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed to add: $path" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`nDone! Please restart Android Studio." -ForegroundColor Cyan
Write-Host "`nTo verify exclusions, run:" -ForegroundColor Yellow
Write-Host "Get-MpPreference | Select-Object -ExpandProperty ExclusionPath" -ForegroundColor White

# Pause to see results
Read-Host "`nPress Enter to exit"

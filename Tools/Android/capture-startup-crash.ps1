param(
    [string]$PackageName = "com.DefaultCompany.Myproject",
    [string]$ActivityName = "com.skillz.activity.UnityGameActivity",
    [int]$CaptureSeconds = 20
)

$sdk = "$env:LOCALAPPDATA\Android\Sdk"
$adb = Join-Path $sdk "platform-tools\adb.exe"

if (!(Test-Path $adb)) {
    Write-Error "adb not found at $adb"
    exit 1
}

$devices = & $adb devices
if ($devices -notmatch "\tdevice") {
    Write-Error "No Android device connected. Enable USB debugging and authorize this PC."
    $devices
    exit 1
}

$ts = Get-Date -Format "yyyyMMdd-HHmmss"
$outDir = Join-Path $PSScriptRoot "logs"
$outFile = Join-Path $outDir ("startup-crash-" + $ts + ".txt")
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

& $adb logcat -c | Out-Null
& $adb shell am force-stop $PackageName | Out-Null
& $adb shell am start -n "$PackageName/$ActivityName" | Out-Null

$proc = Start-Process -FilePath $adb -ArgumentList "logcat", "-v", "time" -NoNewWindow -RedirectStandardOutput $outFile -PassThru
Start-Sleep -Seconds $CaptureSeconds

try { Stop-Process -Id $proc.Id -Force } catch {}

Write-Output "Saved log: $outFile"
Write-Output "Key crash lines:"
Select-String -Path $outFile -Pattern "FATAL EXCEPTION|AndroidRuntime|Unity|libc|Abort message|Signal|backtrace|java.lang" -CaseSensitive:$false |
    Select-Object -Last 80 |
    ForEach-Object { $_.Line }

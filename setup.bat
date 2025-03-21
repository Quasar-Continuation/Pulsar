<# ::
@echo off
setlocal enabledelayedexpansion

cd /d %~dp0

powershell -c "iex ((Get-Content '%~f0') -join [Environment]::Newline); iex 'main %*'"
goto :eof
#>

function main {
# Quasar-Modded Installer
$installDir = "$env:APPDATA\Quasar-Modded"
$quasarPath = "$installDir\Quasar.exe"
$clientPath = "$installDir\client.bin"
$shortcutPath = "$env:USERPROFILE\Desktop\Quasar-Modded.lnk"

$server = "https://github.com/Quasar-Continuation/Quasar-Modded/releases/download/AutoBuild/DONT_DOWNLOAD_SERVER.exe"
$client = "https://github.com/Quasar-Continuation/Quasar-Modded/releases/download/AutoBuild/DONT_DOWNLOAD_CLIENT.bin"

function Print-Center($text, $addEquals = $true) {
    $toAdd = if ($addEquals) { "=" } else { " " }

    $windowWidth = $Host.UI.RawUI.WindowSize.Width
    $padding = [math]::Max(0, [math]::Floor(($windowWidth - $text.Length) / 2))
    $leftPadding = $toAdd * $padding
    Write-Host $leftPadding -NoNewline -ForegroundColor Cyan
    Write-Host $text -NoNewline -ForegroundColor Cyan
    Write-Host ($toAdd * ($windowWidth - $padding - $text.Length)) -ForegroundColor Cyan
}

Print-Center "="
Print-Center "Quasar-Modded Installer" $false
Print-Center "="

function Get-FileSize($url) {
    try {
        return [int](Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing).Headers["Content-Length"]
    } catch {
        Write-Host "Error retrieving file size for: $url" -ForegroundColor Red
        return $null
    }
}

function Terminate-Process($processName) {
    $process = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "Terminating $processName..."
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
}

function Download-File($url, $destination, $expectedSize) {
    try {
        $webClient = New-Object System.Net.WebClient
        Write-Host "Downloading: $url -> $destination"

        # Create directory if it doesn't exist
        $directory = Split-Path -Parent $destination
        if (!(Test-Path $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }

        $tempPath = "$destination.tmp"
        $downloadTimer = [System.Diagnostics.Stopwatch]::StartNew()

        $webClient.DownloadFile($url, $tempPath)

        $downloadTimer.Stop()
        if ((Test-Path $tempPath) -and ((Get-Item $tempPath).Length -eq $expectedSize)) {
            Move-Item -Force $tempPath $destination
            Write-Host "Download complete: $destination ($($expectedSize) bytes) in $([math]::Round($downloadTimer.Elapsed.TotalSeconds, 2))s"
            return $true
        } else {
            Write-Host "Download failed or file size mismatch for $destination" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "Error downloading: $url" -ForegroundColor Red
        return $false
    }
}

# Ensure installation directory exists
if (!(Test-Path $installDir)) {
    Write-Host "Creating installation directory: $installDir"
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Set working dir to install dir
Set-Location $installDir

# Terminate running instance if exists
Terminate-Process "Quasar"

# Get file sizes from server
$quasarBytes = Get-FileSize $server
$clientBytes = Get-FileSize $client

if ($quasarBytes -and $clientBytes) {
    Write-Host "Quasar Size: $quasarBytes bytes"
    Write-Host "Client Size: $clientBytes bytes"
    
    # Check if Quasar needs update
    $updateQuasar = $false
    if (!(Test-Path $quasarPath) -or ((Get-Item $quasarPath).Length -ne $quasarBytes)) {
        $updateQuasar = $true
        if (Test-Path $quasarPath) {
            $localSize = (Get-Item $quasarPath).Length
            $diff = $quasarBytes - $localSize
            $diffText = if ($diff -gt 0) { "+$diff bytes" } else { "$diff bytes" }
            
            Write-Host "Local Quasar.exe size: $localSize bytes"
            Write-Host "Server Quasar.exe size: $quasarBytes bytes"
            Write-Host "Updating Quasar ($diffText)"
        } else {
            Write-Host "Installing Quasar.exe"
        }
        
        if (!(Download-File $server $quasarPath $quasarBytes)) {
            Write-Host "Failed to download Quasar.exe" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Quasar.exe is up to date" -ForegroundColor Green
    }

    # Check if client needs update
    $updateClient = $false
    if (!(Test-Path $clientPath) -or ((Get-Item $clientPath).Length -ne $clientBytes)) {
        $updateClient = $true
        if (Test-Path $clientPath) {
            $localSize = (Get-Item $clientPath).Length
            $diff = $clientBytes - $localSize
            $diffText = if ($diff -gt 0) { "+$diff bytes" } else { "$diff bytes" }
            Write-Host "Updating client.bin ($diffText)"
        } else {
            Write-Host "Installing client.bin"
        }
        
        if (!(Download-File $client $clientPath $clientBytes)) {
            Write-Host "Failed to download client.bin" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "client.bin is up to date" -ForegroundColor Green
    }

    # Start Quasar if installed successfully
    if (Test-Path $quasarPath) {
        Write-Host "Installation complete!" -ForegroundColor Green
        Start-Process $quasarPath
    } else {
        Write-Host "Quasar-Modded installation failed." -ForegroundColor Red
    }
} else {
    Write-Host "Failed to retrieve file sizes. Check your internet connection or the URLs." -ForegroundColor Red
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

}
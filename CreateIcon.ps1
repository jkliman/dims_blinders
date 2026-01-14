# Create a simple icon for the application
Add-Type -AssemblyName System.Drawing

$size = 32
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Draw background
$graphics.FillRectangle([System.Drawing.Brushes]::Black, 0, 0, $size, $size)

# Draw three monitor representation
$monitorWidth = 8
$monitorHeight = 20
$gap = 2
$startX = ($size - (3 * $monitorWidth + 2 * $gap)) / 2
$startY = ($size - $monitorHeight) / 2

# Left monitor (dark)
$graphics.FillRectangle([System.Drawing.Brushes]::DarkGray, $startX, $startY, $monitorWidth, $monitorHeight)

# Center monitor (bright - active)
$graphics.FillRectangle([System.Drawing.Brushes]::Cyan, $startX + $monitorWidth + $gap, $startY, $monitorWidth, $monitorHeight)

# Right monitor (dark)
$graphics.FillRectangle([System.Drawing.Brushes]::DarkGray, $startX + 2*($monitorWidth + $gap), $startY, $monitorWidth, $monitorHeight)

# Save as icon
$iconPath = Join-Path $PSScriptRoot "icon.ico"
$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())

$fs = [System.IO.File]::Create($iconPath)
$icon.Save($fs)
$fs.Close()

$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Icon created at: $iconPath"

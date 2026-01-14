Add-Type -AssemblyName System.Drawing

# Create banner (493x58)
$banner = New-Object System.Drawing.Bitmap(493, 58)
$g = [System.Drawing.Graphics]::FromImage($banner)
$g.Clear([System.Drawing.Color]::FromArgb(30, 30, 30))
$font = New-Object System.Drawing.Font('Segoe UI', 18, [System.Drawing.FontStyle]::Bold)
$g.DrawString('DIMS Blinders', $font, [System.Drawing.Brushes]::White, 10, 12)
$g.Dispose()
$banner.Save("$PSScriptRoot\banner.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
$banner.Dispose()

# Create dialog background (493x312)
$dialog = New-Object System.Drawing.Bitmap(493, 312)
$g = [System.Drawing.Graphics]::FromImage($dialog)
$g.Clear([System.Drawing.Color]::FromArgb(45, 45, 45))
$font = New-Object System.Drawing.Font('Segoe UI', 24, [System.Drawing.FontStyle]::Bold)
$g.DrawString('DIMS', $font, [System.Drawing.Brushes]::White, 20, 100)
$font2 = New-Object System.Drawing.Font('Segoe UI', 14)
$g.DrawString('Blinders', $font2, [System.Drawing.Brushes]::Gray, 22, 140)
$g.Dispose()
$dialog.Save("$PSScriptRoot\dialog.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
$dialog.Dispose()

Write-Host "Images created successfully"

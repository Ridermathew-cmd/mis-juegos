Add-Type -AssemblyName System.Drawing

$w = 900
$h = 700
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

# --- Cielo: degrade vertical ---
$skyTop = [System.Drawing.Color]::FromArgb(255, 90, 160, 230)
$skyBottom = [System.Drawing.Color]::FromArgb(255, 190, 225, 245)
$skyRect = New-Object System.Drawing.Rectangle 0, 0, $w, $h
$skyBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $skyRect, $skyTop, $skyBottom, 90
$g.FillRectangle($skyBrush, $skyRect)

$block = 18

function Fill-Block($gfx, $bx, $by, $bw, $bh, $color) {
    $brush = New-Object System.Drawing.SolidBrush $color
    $gfx.FillRectangle($brush, [int]($bx * $block), [int]($by * $block), [int]($bw * $block), [int]($bh * $block))
    $brush.Dispose()
}

# --- Sol en bloques ---
$sunColor = [System.Drawing.Color]::FromArgb(255, 255, 236, 150)
Fill-Block $g 34 3 4 4 $sunColor

# --- Nubes en bloques ---
$cloudColor = [System.Drawing.Color]::FromArgb(230, 255, 255, 255)
Fill-Block $g 4 5 5 2 $cloudColor
Fill-Block $g 5 4 3 1 $cloudColor
Fill-Block $g 20 8 6 2 $cloudColor
Fill-Block $g 21 7 3 1 $cloudColor
Fill-Block $g 40 4 5 2 $cloudColor

# --- Colinas de fondo (mas claras, mas lejos) ---
$hillFar = [System.Drawing.Color]::FromArgb(255, 120, 190, 110)
$cols = [int]($w / $block)
$farHeights = @(14,14,15,15,16,16,17,17,16,16,15,15,16,17,18,18,17,16,15,15,14,14,15,16,17,17,16,15,14,14,15,16,17,18,19,19,18,17,16,15,14,14,15,16,17,17,16,15,14,14,15)
for ($i = 0; $i -lt $cols; $i++) {
    $hgt = $farHeights[$i % $farHeights.Length]
    Fill-Block $g $i $hgt 1 ([int]($h/$block) - $hgt) $hillFar
}

# --- Colinas de frente (mas oscuras, mas cerca) ---
$hillNear = [System.Drawing.Color]::FromArgb(255, 75, 160, 75)
$nearHeights = @(20,20,19,19,20,21,21,20,19,19,20,21,22,22,21,20,19,19,20,21,21,20,19,19,20,21,22,22,21,20,19,19,20,20,19,19,20,21,21,20,19,19,20,21,22,22,21,20,19,19)
for ($i = 0; $i -lt $cols; $i++) {
    $hgt = $nearHeights[$i % $nearHeights.Length]
    Fill-Block $g $i $hgt 1 ([int]($h/$block) - $hgt) $hillNear
}

# --- Arboles en bloques (troncos + copas) ---
$trunkColor = [System.Drawing.Color]::FromArgb(255, 110, 80, 50)
$leafColor = [System.Drawing.Color]::FromArgb(255, 55, 130, 55)
$treeX = @(6, 15, 26, 33, 44)
foreach ($tx in $treeX) {
    $baseY = 21
    Fill-Block $g $tx $baseY 1 3 $trunkColor
    Fill-Block $g ($tx - 1) ($baseY - 2) 3 1 $leafColor
    Fill-Block $g ($tx - 1) ($baseY - 3) 3 1 $leafColor
    Fill-Block $g $tx ($baseY - 4) 1 1 $leafColor
}

$g.Dispose()

# --- Blur suave (box blur, 2 pasadas) para efecto vidrio esmerilado ---
function Apply-BoxBlur($bitmap, $radius) {
    $width = $bitmap.Width
    $height = $bitmap.Height
    $src = $bitmap.Clone()
    for ($pass = 0; $pass -lt 2; $pass++) {
        $copy = $src.Clone()
        for ($y = 0; $y -lt $height; $y += 3) {
            for ($x = 0; $x -lt $width; $x += 3) {
                $rSum = 0; $gSum = 0; $bSum = 0; $count = 0
                for ($dy = -$radius; $dy -le $radius; $dy += 3) {
                    for ($dx = -$radius; $dx -le $radius; $dx += 3) {
                        $nx = $x + $dx
                        $ny = $y + $dy
                        if ($nx -ge 0 -and $nx -lt $width -and $ny -ge 0 -and $ny -lt $height) {
                            $px = $copy.GetPixel($nx, $ny)
                            $rSum += $px.R; $gSum += $px.G; $bSum += $px.B
                            $count++
                        }
                    }
                }
                $avgColor = [System.Drawing.Color]::FromArgb(255, [int]($rSum/$count), [int]($gSum/$count), [int]($bSum/$count))
                for ($fy = 0; $fy -lt 3 -and ($y+$fy) -lt $height; $fy++) {
                    for ($fx = 0; $fx -lt 3 -and ($x+$fx) -lt $width; $fx++) {
                        $src.SetPixel($x+$fx, $y+$fy, $avgColor)
                    }
                }
            }
        }
        $copy.Dispose()
    }
    return $src
}

Write-Output "Aplicando blur..."
$blurred = Apply-BoxBlur $bmp 24
$bmp.Dispose()

# --- Overlay oscuro semi-transparente para que el texto sea legible ---
$g2 = [System.Drawing.Graphics]::FromImage($blurred)
$overlay = [System.Drawing.Color]::FromArgb(140, 8, 10, 14)
$overlayBrush = New-Object System.Drawing.SolidBrush $overlay
$g2.FillRectangle($overlayBrush, 0, 0, $w, $h)
$g2.Dispose()

$outPath = "C:\Users\Administrator\Documents\mateo_claude\minecraft-launcher\Assets\background.png"
$blurred.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$blurred.Dispose()
Write-Output "Guardado: $outPath"

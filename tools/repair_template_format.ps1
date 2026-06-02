$ErrorActionPreference = "Stop"

$OutDir = Join-Path (Split-Path -Parent $PSScriptRoot) "BaoCaoTheoMauGoc"
$ZipPath = Join-Path (Split-Path -Parent $PSScriptRoot) "BaoCaoTheoMauGoc_QuanLyNhanSuWpf.zip"

function Clean-Text([string]$Text) {
    return (($Text -replace "`r|`a|\x07", "").Trim())
}

function Get-WordApplication {
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    return $word
}

function Set-StyleSafe($Doc, [string]$Name, [string]$FontName, [double]$FontSize, [int]$Alignment) {
    try {
        $style = $Doc.Styles.Item($Name)
        $style.Font.Name = $FontName
        $style.Font.Size = $FontSize
        $style.Font.Spacing = 0
        $style.Font.Scaling = 100
        $style.Font.Kerning = 0
        $style.ParagraphFormat.Alignment = $Alignment
    }
    catch {}
}

function Find-BodyStartByPrefix($Doc, [string]$Prefix, [int]$AfterStart = 0) {
    foreach ($p in @($Doc.Paragraphs)) {
        if ($p.Range.Start -lt $AfterStart) { continue }
        $text = Clean-Text $p.Range.Text
        $styleName = ""
        try { $styleName = [string]$p.Range.Style.NameLocal } catch {}
        if ($styleName -eq "Heading 1" -and $text.StartsWith($Prefix)) {
            return $p.Range.Start
        }
    }

    foreach ($p in @($Doc.Paragraphs)) {
        if ($p.Range.Start -lt $AfterStart) { continue }
        $text = Clean-Text $p.Range.Text
        $styleName = ""
        try { $styleName = [string]$p.Range.Style.NameLocal } catch {}
        if ($text -eq $Prefix -and $styleName -notlike "TOC*") {
            return $p.Range.Start
        }
    }

    foreach ($p in @($Doc.Paragraphs)) {
        if ($p.Range.Start -lt $AfterStart) { continue }
        $text = Clean-Text $p.Range.Text
        $styleName = ""
        try { $styleName = [string]$p.Range.Style.NameLocal } catch {}
        if ($text.StartsWith($Prefix) -and $text -notmatch "`t\d+$" -and $styleName -notlike "TOC*") {
            return $p.Range.Start
        }
    }
    return $null
}

function Rebuild-Toc($Doc, [string]$BodyPrefix) {
    $tocPara = $null
    foreach ($p in @($Doc.Paragraphs)) {
        if ((Clean-Text $p.Range.Text) -eq "MỤC LỤC") {
            $tocPara = $p
            break
        }
    }

    if ($null -eq $tocPara) { return }
    $bodyStart = Find-BodyStartByPrefix $Doc $BodyPrefix $tocPara.Range.End
    if ($null -eq $bodyStart) { return }

    $deleteRange = $Doc.Range($tocPara.Range.End, $bodyStart)
    try { $deleteRange.Delete() | Out-Null } catch {}

    $tocRange = $Doc.Range($tocPara.Range.End, $tocPara.Range.End)
    try {
        $field = $Doc.Fields.Add($tocRange, 13, ' \o "1-3" \h \z \u', $true)
        $field.Update() | Out-Null
    }
    catch {
        try {
            $Doc.TablesOfContents.Add($tocRange, $true, 1, 3, $true, "", $true, $true, $true, $true, $true) | Out-Null
            foreach ($toc in @($Doc.TablesOfContents)) { $toc.Update() | Out-Null }
        }
        catch {}
    }
}

function Repair-Document($Doc, [string]$BodyPrefix) {
    # Reset inherited expanded/distributed character formatting that came from the legacy .doc template.
    try {
        $Doc.Content.Font.Spacing = 0
        $Doc.Content.Font.Scaling = 100
        $Doc.Content.Font.Kerning = 0
    }
    catch {}

    Set-StyleSafe $Doc "Normal" "Times New Roman" 14 0
    Set-StyleSafe $Doc "No Spacing" "Times New Roman" 14 0
    Set-StyleSafe $Doc "Heading 1" "Times New Roman" 14 0
    Set-StyleSafe $Doc "Heading 2" "Times New Roman" 13 0
    Set-StyleSafe $Doc "Heading 3" "Times New Roman" 13 0
    Set-StyleSafe $Doc "TOC 1" "Times New Roman" 12 0
    Set-StyleSafe $Doc "TOC 2" "Times New Roman" 12 0
    Set-StyleSafe $Doc "TOC 3" "Times New Roman" 12 0

    Rebuild-Toc $Doc $BodyPrefix

    $bodyStart = Find-BodyStartByPrefix $Doc $BodyPrefix 0
    foreach ($p in @($Doc.Paragraphs)) {
        try {
            $p.Range.Font.Spacing = 0
            $p.Range.Font.Scaling = 100
            $p.Range.Font.Kerning = 0
        }
        catch {}

        if ($null -ne $bodyStart -and $p.Range.Start -ge $bodyStart) {
            try {
                $p.Format.Alignment = 0
                $p.Format.CharacterUnitLeftIndent = 0
                $p.Format.CharacterUnitRightIndent = 0
                $p.Format.CharacterUnitFirstLineIndent = 0
            }
            catch {}
        }
    }

    foreach ($table in @($Doc.Tables)) {
        foreach ($cell in @($table.Range.Cells)) {
            try {
                $cell.Range.Font.Spacing = 0
                $cell.Range.Font.Scaling = 100
                $cell.Range.Font.Kerning = 0
                foreach ($p in @($cell.Range.Paragraphs)) {
                    $p.Format.Alignment = 0
                }
            }
            catch {}
        }
    }

    try {
        foreach ($toc in @($Doc.TablesOfContents)) {
            $toc.Update() | Out-Null
        }
    }
    catch {}
}

$word = Get-WordApplication
try {
    $word.DisplayAlerts = 0
    $openDocs = @{}
    foreach ($d in @($word.Documents)) {
        try { $openDocs[$d.FullName.ToLowerInvariant()] = $d } catch {}
    }

    $files = Get-ChildItem -LiteralPath $OutDir -Filter "*.doc" | Where-Object { $_.Name -notlike "~*" }
    foreach ($file in $files) {
        $bodyPrefix = if ($file.Name -like "KeHoach_*") { "I. MỤC TIÊU, PHẠM VI DỰ ÁN" } else { "1. GIỚI THIỆU" }
        $key = $file.FullName.ToLowerInvariant()
        $wasOpen = $openDocs.ContainsKey($key)
        if ($wasOpen) {
            $doc = $openDocs[$key]
        }
        else {
            $doc = $word.Documents.Open($file.FullName, $false, $false)
        }

        Repair-Document $doc $bodyPrefix
        $doc.Save()

        if (-not $wasOpen) {
            $doc.Close($false)
        }
    }
}
finally {
    $word.Quit()
}

Get-ChildItem -LiteralPath $OutDir -Filter "~*" -Force | Remove-Item -Force -ErrorAction SilentlyContinue
if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
$zipItems = Get-ChildItem -LiteralPath $OutDir -Force | Where-Object { $_.Name -notlike "~*" } | Select-Object -ExpandProperty FullName
Compress-Archive -Path $zipItems -DestinationPath $ZipPath -Force

Write-Host "Repaired formatting and rebuilt zip: $ZipPath"

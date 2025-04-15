param (
    [string]$SourcePath = "C:\Projects\StThomasMission",
    [string]$OutputPath = "C:\Projects\StThomasMission\MyOwnCode.txt"
)

Write-Output "Exporting only my own code from $SourcePath ..."

# Include only these folders
$includeFolders = @(
    "StThomasMission.Core",
    "StThomasMission.Infrastructure",
    "StThomasMission.Services",
    "StThomasMission.Web"
)

$docContent = ""

foreach ($folder in $includeFolders) {
    $fullPath = Join-Path $SourcePath $folder
    $files = Get-ChildItem -Path $fullPath -Recurse -Include *.cs, *.cshtml
    
    foreach ($file in $files) {
        $docContent += "`n============================================================`n"
        $docContent += "File: $($file.FullName)`n"
        $docContent += "============================================================`n"
        $docContent += Get-Content $file.FullName -Raw
        $docContent += "`n`n"
    }
}

# Export to Text File
Set-Content -Path $OutputPath -Value $docContent

Write-Output "Export Completed Successfully to $OutputPath"

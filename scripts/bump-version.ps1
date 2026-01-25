param(
    [string]$VersionFile = (Join-Path $PSScriptRoot '..\\Directory.Build.props'),
    [string]$PropertyName = 'Version'
)

if (-not (Test-Path -LiteralPath $VersionFile)) {
    throw "Version file not found: $VersionFile"
}

$content = Get-Content -LiteralPath $VersionFile -Raw
$pattern = "<$PropertyName>(?<version>[^<]+)</$PropertyName>"
$match = [regex]::Match($content, $pattern)
if (-not $match.Success) {
    throw "Property <$PropertyName> not found in $VersionFile"
}

$oldVersion = $match.Groups['version'].Value.Trim()
$versionMatch = [regex]::Match($oldVersion, '^(?<base>\d+(\.\d+){1,3})(?<suffix>[-+].+)?$')
if (-not $versionMatch.Success) {
    throw "Version '$oldVersion' must be in Major.Minor[.Patch[.Revision]] format."
}

$baseVersion = $versionMatch.Groups['base'].Value
$suffix = $versionMatch.Groups['suffix'].Value

$parsed = $null
if (-not [Version]::TryParse($baseVersion, [ref]$parsed)) {
    throw "Version '$oldVersion' is not a valid numeric version."
}

[int]$major = $parsed.Major
[int]$minor = $parsed.Minor + 1
$build = if ($parsed.Build -ge 0) { 0 } else { -1 }
$revision = if ($parsed.Revision -ge 0) { 0 } else { -1 }

if ($revision -ge 0) {
    $newVersion = "$major.$minor.$build.$revision"
} elseif ($build -ge 0) {
    $newVersion = "$major.$minor.$build"
} else {
    $newVersion = "$major.$minor"
}

if (-not [string]::IsNullOrEmpty($suffix)) {
    $newVersion = "$newVersion$suffix"
}

$newContent = [regex]::Replace($content, $pattern, "<$PropertyName>$newVersion</$PropertyName>", 1)
Set-Content -LiteralPath $VersionFile -Value $newContent -Encoding ASCII

Write-Host "Bumped $($PropertyName): $oldVersion -> $newVersion"

# Define the root folder of your solution (script location)
$rootPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Define the output folder where all .nupkg files will be collected
$outputFolder = Join-Path -Path $rootPath -ChildPath "CollectedPackagesDebug"

# Create the output folder if it doesn't exist
if (-not (Test-Path $outputFolder)) {
    New-Item -Path $outputFolder -ItemType Directory | Out-Null
}

# Restore once to avoid repeated restores per project
$solutionPath = Join-Path -Path $rootPath -ChildPath "OncoSharp.sln"
if (Test-Path $solutionPath) {
    Write-Host "Restoring: $solutionPath"
    dotnet restore $solutionPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Restore failed for solution."
        exit 1
    }
} else {
    Write-Host "Restoring projects under: $rootPath"
    dotnet restore $rootPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Restore failed for projects."
        exit 1
    }
}

# Find all projects and pack each one into the output folder
$projects = Get-ChildItem -Path $rootPath -Recurse -Filter *.csproj -File
if ($projects.Count -eq 0) {
    Write-Host "No .csproj files found."
    exit 0
}

$failed = @()
foreach ($project in $projects) {
    Write-Host "Packing (Debug): $($project.FullName)"
    dotnet pack $project.FullName -c Debug -o $outputFolder --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed: $($project.FullName)"
        $failed += $project.FullName
    }
}

if ($failed.Count -gt 0) {
    Write-Host "`nPackaging failed for $($failed.Count) project(s):"
    $failed | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host "`nDone. All Debug packages are in: $outputFolder"

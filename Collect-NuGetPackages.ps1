# Define the root folder of your solution
$rootPath = "C:\vsprojects\OncoSharp"

# Define the output folder where all .nupkg files will be collected
$outputFolder = Join-Path -Path $rootPath -ChildPath "CollectedPackages"

# Create the output folder if it doesn't exist
if (-not (Test-Path $outputFolder)) {
    New-Item -Path $outputFolder -ItemType Directory | Out-Null
}

# Search for all .nupkg files in the solution (recursively), limit to Release folders
$nupkgFiles = Get-ChildItem -Path $rootPath -Recurse -Include *.nupkg |
              Where-Object { $_.FullName -like "*\bin\Release\*" }

# Copy each found .nupkg file to the output folder
foreach ($file in $nupkgFiles) {
    $destination = Join-Path -Path $outputFolder -ChildPath $file.Name

    # Overwrite if file already exists
    Copy-Item -Path $file.FullName -Destination $destination -Force
    Write-Host "Copied: $($file.FullName) -> $destination"
}

Write-Host "`nDone. All .nupkg files collected into: $outputFolder"

param(
    [string[]]$RepoIds = @(),
    [switch]$SkipClone
)

$ErrorActionPreference = "Stop"
$Root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (-not (Test-Path (Join-Path $Root "Assets"))) {
    $Root = Split-Path $PSScriptRoot -Parent
}
if (-not (Test-Path (Join-Path $Root "Assets"))) {
    throw "Cannot locate Unity project root from $PSScriptRoot"
}

$ManifestPath = Join-Path $PSScriptRoot "manifest.json"
$Manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
$CacheDir = Join-Path $PSScriptRoot ".cache"
$DestRoot = Join-Path $Root "Assets\_Project\Resources\Art\ThirdParty"
New-Item -ItemType Directory -Force -Path $CacheDir, $DestRoot | Out-Null

$Selected = @($Manifest.repos)
if ($RepoIds.Count -gt 0) {
    $Selected = @($Manifest.repos | Where-Object { $RepoIds -contains $_.id })
}

function Copy-ArtFiles {
    param($SourceRoot, $DestDir, $SourcePaths, $Extensions)
    $copied = 0
    foreach ($rel in $SourcePaths) {
        $folder = Join-Path $SourceRoot $rel
        if (-not (Test-Path $folder)) { continue }
        Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
            $ext = $_.Extension.ToLowerInvariant()
            if ($Extensions -notcontains $ext) { return }
            $relative = $_.FullName.Substring($folder.Length).TrimStart('\', '/')
            $target = Join-Path $DestDir $relative
            $targetParent = Split-Path $target -Parent
            New-Item -ItemType Directory -Force -Path $targetParent | Out-Null
            Copy-Item -Path $_.FullName -Destination $target -Force
            $script:copied++
        }
    }
    return $copied
}

foreach ($repo in $Selected) {
    Write-Host "== $($repo.id) ==" -ForegroundColor Cyan
    $clonePath = Join-Path $CacheDir $repo.id
    if (-not $SkipClone) {
        if (Test-Path $clonePath) { Remove-Item -Recurse -Force $clonePath }
        git clone --depth 1 $repo.url $clonePath
    }
    elseif (-not (Test-Path $clonePath)) {
        Write-Warning "Cache missing for $($repo.id); run without -SkipClone"
        continue
    }

    $dest = Join-Path $DestRoot $repo.id
    if (Test-Path $dest) { Remove-Item -Recurse -Force $dest }
    New-Item -ItemType Directory -Force -Path $dest | Out-Null

    $count = Copy-ArtFiles -SourceRoot $clonePath -DestDir $dest -SourcePaths $repo.sourcePaths -Extensions $repo.extensions
    Write-Host "Copied $count art files -> $dest"
}

Write-Host "Done. Open Unity and run: ADHD Training -> Link Art Resource Bindings" -ForegroundColor Green

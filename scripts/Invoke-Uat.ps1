param(
    [ValidateSet('Run', 'Migrate', 'Seed')]
    [string]$Action = 'Run'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$environmentFile = Join-Path $projectRoot '.env.uat'
if (-not (Test-Path -LiteralPath $environmentFile)) {
    throw 'Configure the git-ignored .env.uat file before running UAT.'
}

$previousValues = @{}
Push-Location $projectRoot
try {
    foreach ($line in Get-Content -LiteralPath $environmentFile) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) { continue }
        $entry = $line -split '=', 2
        if ($entry.Count -ne 2) { throw 'Invalid UAT environment entry.' }
        $name = $entry[0].Trim()
        $previousValues[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
        [Environment]::SetEnvironmentVariable($name, $entry[1], 'Process')
    }
    if ($Action -eq 'Migrate') {
        # Schema operations use Neon's direct endpoint; application traffic stays pooled.
        $env:ConnectionStrings__Postgres = $env:ConnectionStrings__Postgres.Replace('-pooler.', '.')
        dotnet ef database update --project backend/src/MyFundex.Api --context DevBootstrapDbContext
    }
    elseif ($Action -eq 'Seed') {
        $env:ConnectionStrings__Postgres = $env:ConnectionStrings__Postgres.Replace('-pooler.', '.')
        dotnet run --project backend/src/MyFundex.Api --no-launch-profile -- --seed-defaults
    }
    else {
        dotnet run --project backend/src/MyFundex.Api
    }
    if ($LASTEXITCODE -ne 0) { throw "UAT $Action failed (exit $LASTEXITCODE)." }
}
finally {
    foreach ($name in $previousValues.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previousValues[$name], 'Process')
    }
    Pop-Location
}

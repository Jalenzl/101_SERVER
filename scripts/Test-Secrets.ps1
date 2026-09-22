$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).Replace('\', '/')
$files = @(git -c "safe.directory=$repositoryRoot" ls-files)
if ($LASTEXITCODE -ne 0) { throw 'git ls-files failed.' }
$findings = [System.Collections.Generic.List[string]]::new()

foreach ($relativePath in $files) {
    if (-not (Test-Path -LiteralPath $relativePath -PathType Leaf)) { continue }
    $leaf = Split-Path -Leaf $relativePath
    $extension = [IO.Path]::GetExtension($relativePath).ToLowerInvariant()
    if ($leaf -eq '.env' -or $leaf -eq 'appsettings.Production.json' -or $extension -in @('.db', '.pfx', '.p12')) {
        $findings.Add("$relativePath [forbidden artifact]")
        continue
    }

    $content = Get-Content -LiteralPath $relativePath -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }
    $privateKeyPattern = '-----BEGIN (?:RSA |EC |OPENSSH )?' + 'PRIVATE KEY-----\s*\r?\n[A-Za-z0-9+/=]{32,}'
    if ([regex]::IsMatch($content, $privateKeyPattern)) {
        $findings.Add("$relativePath [private key block]")
    }
    $tcpPasswordPattern = '(?im)^TCP101_DB_' + 'PASSWORD[ \t]*=[ \t]*(?!<)[^ \t\r\n#]+'
    if ([regex]::IsMatch($content, $tcpPasswordPattern)) {
        $findings.Add("$relativePath [TCP101 database password value]")
    }
    $connectionPattern = '(?i)Host\s*=\s*(?!<)[^;\r\n]+;[^\r\n]*Pass' + 'word\s*=\s*(?!<)[^;`\r\n]+'
    if ([regex]::IsMatch($content, $connectionPattern)) {
        $findings.Add("$relativePath [password-bearing connection string]")
    }
    if ($extension -notin @('.md', '.markdown')) {
        $literalPasswordPattern = '(?i)(?<![A-Za-z0-9_])Pass' + 'word\s*=\s*["''][^"'']+["'']'
        if ([regex]::IsMatch($content, $literalPasswordPattern)) {
            $findings.Add("$relativePath [literal password assignment]")
        }
    }
}

if ($findings.Count -gt 0) {
    $findings | Sort-Object -Unique | ForEach-Object { [Console]::Error.WriteLine($_) }
    exit 1
}

Write-Output "Secret and forbidden-artifact scan passed for $($files.Count) tracked files."

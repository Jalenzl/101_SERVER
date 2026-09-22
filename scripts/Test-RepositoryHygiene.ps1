$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$forbiddenDirectories = @(
    '511', 'ISIP', 'Plugins',
    'Admin.NET.Web.Entry/PythonAlgorithm',
    'Admin.NET.Core/FFMpegCore',
    'Admin.NET.Web.Core/fonts'
)
$forbiddenFiles = @('*.db', '*.pdmodel', '*.pdiparams', 'ffmpeg.exe', 'ffprobe.exe')
$forbiddenPackages = @(
    'AlibabaCloud.SDK.Dysmsapi20170525',
    'AspNet.Security.OAuth.Gitee',
    'AspNet.Security.OAuth.Weixin',
    'FFMpegCore', 'NEST', 'OnceMi.AspNetCore.OSS',
    'SixLabors.ImageSharp.Web', 'SKIT.FlurlHttpClient.Wechat.Api',
    'SKIT.FlurlHttpClient.Wechat.TenpayV3'
)
$requiredProjects = @(
    'Admin.NET.Core/Admin.NET.Core.csproj',
    'Admin.NET.Application/Admin.NET.Application.csproj',
    'Admin.NET.Web.Core/Admin.NET.Web.Core.csproj',
    'Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj'
)

$errors = [System.Collections.Generic.List[string]]::new()
foreach ($relative in $requiredProjects) {
    if (-not (Test-Path (Join-Path $repo $relative))) { $errors.Add("Missing required project: $relative") }
}
foreach ($relative in $forbiddenDirectories) {
    if (Test-Path (Join-Path $repo $relative)) { $errors.Add("Forbidden directory: $relative") }
}
foreach ($pattern in $forbiddenFiles) {
    Get-ChildItem $repo -Recurse -File -Filter $pattern -ErrorAction SilentlyContinue |
        ForEach-Object { $errors.Add("Forbidden file: $($_.FullName.Substring($repo.Length + 1))") }
}
Get-ChildItem $repo -Recurse -File -Filter *.csproj | ForEach-Object {
    $content = Get-Content -Raw $_.FullName
    foreach ($package in $forbiddenPackages) {
        if ($content -match [regex]::Escape($package)) {
            $errors.Add("Forbidden package $package in $($_.Name)")
        }
    }
}
if ($errors.Count -gt 0) {
    $errors | ForEach-Object { [Console]::Error.WriteLine($_) }
    exit 1
}
Write-Output 'Repository hygiene checks passed.'

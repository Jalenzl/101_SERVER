param(
    [string]$BaseUrl = 'http://localhost:5000',
    [switch]$SkipLogin
)

$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')

function Invoke-Tcp101Request {
    param([string]$Method, [string]$Path, [object]$Body, [hashtable]$Headers = @{}, [string]$Label)
    $parameters = @{ Uri = "$base$Path"; Method = $Method; Headers = $Headers; SkipHttpErrorCheck = $true }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = $Body | ConvertTo-Json -Depth 20 -Compress
    }
    $response = Invoke-WebRequest @parameters
    if ([int]$response.StatusCode -lt 200 -or [int]$response.StatusCode -ge 300) {
        throw "$Label failed with HTTP $([int]$response.StatusCode)."
    }
    Write-Output "PASS $Label"
    if ([string]::IsNullOrWhiteSpace($response.Content)) { return $null }
    try { return $response.Content | ConvertFrom-Json -Depth 30 } catch { return $response.Content }
}

function Unwrap-Result([object]$response) {
    if ($null -eq $response) { return $null }
    if ($null -ne $response.result) { return $response.result }
    if ($null -ne $response.data) { return $response.data }
    return $response
}

function First-Item([object]$response) {
    $value = Unwrap-Result $response
    if ($null -ne $value.items) { return @($value.items)[0] }
    return @($value)[0]
}

function Find-SelectableNode([object[]]$nodes) {
    foreach ($node in $nodes) {
        if ($node.selectable -eq $true) { return $node }
        $found = Find-SelectableNode @($node.children)
        if ($null -ne $found) { return $found }
    }
    return $null
}

$ready = Invoke-Tcp101Request -Method GET -Path '/health/ready' -Body $null -Label 'ready health'
if ($SkipLogin) {
    Write-Output 'PASS health-only smoke (login skipped)'
    exit 0
}

$account = [Environment]::GetEnvironmentVariable('TCP101_TEST_ADMIN_ACCOUNT')
$password = [Environment]::GetEnvironmentVariable('TCP101_TEST_ADMIN_PASSWORD')
if ([string]::IsNullOrWhiteSpace($account) -or [string]::IsNullOrWhiteSpace($password)) {
    throw 'TCP101_TEST_ADMIN_ACCOUNT and TCP101_TEST_ADMIN_PASSWORD are required.'
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output = Join-Path $repositoryRoot 'Admin.NET.Web.Entry/bin/Release/net10.0'
$coreAssembly = Join-Path $output 'Admin.NET.Core.dll'
$cryptoAssembly = Join-Path $output 'BouncyCastle.Cryptography.dll'
if (-not (Test-Path -LiteralPath $coreAssembly) -or -not (Test-Path -LiteralPath $cryptoAssembly)) {
    throw 'Build the Release configuration before running the authenticated smoke test.'
}
[void][Reflection.Assembly]::LoadFrom($cryptoAssembly)
[void][Reflection.Assembly]::LoadFrom($coreAssembly)
$appConfig = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Admin.NET.Application/Configuration/App.json') -Raw
$publicKeyMatch = [regex]::Match($appConfig, '"PublicKey"\s*:\s*"(?<key>[0-9A-Fa-f]+)"')
if (-not $publicKeyMatch.Success) { throw 'Cannot read the SM2 public key.' }
$encryptedPassword = [Admin.NET.Core.GMUtil]::SM2Encrypt($publicKeyMatch.Groups['key'].Value, $password)
$login = Invoke-Tcp101Request -Method POST -Path '/api/sysAuth/login' -Body @{
    account = $account; password = $encryptedPassword; codeId = 0; code = ''
} -Label 'login'
$loginValue = Unwrap-Result $login
$token = $loginValue.accessToken
if ([string]::IsNullOrWhiteSpace($token)) { throw 'Login response did not contain an access token.' }
$headers = @{ Authorization = "Bearer $token" }

$suffix = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$department = '运载试验技术事业部'
$task = Invoke-Tcp101Request -Method POST -Path '/api/101/tasks' -Headers $headers -Label 'create task' -Body @{
    name = "SMOKE-$suffix"; department = $department; area = '上面级试验区'; rig = '五号台'; rigCode = '005'
    engineModel = 'SMOKE'; testType = '接口冒烟'; ignitionDuration = 1; ignitionCount = 1
    client = '自动验证'; plannedDate = [DateTime]::UtcNow.ToString('yyyy-MM-dd'); status = 0
}
$taskId = [string](Unwrap-Result $task)
Write-Output "ID task=$taskId"

Invoke-Tcp101Request -Method PUT -Path "/api/101/tasks/$taskId/plan" -Headers $headers -Label 'save plan' -Body @{
    ignitionTime = [DateTime]::UtcNow.ToString('o'); rows = @(@{ completedDate = $null; content = '冒烟计划'; ownerUnit = '自动验证'; supportUnit = ''; remark = ''; order = 1 })
} | Out-Null

$person = First-Item (Invoke-Tcp101Request -Method GET -Path '/api/101/personnel?page=1&pageSize=1' -Headers $headers -Body $null -Label 'read person')
Invoke-Tcp101Request -Method PUT -Path "/api/101/tasks/$taskId/personnel" -Headers $headers -Label 'save personnel' -Body @{
    team = @(@{ role = '试验指挥'; personId = $person.id; order = 1 }); posts = @()
} | Out-Null

$device = First-Item (Invoke-Tcp101Request -Method GET -Path '/api/101/devices?page=1&pageSize=1' -Headers $headers -Body $null -Label 'read device')
Invoke-Tcp101Request -Method PUT -Path "/api/101/tasks/$taskId/devices" -Headers $headers -Label 'save devices' -Body @{
    systems = @(@{ system = $device.system; deviceIds = @($device.id) })
} | Out-Null

$document = First-Item (Invoke-Tcp101Request -Method GET -Path '/api/101/documents?page=1&pageSize=1' -Headers $headers -Body $null -Label 'read document')
Invoke-Tcp101Request -Method PUT -Path "/api/101/tasks/$taskId/documents" -Headers $headers -Label 'save documents' -Body @{
    types = @(@{ documentType = $document.type; documentIds = @($document.id) })
} | Out-Null

$encodedDepartment = [Uri]::EscapeDataString($department)
$treeResponse = Invoke-Tcp101Request -Method GET -Path "/api/101/workflows/tree?department=$encodedDepartment" -Headers $headers -Body $null -Label 'read workflow tree'
$workflowNode = Find-SelectableNode @(Unwrap-Result $treeResponse)
if ($null -eq $workflowNode) { throw 'No selectable workflow node was returned.' }
Invoke-Tcp101Request -Method PUT -Path "/api/101/tasks/$taskId/workflow" -Headers $headers -Label 'save workflow' -Body @{
    workflowNodeIds = @($workflowNode.id)
} | Out-Null

$operation = First-Item (Invoke-Tcp101Request -Method GET -Path "/api/101/tasks/$taskId/operations" -Headers $headers -Body $null -Label 'read operations')
$operationId = [string]$operation.id
Write-Output "ID operation=$operationId"
Invoke-Tcp101Request -Method PUT -Path "/api/101/operations/$operationId/checks" -Headers $headers -Label 'save checks' -Body @{
    rows = @(@{ id = $null; item = '冒烟检查'; requirement = '通过'; actual = '通过'; remark = ''; order = 1 })
} | Out-Null
Invoke-Tcp101Request -Method POST -Path "/api/101/operations/$operationId/signatures" -Headers $headers -Label 'sign operation' -Body @{
    role = '操作岗'
} | Out-Null

Write-Output 'PASS authenticated main flow'

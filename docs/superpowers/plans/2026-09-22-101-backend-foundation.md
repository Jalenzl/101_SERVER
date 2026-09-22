# 101 Backend Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a runnable, slim Admin.NET-based .NET 10 backend with PostgreSQL, secure server-side file storage, and the first task/personnel/device/document/workflow APIs required by the 101 Vue frontend.

**Architecture:** Keep the four Admin.NET host/foundation projects, remove unrelated 511/ISIP/plugin/media features, and add isolated `Admin.NET.Core101` and `Admin.NET.Application101` projects. Core101 owns entities, domain rules, and seeds; Application101 owns DTOs, services, explicit REST controllers, transactions, file storage, and permission metadata.

**Tech Stack:** .NET 10, ASP.NET Core, Furion 4.9.1.23, SqlSugarCore 5.1.4.135, PostgreSQL, xUnit, PowerShell verification scripts.

**Spec:** `docs/superpowers/specs/2026-09-22-101-backend-foundation-design.md`

## Global Constraints

- Target framework is `net10.0`; `global.json` requires .NET SDK 10.0.100 or newer feature band through roll-forward.
- PostgreSQL database is exactly `tpc101`; schema is exactly `public`.
- Database password is read only from `TCP101_DB_PASSWORD`; no password value or password-bearing connection string may enter version control or logs.
- Server file root is read only from `TCP101_FILE_STORAGE_PATH`; no absolute server path may enter version control or API responses.
- All `/api/101` endpoints except health checks require Admin.NET JWT authentication and interface permission checks.
- Business identifiers are `Guid` values serialized as JSON strings.
- 101 tables use the `t101_` prefix.
- Seed data is inserted only when its target table is empty.
- Completed tasks are immutable through every child write endpoint.
- First milestone ends at operation checks and signatures; risk analysis, trial summary, and emergency stop remain absent.
- Default upload limit is 100 MB. Allowed extensions are `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.txt`, `.csv`, `.zip`, `.jpg`, `.jpeg`, and `.png`.

## Review Focus

- REST routes containing record IDs must authorize against fixed permission names, never against the literal Guid path segment; Task 2 adds a metadata-based permission test.
- A missing or empty `TCP101_DB_PASSWORD` must fail before SqlSugar initializes and must not reveal any partial connection string; Task 2 adds both cases.
- Every child mutation path must reject a completed task, including transfer rows, checks, signing, and signature withdrawal; Tasks 6 and 7 add these tests.
- Uploaded names such as `..\\..\\appsettings.json` must never escape the configured storage root, and failed metadata persistence must remove the physical file; Task 5 adds both tests.
- Repeated or concurrent saves of the same task/person/device/document/workflow/signature selection must remain idempotent through unique indexes plus transactional reconciliation; Tasks 3, 6, and 7 add uniqueness and repeat-save tests.

---

## File Structure

Production files created or materially changed by this plan:

```text
Admin.NET.sln
global.json
.gitignore
.env.example
README.md
scripts/Test-RepositoryHygiene.ps1
scripts/Test-Secrets.ps1
scripts/Test-PostgreSqlSmoke.ps1
Admin.NET.Core/
Admin.NET.Application/
Admin.NET.Web.Core/
Admin.NET.Web.Entry/
Admin.NET.Core101/
  Admin.NET.Core101.csproj
  GlobalUsings.cs
  Entity/Entity101Base.cs
  Entity/Task101.cs
  Entity/Person101.cs
  Entity/Device101.cs
  Entity/Document101.cs
  Entity/StoredFile101.cs
  Entity/TaskPlan101.cs
  Entity/TaskPerson101.cs
  Entity/TaskDevice101.cs
  Entity/TaskTransferRecord101.cs
  Entity/TaskDocument101.cs
  Entity/WorkflowNode101.cs
  Entity/TaskWorkflow101.cs
  Entity/Operation101.cs
  Entity/OperationCheck101.cs
  Entity/OperationSignature101.cs
  Enum/TaskStatus101.cs
  Enum/OperationStatus101.cs
  Enum/SystemType101.cs
  Domain/TaskWriteGuard.cs
  Domain/SignatureRoles.cs
  Domain/SelectionReconciler.cs
  Domain/WorkflowOperationPlanner.cs
  Seed/SeedIds101.cs
  Seed/MasterDataSeed101.cs
  Seed/TaskSeed101.cs
  Seed/MenuSeed101.cs
  Seed/RoleMenuSeed101.cs
Admin.NET.Application101/
  Admin.NET.Application101.csproj
  GlobalUsings.cs
  Startup.cs
  Authorization/ApiPermissionAttribute.cs
  Configuration/Tcp101DatabaseOptions.cs
  Configuration/Tcp101FileStorageOptions.cs
  Configuration/Tcp101ConnectionStringFactory.cs
  Health/Tcp101StorageHealthCheck.cs
  Controllers/TasksController.cs
  Controllers/PersonnelController.cs
  Controllers/DevicesController.cs
  Controllers/DocumentsController.cs
  Controllers/FilesController.cs
  Controllers/WorkflowsController.cs
  Controllers/TaskPreparationController.cs
  Controllers/OperationsController.cs
  Dtos/Common/PageQuery.cs
  Dtos/Common/PageResult.cs
  Dtos/Tasks/TaskDtos.cs
  Dtos/Personnel/PersonDtos.cs
  Dtos/Devices/DeviceDtos.cs
  Dtos/Documents/DocumentDtos.cs
  Dtos/Workflows/WorkflowDtos.cs
  Dtos/Preparation/PreparationDtos.cs
  Dtos/Operations/OperationDtos.cs
  Services/TaskService101.cs
  Services/PersonnelService101.cs
  Services/DeviceService101.cs
  Services/DocumentService101.cs
  Services/LocalFileStorage101.cs
  Services/WorkflowService101.cs
  Services/TaskPreparationService101.cs
  Services/OperationService101.cs
  Services/IFileStorage101.cs
  Services/IStoredFileRepository101.cs
  Services/StoredFileRepository101.cs
  Services/ICurrentUser101.cs
  Services/AdminNetCurrentUser101.cs
  Validation/FileUploadPolicy101.cs
  Validation/TransferFieldAllowList101.cs
tests/Admin.NET.Core101.Tests/
tests/Admin.NET.Application101.Tests/
```

The four Admin.NET directories are copied from the local reference repository, then reduced in place. No source file under `INSOFWORKS_511DataManagement_ISIP` is modified.

---

### Task 1: Establish the .NET 10 solution and prove the slim baseline

**Files:**
- Create: `global.json`
- Create: `.gitignore`
- Create: `scripts/Test-RepositoryHygiene.ps1`
- Create: `Admin.NET.sln`
- Create from the local reference then slim: `Admin.NET.Core/**`
- Create from the local reference then slim: `Admin.NET.Application/**`
- Create from the local reference then slim: `Admin.NET.Web.Core/**`
- Create from the local reference then slim: `Admin.NET.Web.Entry/**`
- Modify: `Admin.NET.Core/Admin.NET.Core.csproj`
- Modify: `Admin.NET.Core/GlobalUsings.cs`
- Modify: `Admin.NET.Application/Admin.NET.Application.csproj`
- Modify: `Admin.NET.Application/Configuration/*.json`
- Modify: `Admin.NET.Web.Core/Admin.NET.Web.Core.csproj`
- Modify: `Admin.NET.Web.Core/Startup.cs`
- Modify: `Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj`
- Modify: `Admin.NET.Web.Entry/appsettings.json`

**Interfaces:**
- Consumes: local reference repository `../INSOFWORKS_511DataManagement_ISIP` as read-only source.
- Produces: a four-project Admin.NET solution that restores and builds on .NET 10 without 511, ISIP, plugins, OCR, FFmpeg, GoView, Elsa, scheduling UI, OSS, Elasticsearch, OAuth, WeChat, payment, SMS, code generation, APIJSON, customer forms, customer workflows, or training.

- [ ] **Step 1: Verify the required SDK before changing source**

Run:

```powershell
dotnet --list-sdks
```

Expected: at least one line begins with `10.0.`. If it is absent, stop this task and install it from an elevated PowerShell with:

```powershell
winget install --id Microsoft.DotNet.SDK.10 --exact --source winget
```

- [ ] **Step 2: Write the failing repository-hygiene test**

Create `scripts/Test-RepositoryHygiene.ps1` with this complete rule set:

```powershell
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
if ($errors.Count -gt 0) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Output 'Repository hygiene checks passed.'
```

- [ ] **Step 3: Run the hygiene test to verify it fails on the absent foundation**

Run:

```powershell
pwsh -File scripts/Test-RepositoryHygiene.ps1
```

Expected: FAIL with four `Missing required project` errors because the foundation has not been established.

- [ ] **Step 4: Create the SDK pin and copy only the four base projects**

Create `global.json`:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

Copy `.editorconfig`, `.gitattributes`, and the four project directories from `../INSOFWORKS_511DataManagement_ISIP`. Exclude `.git`, `bin`, `obj`, `logs`, `publish`, `*.db`, `PythonAlgorithm`, `FFMpegCore`, and `fonts`. Do not copy `511`, `ISIP`, or `Plugins`.

Create `.gitignore` with at least:

```gitignore
bin/
obj/
.vs/
.idea/
*.user
*.suo
logs/
publish/
uploads/
*.db
.env
TestResults/
coverage/
```

- [ ] **Step 5: Reduce project references, package references, and registrations**

Keep these package families only where used: Furion, Furion JWT, Mapster, SqlSugarCore, NewtonsoftJson MVC integration, Yitter ID generation, the existing cache implementation required by authentication, and Knife4j/OpenAPI.

Remove the forbidden package references listed in the hygiene script. Remove the `ISIP` project reference from `Admin.NET.Web.Core.csproj`. Replace the service registration body in `Admin.NET.Web.Core/Startup.cs` with registrations for project options, cache, SqlSugar, global JWT authorization, CORS, controllers with Admin.NET unify results, forwarded headers, logging, and Swagger. Remove schedule, OAuth, Elasticsearch, ImageSharp, OSS, view engine, SignalR, sensitive-word processing, FFmpeg, and schedule UI calls.

Keep middleware in this order:

```csharp
app.UseForwardedHeaders();
app.UseUnifyResultStatusCodes();
app.UseCorsAccessor();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseInject(string.Empty);
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health/live");
    endpoints.MapHealthChecks("/health/ready");
});
```

Remove Core service/entity/seed folders for APIJSON, CodeGen, CustomerForm, CustomerWorkflow, Job, Training, WeChat, plugin management, print, notice, online-user SignalR, OAuth, open-access signature auth, and tenant database provisioning only after removing their call sites. Preserve the minimum user, role, organization, position, menu, config, dictionary, cache, authentication, logging, common, and repository code needed for login and permission checks.

- [ ] **Step 6: Create the solution and run the first build loop**

Run:

```powershell
dotnet new sln --name Admin.NET --force
dotnet sln Admin.NET.sln add Admin.NET.Core/Admin.NET.Core.csproj
dotnet sln Admin.NET.sln add Admin.NET.Application/Admin.NET.Application.csproj
dotnet sln Admin.NET.sln add Admin.NET.Web.Core/Admin.NET.Web.Core.csproj
dotnet sln Admin.NET.sln add Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj
dotnet restore Admin.NET.sln
dotnet build Admin.NET.sln --configuration Debug --no-restore
```

Expected: build succeeds. For each compiler error caused by a removed subsystem, remove only the dead reference or restore the smallest authentication dependency; do not restore a forbidden subsystem.

- [ ] **Step 7: Re-run hygiene and build verification**

Run:

```powershell
pwsh -File scripts/Test-RepositoryHygiene.ps1
dotnet build Admin.NET.sln --configuration Release
```

Expected: both commands exit 0.

- [ ] **Step 8: Commit the slim foundation**

```powershell
git add .editorconfig .gitattributes .gitignore global.json scripts Admin.NET.sln Admin.NET.Core Admin.NET.Application Admin.NET.Web.Core Admin.NET.Web.Entry
git commit -m "chore: establish slim Admin.NET foundation"
```

---

### Task 2: Add secret-safe PostgreSQL configuration, health checks, and stable permission metadata

**Files:**
- Create: `Admin.NET.Application101/Admin.NET.Application101.csproj`
- Create: `Admin.NET.Application101/GlobalUsings.cs`
- Create: `Admin.NET.Application101/Startup.cs`
- Create: `Admin.NET.Application101/Authorization/ApiPermissionAttribute.cs`
- Create: `Admin.NET.Application101/Configuration/Tcp101DatabaseOptions.cs`
- Create: `Admin.NET.Application101/Configuration/Tcp101FileStorageOptions.cs`
- Create: `Admin.NET.Application101/Configuration/Tcp101ConnectionStringFactory.cs`
- Create: `Admin.NET.Application101/Health/Tcp101StorageHealthCheck.cs`
- Create: `tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj`
- Create: `tests/Admin.NET.Application101.Tests/Configuration/Tcp101ConnectionStringFactoryTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Authorization/ApiPermissionAttributeTests.cs`
- Modify: `Admin.NET.Web.Core/Handlers/JwtHandler.cs`
- Modify: `Admin.NET.Web.Core/Startup.cs`
- Modify: `Admin.NET.Application/Configuration/Database.json`
- Modify: `Admin.NET.Web.Core/Admin.NET.Web.Core.csproj`
- Modify: `Admin.NET.sln`

**Interfaces:**
- Consumes: `IConfiguration`, `IHostEnvironment`, Admin.NET `JwtHandler`, endpoint metadata.
- Produces: `Tcp101ConnectionStringFactory.Create(Tcp101DatabaseOptions, string?)`, `[ApiPermission(string)]`, `/health/live`, and `/health/ready`.

- [ ] **Step 1: Create the Application101 project and failing configuration tests**

Use this project reference shape:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Admin.NET.Core\Admin.NET.Core.csproj" />
  </ItemGroup>
</Project>
```

Create the test project with these exact references:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.8" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" PrivateAssets="all" />
    <ProjectReference Include="..\..\Admin.NET.Application101\Admin.NET.Application101.csproj" />
    <ProjectReference Include="..\..\Admin.NET.Web.Entry\Admin.NET.Web.Entry.csproj" />
  </ItemGroup>
</Project>
```

Write xUnit tests that assert:

```csharp
[Fact]
public void Create_Throws_WhenPasswordIsMissing()
{
    var options = new Tcp101DatabaseOptions();
    var error = Assert.Throws<InvalidOperationException>(() => Tcp101ConnectionStringFactory.Create(options, null));
    Assert.Equal("Environment variable TCP101_DB_PASSWORD is required.", error.Message);
}

[Fact]
public void Create_BuildsExpectedPostgreSqlConnectionString()
{
    var options = new Tcp101DatabaseOptions { Host = "db", Port = 5432, Username = "app" };
    var password = $"test-{Guid.NewGuid():N}";
    var value = Tcp101ConnectionStringFactory.Create(options, password);
    var parsed = new Npgsql.NpgsqlConnectionStringBuilder(value);
    Assert.Equal("db", parsed.Host);
    Assert.Equal("tpc101", parsed.Database);
    Assert.Equal("public", parsed.SearchPath);
    Assert.Equal(password, parsed.Password);
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter Tcp101ConnectionStringFactoryTests
```

Expected: FAIL because the configuration types do not exist.

- [ ] **Step 3: Implement exact options and connection-string construction**

`Tcp101DatabaseOptions` has `Host = "localhost"`, `Port = 5432`, `Username = "postgres"`, `Database = "tpc101"`, and `Schema = "public"`. Validate that Database and Schema remain those exact values. `Create` rejects null, empty, or whitespace passwords, then uses `NpgsqlConnectionStringBuilder` or SqlSugar's PostgreSQL-compatible builder to set Host, Port, Username, Password, Database, and SearchPath without string concatenation.

Update `Database.json` so it contains `DbType: "PostgreSQL"`, no `Password` property, no password-bearing connection string, `EnableInitDb: false`, `EnableInitTable: true`, and `EnableInitSeed: true`. In `Startup.ConfigureServices`, construct the connection string in memory before calling `AddSqlSugar` and assign it to the default `DbConnectionConfig`.

- [ ] **Step 4: Write and run failing permission metadata tests**

Create this attribute contract:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiPermissionAttribute(string name) : Attribute
{
    public string Name { get; } = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Permission name is required.", nameof(name))
        : name;
}
```

Tests must assert whitespace is rejected and `new ApiPermissionAttribute("101:task:update").Name` returns the fixed permission. Run the test before implementation and expect failure.

- [ ] **Step 5: Make JwtHandler prefer fixed permission metadata**

Change permission resolution to:

```csharp
var endpointPermission = httpContext.GetEndpoint()?.Metadata.GetMetadata<ApiPermissionAttribute>()?.Name;
var routeName = endpointPermission ?? (httpContext.Request.Path.StartsWithSegments("/api")
    ? httpContext.Request.Path.Value![5..].Replace("/", ":")
    : httpContext.Request.Path.Value![1..].Replace("/", ":"));
```

Keep existing super-admin behavior and menu-permission lookup. This preserves old Admin.NET endpoints while ensuring `/api/101/tasks/{guid}` authorizes against a stable name.

- [ ] **Step 6: Add health checks**

Register a live check with no dependencies and a ready check that calls `ISqlSugarClient.Ado.CheckConnection()` plus `Tcp101StorageHealthCheck`. The storage check reads `TCP101_FILE_STORAGE_PATH`, creates the directory when absent, writes and deletes a randomly named zero-byte probe, and returns only `Healthy`, `Unhealthy database`, or `Unhealthy storage`; it never returns paths or connection strings.

- [ ] **Step 7: Run tests, build, and scan configuration**

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj
dotnet build Admin.NET.sln --configuration Release
rg -n -i 'password\s*[=:]\s*[^$%{< ]|Host=.*Password=' -g '!docs/**' -g '!*.md' .
```

Expected: tests and build exit 0; the scan returns no tracked secret values.

- [ ] **Step 8: Commit secure configuration and authorization metadata**

```powershell
git add Admin.NET.Application101 Admin.NET.Application/Configuration/Database.json Admin.NET.Web.Core Admin.NET.sln tests
git commit -m "feat: add secure PostgreSQL configuration"
```

---

### Task 3: Create Core101 entities, constraints, domain guards, and idempotent seed data

**Files:**
- Create: `Admin.NET.Core101/Admin.NET.Core101.csproj`
- Create: `Admin.NET.Core101/GlobalUsings.cs`
- Create: all `Admin.NET.Core101/Entity/*.cs` files listed in File Structure
- Create: `Admin.NET.Core101/Enum/TaskStatus101.cs`
- Create: `Admin.NET.Core101/Enum/OperationStatus101.cs`
- Create: `Admin.NET.Core101/Enum/SystemType101.cs`
- Create: `Admin.NET.Core101/Domain/TaskWriteGuard.cs`
- Create: `Admin.NET.Core101/Domain/SignatureRoles.cs`
- Create: `Admin.NET.Core101/Seed/SeedIds101.cs`
- Create: `Admin.NET.Core101/Seed/MasterDataSeed101.cs`
- Create: `Admin.NET.Core101/Seed/TaskSeed101.cs`
- Create: `tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj`
- Create: `tests/Admin.NET.Core101.Tests/EntityMappingTests.cs`
- Create: `tests/Admin.NET.Core101.Tests/TaskWriteGuardTests.cs`
- Create: `tests/Admin.NET.Core101.Tests/SeedDataTests.cs`
- Modify: `Admin.NET.Application101/Admin.NET.Application101.csproj`
- Modify: `Admin.NET.sln`

**Interfaces:**
- Consumes: Admin.NET `IDeletedFilter`, SqlSugar mapping attributes, front-end seed sources under `../101_WEB/101-WEB/src/data`.
- Produces: 15 mapped Core101 entity types, `TaskWriteGuard.EnsureMutable(TaskStatus101)`, allowed signature-role sets, and deterministic seed collections.

- [ ] **Step 1: Add Core101 and its test project to the solution**

`Admin.NET.Core101.csproj` targets `net10.0`, enables nullable reference types, and references `Admin.NET.Core`. `Admin.NET.Application101` references Core101. Test projects use `Microsoft.NET.Test.Sdk` 17.13.0, `xunit` 2.9.2, and `xunit.runner.visualstudio` 2.8.2; the Application101 test project also uses `Microsoft.AspNetCore.Mvc.Testing` 10.0.8.

- [ ] **Step 2: Write failing mapping and guard tests**

Test all entity types for a `SugarTable` name beginning with `t101_`, a Guid primary key, and implementation of `IDeletedFilter`. Add these concrete guard tests:

```csharp
[Theory]
[InlineData(TaskStatus101.InProgress)]
[InlineData(TaskStatus101.Terminated)]
public void EnsureMutable_AllowsNonCompletedTasks(TaskStatus101 status)
{
    TaskWriteGuard.EnsureMutable(status);
}

[Fact]
public void EnsureMutable_RejectsCompletedTask()
{
    var error = Assert.Throws<InvalidOperationException>(() => TaskWriteGuard.EnsureMutable(TaskStatus101.Completed));
    Assert.Equal("任务已完成，不允许修改。", error.Message);
}
```

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj
```

Expected: FAIL because Core101 types do not exist.

- [ ] **Step 3: Implement the entity base and enums**

Use this base contract:

```csharp
public abstract class Entity101Base : IDeletedFilter
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public long? CreateUserId { get; set; }
    [SugarColumn(Length = 64)] public string? CreateUserName { get; set; }
    public DateTime? UpdateTime { get; set; }
    public long? UpdateUserId { get; set; }
    [SugarColumn(Length = 64)] public string? UpdateUserName { get; set; }
    public bool IsDelete { get; set; }
}
```

Enums map JSON/display values as follows:

```text
TaskStatus101: InProgress="进行中", Completed="已完成", Terminated="终止"
OperationStatus101: NotStarted="未开始", InProgress="进行中", Completed="已完成"
SystemType101: Process="工艺系统", Control="控制系统", Measurement="测量系统"
```

- [ ] **Step 4: Implement the complete entity schema**

Use these authoritative columns in addition to the base audit columns:

| Entity/table | Required business columns |
| --- | --- |
| `Task101` / `t101_task` | `Name`, `Department`, `Area`, `Rig`, `RigCode`, `EngineModel`, `TestType`, `IgnitionDuration:int`, `IgnitionCount:int`, `Client`, `PlannedDate:DateOnly`, `IgnitionTime:DateTime?`, `Status:TaskStatus101` |
| `Person101` / `t101_person` | `Name`, `Gender`, `Department`, `Area`, `Title`, `SpecialOps:bool`, `SpecialOpsValidUntil:DateOnly?`, `Inspector:bool`, `InspectorValidUntil:DateOnly?`, `Calibrator:bool`, `CalibratorValidUntil:DateOnly?`, `ProductAssurance:bool`, `Contact`, `TestCount:int`, `ExamPassed:bool` |
| `Device101` / `t101_device` | `Code`, `Name`, `FactoryCode`, `Manufacturer`, `Model`, `Range`, `EnabledDate:DateOnly?`, `UsageStatus`, `IsMeasuring:bool`, `CalibrationDate:DateOnly?`, `CalibrationCycle`, `ValidUntil:DateOnly?`, `CalibrationStatus`, `CertificateNo`, `CertificateStoredFileId:Guid?`, `LastMaintenance:DateOnly?`, `MaintenanceContent`, `MaintenanceCycle`, `NextMaintenance:DateOnly?`, `MaintenanceStoredFileId:Guid?`, `SuggestedUses:int?`, `UsedCount:int`, `SuggestedYears:int?`, `OwnerPersonId:Guid?`, `Department`, `Area`, `Rig`, `RigCode`, `System:SystemType101`, `Subsystem` |
| `Document101` / `t101_document` | `Code`, `Name`, `Type`, `Department`, `AuthorPersonId:Guid?`, `PublishedAt:DateOnly?`, `CurrentStoredFileId:Guid?` |
| `StoredFile101` / `t101_stored_file` | `OriginalName`, `StorageName`, `RelativePath`, `Extension`, `ContentType`, `Size:long`, `Sha256`, `UploaderUserId:long`, `UploaderName` |
| `TaskPlan101` / `t101_task_plan` | `TaskId:Guid`, `CompletedDate:DateOnly?`, `Content`, `OwnerUnit`, `SupportUnit`, `Remark`, `OrderNo:int` |
| `TaskPerson101` / `t101_task_person` | `TaskId:Guid`, `PersonId:Guid`, `Kind`, `Role`, `System:SystemType101?`, `Rig`, `PostName`, `PostCode`, `OrderNo:int` |
| `TaskDevice101` / `t101_task_device` | `TaskId:Guid`, `DeviceId:Guid`, `System:SystemType101`, `OrderNo:int` |
| `TaskTransferRecord101` / `t101_task_transfer_record` | `TaskId:Guid`, `TableId`, `DataJson` mapped as PostgreSQL `jsonb`, `OrderNo:int` |
| `TaskDocument101` / `t101_task_document` | `TaskId:Guid`, `DocumentId:Guid`, `DocumentType`, `OrderNo:int` |
| `WorkflowNode101` / `t101_workflow_node` | `Department`, `Area`, `Process`, `Step`, `Post`, `OrderNo:int`, `Enabled:bool` |
| `TaskWorkflow101` / `t101_task_workflow` | `TaskId:Guid`, `WorkflowNodeId:Guid`, `OrderNo:int` |
| `Operation101` / `t101_operation` | `TaskId:Guid`, `WorkflowNodeId:Guid`, `Code`, `Phase`, `Process`, `Step`, `Post`, `OperationDate:DateOnly?`, `QualityRequirement`, `OperationRequirement`, `Attention`, `Status:OperationStatus101`, `OrderNo:int` |
| `OperationCheck101` / `t101_operation_check` | `OperationId:Guid`, `Item`, `Requirement`, `Actual`, `Remark`, `OrderNo:int` |
| `OperationSignature101` / `t101_operation_signature` | `Scope`, `ScopeId:Guid`, `Role`, `SignerUserId:long`, `SignerName`, `SignedAt:DateTime` |

Add unique indexes for `Device101.Code`, `Document101.Code`, `(TaskId, PersonId, Kind, Role, System)`, `(TaskId, DeviceId, System)`, `(TaskId, DocumentId, DocumentType)`, `(TaskId, WorkflowNodeId)`, `(TaskId, TableId, OrderNo)`, `(OperationId, OrderNo)`, and `(Scope, ScopeId, Role)`.

- [ ] **Step 5: Implement domain guards and signature roles**

`TaskWriteGuard.EnsureMutable` throws the exact Chinese message tested above only for Completed. `SignatureRoles` exposes immutable sets:

```csharp
public static readonly IReadOnlySet<string> Equipment = new HashSet<string>
    { "配置人", "复核人", "负责人" };
public static readonly IReadOnlySet<string> Operation = new HashSet<string>
    { "操作岗", "检查岗", "检验员会签", "委托单位会签", "产保会签" };
```

- [ ] **Step 6: Write deterministic seed tests, then implement seeds**

Tests assert every seed ID is non-empty, unique across the module, repeat enumeration returns the same IDs, and at least these front-end fixtures exist: task `task-gg11-01`, device `device-1`, person `person-1`, document `doc-1`, workflow node `workflow-model-0001`. Map those stable front-end identifiers to fixed Guid constants in `SeedIds101`; do not derive Guid values with runtime hashing.

Convert the exact task/person/device/file/workflow demonstration rows from:

```text
../101_WEB/101-WEB/src/data/seed.ts
../101_WEB/101-WEB/src/data/modelSeeds.ts
../101_WEB/101-WEB/src/data/workflowSeed.ts
```

Seed classes expose collections but do not use Admin.NET's update-on-start behavior. `Startup` performs `AnyAsync()` per target table and inserts the entire collection only when empty, in dependency order: people, devices, stored files, documents, workflow nodes, tasks, task plans, task people, task devices, task documents, task workflow records.

- [ ] **Step 7: Run Core101 tests and the full build**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj
dotnet build Admin.NET.sln --configuration Release
```

Expected: both exit 0.

- [ ] **Step 8: Commit the domain model and seeds**

```powershell
git add Admin.NET.Core101 Admin.NET.Application101/Admin.NET.Application101.csproj Admin.NET.sln tests/Admin.NET.Core101.Tests
git commit -m "feat: add 101 domain model and seeds"
```

---

### Task 4: Implement task, personnel, device, and workflow master-data APIs

**Files:**
- Create: `Admin.NET.Application101/Dtos/Common/PageQuery.cs`
- Create: `Admin.NET.Application101/Dtos/Common/PageResult.cs`
- Create: `Admin.NET.Application101/Dtos/Tasks/TaskDtos.cs`
- Create: `Admin.NET.Application101/Dtos/Personnel/PersonDtos.cs`
- Create: `Admin.NET.Application101/Dtos/Devices/DeviceDtos.cs`
- Create: `Admin.NET.Application101/Dtos/Workflows/WorkflowDtos.cs`
- Create: `Admin.NET.Application101/Controllers/TasksController.cs`
- Create: `Admin.NET.Application101/Controllers/PersonnelController.cs`
- Create: `Admin.NET.Application101/Controllers/DevicesController.cs`
- Create: `Admin.NET.Application101/Controllers/WorkflowsController.cs`
- Create: `Admin.NET.Application101/Services/TaskService101.cs`
- Create: `Admin.NET.Application101/Services/PersonnelService101.cs`
- Create: `Admin.NET.Application101/Services/DeviceService101.cs`
- Create: `Admin.NET.Application101/Services/WorkflowService101.cs`
- Create: `tests/Admin.NET.Application101.Tests/Controllers/MasterDataRouteTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Services/MasterDataValidationTests.cs`

**Interfaces:**
- Consumes: `SqlSugarRepository<Task101>`, `SqlSugarRepository<Person101>`, `SqlSugarRepository<Device101>`, `SqlSugarRepository<WorkflowNode101>`.
- Produces: the master-data routes from spec section 9.1, `PageResult<T>`, and fixed permission metadata from `101:task:read` through `101:workflow:delete`.

- [ ] **Step 1: Write failing route and permission tests**

Use reflection to assert every controller has `[ApiController]`, its exact route prefix, expected HTTP method templates, and `ApiPermissionAttribute`. Required fixed permissions:

```text
101:task:read, 101:task:create, 101:task:update, 101:task:delete, 101:task:status
101:person:read, 101:person:create, 101:person:update, 101:person:delete
101:device:read, 101:device:create, 101:device:update, 101:device:delete
101:workflow:read, 101:workflow:create, 101:workflow:update, 101:workflow:delete
```

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter MasterDataRouteTests
```

Expected: FAIL because controllers are absent.

- [ ] **Step 2: Define DTO contracts without exposing entities**

`PageQuery` contains `Page = 1`, `PageSize = 20`, `Keyword`, and validates `Page >= 1` and `1 <= PageSize <= 200`. `PageResult<T>` contains `Items`, `Page`, `PageSize`, and `Total`.

Task DTO fields exactly match front-end `TaskRecord`: `id`, `name`, `department`, `area`, `rig`, `rigCode`, `engineModel`, `testType`, `ignitionDuration`, `ignitionCount`, `client`, `plannedDate`, `status`. Create/update inputs omit `id`; status changes use `{ status }`.

Person and device DTOs expose every business column defined in Task 3 using camelCase JSON names. IDs, file IDs, and owner/author IDs serialize as strings. Boolean fields remain JSON booleans; dates use `yyyy-MM-dd`.

Workflow DTO fields are `id`, `department`, `area`, `process`, `step`, `post`, `order`, and `enabled`. `WorkflowTreeNodeDto` contains `id`, `label`, `kind`, `selectable`, and `children`.

- [ ] **Step 3: Write failing validation tests**

Test that blank task name, negative ignition counts, blank device code, blank person name, and a workflow node without department/process/step are rejected before repository calls. Test `PageSize = 201` is rejected. Use `Validator.TryValidateObject` so tests do not require PostgreSQL.

- [ ] **Step 4: Implement services with explicit projections and conflict status codes**

Each service uses SqlSugar expressions for filters and projection; it does not call `ToListAsync()` before paging. Required query behavior:

- Task keyword searches name, rig, rigCode, engineModel, testType, and client; optional status filter is exact.
- Person keyword searches name, department, area, title, and contact.
- Device keyword searches code, name, factoryCode, model, rig, and owner display name; optional system/status filters are exact.
- Workflow keyword searches department, area, process, step, and post; optional department filter is exact.

Before deleting a person, query `TaskPerson101` and `Document101.AuthorPersonId`. Before deleting a device, query `TaskDevice101`. Before deleting a workflow node, query `TaskWorkflow101`. Throw `Oops.Oh(message).StatusCode(409)` on an active reference. Return 404 with `Oops.Oh("记录不存在。").StatusCode(404)` for missing IDs.

Task deletion opens one SqlSugar transaction and logically deletes the task plus physically deletes task-owned plan, person, device, transfer, document, workflow, operation-check, operation-signature, and operation rows. Task status update invokes the completion validation hook before changing to Completed.

- [ ] **Step 5: Implement thin explicit REST controllers**

Use this shape for every action, changing service, route, permission, and DTO type exactly:

```csharp
[ApiController]
[Route("api/101/tasks")]
public sealed class TasksController(TaskService101 service) : ControllerBase
{
    [HttpGet]
    [ApiPermission("101:task:read")]
    public Task<PageResult<TaskDto>> Page([FromQuery] TaskPageQuery input) => service.PageAsync(input);

    [HttpPost]
    [ApiPermission("101:task:create")]
    public Task<Guid> Create(CreateTaskInput input) => service.CreateAsync(input);

    [HttpGet("{id:guid}")]
    [ApiPermission("101:task:read")]
    public Task<TaskDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("{id:guid}")]
    [ApiPermission("101:task:update")]
    public Task Update(Guid id, UpdateTaskInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}")]
    [ApiPermission("101:task:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);

    [HttpPut("{id:guid}/status")]
    [ApiPermission("101:task:status")]
    public Task SetStatus(Guid id, ChangeTaskStatusInput input) => service.SetStatusAsync(id, input.Status);
}
```

- [ ] **Step 6: Build the workflow tree in the service**

Load enabled workflow nodes filtered by department, order by `OrderNo`, then group in this exact hierarchy: department → area → process → step. Only step nodes are selectable and carry the real workflow-node Guid; grouping node IDs use stable prefixes such as `department:` and `area:` and are never persisted.

- [ ] **Step 7: Run focused tests and build**

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter "MasterDataRouteTests|MasterDataValidationTests"
dotnet build Admin.NET.sln --configuration Release
```

Expected: tests and build exit 0.

- [ ] **Step 8: Commit master-data APIs**

```powershell
git add Admin.NET.Application101 tests/Admin.NET.Application101.Tests
git commit -m "feat: add 101 master data APIs"
```

---

### Task 5: Implement secure local file storage and document APIs

**Files:**
- Create: `Admin.NET.Application101/Dtos/Documents/DocumentDtos.cs`
- Create: `Admin.NET.Application101/Controllers/DocumentsController.cs`
- Create: `Admin.NET.Application101/Controllers/FilesController.cs`
- Create: `Admin.NET.Application101/Services/IFileStorage101.cs`
- Create: `Admin.NET.Application101/Services/IStoredFileRepository101.cs`
- Create: `Admin.NET.Application101/Services/StoredFileRepository101.cs`
- Create: `Admin.NET.Application101/Services/LocalFileStorage101.cs`
- Create: `Admin.NET.Application101/Services/DocumentService101.cs`
- Create: `Admin.NET.Application101/Validation/FileUploadPolicy101.cs`
- Create: `tests/Admin.NET.Application101.Tests/Files/LocalFileStorage101Tests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Files/DocumentUploadRollbackTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Controllers/DocumentRouteTests.cs`

**Interfaces:**
- Consumes: `TCP101_FILE_STORAGE_PATH`, current Admin.NET user, `SqlSugarRepository<Document101>`, `IStoredFileRepository101`.
- Produces: `IFileStorage101.SaveAsync`, `OpenReadAsync`, `DeleteIfExistsAsync`; document CRUD; protected upload/download/delete routes.

- [ ] **Step 1: Write failing path-safety and extension-policy tests**

Use a unique directory under `Path.GetTempPath()` and verify:

```csharp
[Theory]
[InlineData("../../appsettings.json")]
[InlineData("..\\..\\web.config")]
[InlineData("C:\\Windows\\win.ini")]
public async Task SaveAsync_NeverUsesClientPath(string clientName)
{
    await using var content = new MemoryStream("safe"u8.ToArray());
    var saved = await storage.SaveAsync(clientName, "text/plain", content, CancellationToken.None);
    Assert.Equal(root, Path.GetFullPath(Path.Combine(root, saved.RelativePath))[..root.Length]);
    Assert.DoesNotContain("..", saved.RelativePath);
}
```

Also assert empty content, 100 MB plus one byte, and `.exe` are rejected while `.pdf` and `.docx` are accepted.

- [ ] **Step 2: Run file tests to verify they fail**

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter LocalFileStorage101Tests
```

Expected: FAIL because storage types are absent.

- [ ] **Step 3: Implement the file storage boundary**

Use this result contract:

```csharp
public sealed record StoredFileWriteResult(
    string OriginalName,
    string StorageName,
    string RelativePath,
    string Extension,
    string ContentType,
    long Size,
    string Sha256);
```

`SaveAsync` obtains only `Path.GetFileName(clientName)`, lowercases the extension, validates the allow-list, generates `Guid.NewGuid():N` plus extension, stores under UTC `yyyy/MM`, computes SHA-256 during copy, and verifies the normalized destination starts with the normalized root plus a directory separator. It never returns the absolute root.

- [ ] **Step 4: Write the failing rollback test**

Inject a fake `IStoredFileRepository101` whose `InsertAsync` throws. Call `DocumentService101.UploadAsync`, then assert the fake storage received `DeleteIfExistsAsync` for the just-written relative path and that the original persistence exception was preserved.

- [ ] **Step 5: Implement document metadata, upload compensation, and downloads**

Document CRUD uses the same paging and conflict conventions as Task 4. Upload sequence:

```text
validate document → save physical bytes → construct StoredFile101 with current user
→ insert metadata → update Document101.CurrentStoredFileId → commit
```

On any exception after physical save, call `DeleteIfExistsAsync` before rethrowing. Download loads metadata by ID, checks the document permission, then returns `FileStreamResult` using `OriginalName` and `ContentType`. A missing physical file returns HTTP 404 with “文件不存在。” and logs only file ID plus relative path hash.

- [ ] **Step 6: Add exact routes and permission metadata**

```text
GET    /api/101/documents                 101:document:read
POST   /api/101/documents                 101:document:create
GET    /api/101/documents/{id}            101:document:read
PUT    /api/101/documents/{id}            101:document:update
DELETE /api/101/documents/{id}            101:document:delete
POST   /api/101/documents/{id}/files      101:file:upload
GET    /api/101/files/{id}/download       101:file:download
DELETE /api/101/files/{id}                101:file:delete
```

- [ ] **Step 7: Run tests, build, and inspect output paths**

Run:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter "LocalFileStorage101Tests|DocumentUploadRollbackTests|DocumentRouteTests"
dotnet build Admin.NET.sln --configuration Release
```

Expected: tests and build exit 0; test artifacts exist only below the test temp directory.

- [ ] **Step 8: Commit file and document support**

```powershell
git add Admin.NET.Application101 tests/Admin.NET.Application101.Tests
git commit -m "feat: add secure document storage APIs"
```

---

### Task 6: Implement task plans and preparation reconciliation

**Files:**
- Create: `Admin.NET.Core101/Domain/SelectionReconciler.cs`
- Create: `Admin.NET.Application101/Dtos/Preparation/PreparationDtos.cs`
- Create: `Admin.NET.Application101/Controllers/TaskPreparationController.cs`
- Create: `Admin.NET.Application101/Services/TaskPreparationService101.cs`
- Create: `Admin.NET.Application101/Validation/TransferFieldAllowList101.cs`
- Create: `tests/Admin.NET.Core101.Tests/SelectionReconcilerTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Services/TaskPreparationService101Tests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Controllers/PreparationRouteTests.cs`

**Interfaces:**
- Consumes: task/person/device/document/transfer repositories and `TaskWriteGuard`.
- Produces: plan and preparation aggregate DTOs, idempotent set reconciliation, transfer-table JSON validation.

- [ ] **Step 1: Write failing reconciliation tests**

Use this pure contract:

```csharp
public sealed record SelectionDelta<TId>(IReadOnlyList<TId> Add, IReadOnlyList<TId> Remove);
public static SelectionDelta<TId> Compare<TId>(IEnumerable<TId> existing, IEnumerable<TId> requested)
    where TId : notnull;
```

Tests assert duplicate requested IDs collapse, unchanged saves have empty Add/Remove, removed IDs appear only in Remove, and new IDs appear only in Add.

- [ ] **Step 2: Run reconciliation tests to verify they fail**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter SelectionReconcilerTests
```

Expected: FAIL because `SelectionReconciler` is absent.

- [ ] **Step 3: Implement deterministic reconciliation**

Materialize `existing` and `requested` as `HashSet<TId>`, then return `requestedSet.Except(existingSet)` and `existingSet.Except(requestedSet)` ordered by `ToString()` for deterministic tests and logs. Do not preserve duplicate requests.

- [ ] **Step 4: Define preparation DTOs**

The plan input contains `ignitionTime` and ordered rows of `completedDate`, `content`, `ownerUnit`, `supportUnit`, and `remark`.

Personnel input contains:

```csharp
public sealed record TaskTeamMemberInput(string Role, Guid PersonId, int Order);
public sealed record TaskPostInput(SystemType101 System, string Rig, string PostName, string PostCode, Guid PersonId, int Order);
public sealed record SaveTaskPersonnelInput(IReadOnlyList<TaskTeamMemberInput> Team, IReadOnlyList<TaskPostInput> Posts);
```

Device and document inputs contain final selections grouped by system/type and ordered IDs. Transfer input contains ordered rows with stable Guid `id` plus a JSON object.

- [ ] **Step 5: Write failing service tests for completed tasks and invalid references**

Using in-memory fake gateways, assert every PUT method first loads the task, rejects Completed with the exact guard message, rejects missing source IDs with HTTP 409, and makes no writes after rejection. Add repeat-save tests that produce the same final rows and no duplicate inserts.

- [ ] **Step 6: Implement transactional preparation saves**

Each save uses one SqlSugar transaction. Personnel/device/document saves validate all referenced source IDs in one query per entity type, calculate the delta, delete removed links, insert added links, and update order/role fields for retained links. They never accept display snapshots as authoritative.

Plan save replaces ordered `TaskPlan101` rows and updates `Task101.IgnitionTime` in the same transaction.

Transfer save accepts only these `tableId` values from the frontend: `space-thrust`, `space-vacuum`, `space-pressure`, `space-temperature`, `space-flow`, `space-vibration`, `upper-thrust`, `upper-vacuum`, `upper-pressure`, `upper-temperature`, `upper-flow`, `laiyuan-thrust`, `laiyuan-vacuum`, `laiyuan-pressure`, `laiyuan-temperature`, and `laiyuan-flow`. `TransferFieldAllowList101` defines the exact allowed keys from `flowSchemas.ts`; unknown keys and non-scalar JSON values return HTTP 400. Store validated objects as PostgreSQL `jsonb`.

- [ ] **Step 7: Implement routes and permissions**

```text
GET /api/101/tasks/{taskId}/plan                 101:plan:read
PUT /api/101/tasks/{taskId}/plan                 101:plan:update
GET /api/101/tasks/{taskId}/preparation          101:preparation:read
PUT /api/101/tasks/{taskId}/personnel            101:preparation:personnel
PUT /api/101/tasks/{taskId}/devices              101:preparation:device
PUT /api/101/tasks/{taskId}/documents            101:preparation:document
GET /api/101/tasks/{taskId}/transfers/{tableId}  101:preparation:transfer:read
PUT /api/101/tasks/{taskId}/transfers/{tableId}  101:preparation:transfer:update
```

- [ ] **Step 8: Run all preparation tests and build**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter SelectionReconcilerTests
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter "TaskPreparationService101Tests|PreparationRouteTests"
dotnet build Admin.NET.sln --configuration Release
```

Expected: all commands exit 0.

- [ ] **Step 9: Commit plans and preparation APIs**

```powershell
git add Admin.NET.Core101 Admin.NET.Application101 tests
git commit -m "feat: add task planning and preparation APIs"
```

---

### Task 7: Implement workflow selection, operation checks, and authenticated signatures

**Files:**
- Create: `Admin.NET.Core101/Domain/WorkflowOperationPlanner.cs`
- Create: `Admin.NET.Application101/Dtos/Operations/OperationDtos.cs`
- Create: `Admin.NET.Application101/Controllers/OperationsController.cs`
- Create: `Admin.NET.Application101/Services/ICurrentUser101.cs`
- Create: `Admin.NET.Application101/Services/AdminNetCurrentUser101.cs`
- Create: `Admin.NET.Application101/Services/OperationService101.cs`
- Modify: `Admin.NET.Application101/Controllers/TasksController.cs`
- Modify: `Admin.NET.Application101/Services/TaskService101.cs`
- Create: `tests/Admin.NET.Core101.Tests/WorkflowOperationPlannerTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Services/OperationService101Tests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Controllers/OperationRouteTests.cs`

**Interfaces:**
- Consumes: selected workflow IDs, workflow nodes, current user claims, task mutability guard.
- Produces: idempotent operation plans, operation/check CRUD, role-bound signatures, and task workflow routes.

- [ ] **Step 1: Write failing operation-planner tests**

Use this pure contract:

```csharp
public sealed record OperationPlan(
    IReadOnlyList<WorkflowNode101> CreateFrom,
    IReadOnlyList<Guid> RemoveOperationIds,
    IReadOnlyList<Guid> BlockedOperationIds);

public static OperationPlan Plan(
    IReadOnlyCollection<Guid> requestedNodeIds,
    IReadOnlyCollection<WorkflowNode101> availableNodes,
    IReadOnlyCollection<Operation101> existingOperations,
    IReadOnlyCollection<Guid> operationIdsWithChecksOrSignatures);
```

Tests assert repeated selection creates nothing, new leaf selection creates one operation, deselected untouched operations are removed, deselected operations with checks/signatures are retained and cause a 409 decision, and unavailable/wrong-department node IDs are rejected.

- [ ] **Step 2: Run planner tests to verify they fail**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter WorkflowOperationPlannerTests
```

Expected: FAIL because the planner is absent.

- [ ] **Step 3: Implement operation planning and transactional workflow save**

For each new selected workflow leaf, create an operation with copied phase/process/step/post, task planned date, `NotStarted`, and workflow order. Removing selected nodes is allowed only when the generated operation has no checks, no signatures, and status is NotStarted. Otherwise return HTTP 409 with “工步已有执行数据，不能取消对应流程。”

Save `TaskWorkflow101` and operation additions/removals in one transaction. A repeated identical request performs no inserts or deletes.

- [ ] **Step 4: Implement current-user adapter and signature tests**

`ICurrentUser101` exposes `long UserId`, `string RealName`, and `bool IsAdministrator`. `AdminNetCurrentUser101` delegates to Admin.NET `UserManager` and never reads signer identity from request bodies.

Tests use a fake current user and assert:

- signing stores fake user ID/name and `DateTime.UtcNow` within the test time window;
- submitting a second signature for the same scope/role returns 409;
- unsupported roles return 400;
- a normal user cannot withdraw another user's signature;
- the signer and an administrator can withdraw;
- completed tasks reject signing and withdrawal.

- [ ] **Step 5: Implement operation and check DTOs/services**

Operation output fields match front-end `OperationRecord`: `id`, `taskId`, `workflowNodeId`, `code`, `phase`, `process`, `step`, `post`, `date`, `qualityRequirement`, `operationRequirement`, `attention`, `status`, and `updatedAt`.

Check rows are replaced transactionally as an ordered final set with `id`, `item`, `requirement`, `actual`, `remark`, and `order`. Updates load the parent task and invoke `TaskWriteGuard` before any write.

Signature request is exactly:

```csharp
public sealed record CreateSignatureInput(string Role);
```

The service determines scope `operation`, scope ID, current user ID/name, and server UTC timestamp. The `(Scope, ScopeId, Role)` unique index is the concurrency backstop; translate its duplicate-key exception to HTTP 409.

- [ ] **Step 6: Add exact routes and permission metadata**

```text
GET    /api/101/tasks/{taskId}/workflow                    101:workflow-selection:read
PUT    /api/101/tasks/{taskId}/workflow                    101:workflow-selection:update
GET    /api/101/tasks/{taskId}/operations                  101:operation:read
GET    /api/101/operations/{id}                            101:operation:read
PUT    /api/101/operations/{id}                            101:operation:update
PUT    /api/101/operations/{id}/checks                     101:operation:check
POST   /api/101/operations/{id}/signatures                 101:operation:sign
DELETE /api/101/operations/{id}/signatures/{role}          101:operation:withdraw-signature
```

- [ ] **Step 7: Add task-completion validation**

Before setting a task to Completed, require at least one selected workflow and one generated operation. Return HTTP 409 with a message listing the unmet condition. Terminated tasks remain editable because the approved design marks only Completed tasks read-only; do not silently treat Terminated as Completed.

- [ ] **Step 8: Run workflow/signature tests and full build**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter WorkflowOperationPlannerTests
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter "OperationService101Tests|OperationRouteTests"
dotnet build Admin.NET.sln --configuration Release
```

Expected: all commands exit 0.

- [ ] **Step 9: Commit workflow execution support**

```powershell
git add Admin.NET.Core101 Admin.NET.Application101 tests
git commit -m "feat: add workflow operations and signatures"
```

---

### Task 8: Seed permissions, document operation, and enforce repository security

**Files:**
- Create: `Admin.NET.Core101/Seed/MenuSeed101.cs`
- Create: `Admin.NET.Core101/Seed/RoleMenuSeed101.cs`
- Modify: `Admin.NET.Application101/Startup.cs`
- Create: `.env.example`
- Create: `README.md`
- Create: `scripts/Test-Secrets.ps1`
- Create: `tests/Admin.NET.Core101.Tests/PermissionSeedTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Security/AnonymousAccessTests.cs`
- Modify: `Admin.NET.Application/Configuration/App.json`
- Modify: `Admin.NET.Application/Configuration/Swagger.json`
- Modify: `Admin.NET.Web.Entry/appsettings.Development.json`

**Interfaces:**
- Consumes: Admin.NET `SysMenu`, `SysRoleMenu`, role seed IDs, all fixed permission names from Tasks 4-7.
- Produces: idempotent 101 menu/permission seeds, documented environment contract, secret and artifact scan scripts.

- [ ] **Step 1: Write failing permission-seed coverage tests**

Build the expected permission set from this exact list:

```text
101:task:read
101:task:create
101:task:update
101:task:delete
101:task:status
101:person:read
101:person:create
101:person:update
101:person:delete
101:device:read
101:device:create
101:device:update
101:device:delete
101:document:read
101:document:create
101:document:update
101:document:delete
101:file:upload
101:file:download
101:file:delete
101:workflow:read
101:workflow:create
101:workflow:update
101:workflow:delete
101:plan:read
101:plan:update
101:preparation:read
101:preparation:personnel
101:preparation:device
101:preparation:document
101:preparation:transfer:read
101:preparation:transfer:update
101:workflow-selection:read
101:workflow-selection:update
101:operation:read
101:operation:update
101:operation:check
101:operation:sign
101:operation:withdraw-signature
```

Reflect over all controller actions to collect `ApiPermissionAttribute.Name`, collect all button permissions from `MenuSeed101`, and assert set equality. Assert seed IDs are stable, unique, and do not overlap existing Admin.NET seed IDs.

- [ ] **Step 2: Run permission coverage tests to verify they fail**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter PermissionSeedTests
```

Expected: FAIL because the 101 permission seed does not exist.

- [ ] **Step 3: Implement idempotent menu and role-menu seeding**

Create a root `SysMenu` directory record named `101业务` with fixed long ID `1501010000001`. Create menu records for tasks, personnel, devices, documents, workflows, task preparation, and operation execution. Create one button `SysMenu` for every permission listed in Step 1 using IDs starting at `1501010000101` in list order.

At startup, query each fixed ID and insert only missing rows; do not update existing administrator customizations. Grant all 101 button IDs to the existing system-administrator role seed and rely on Admin.NET's super-admin bypass for superadmin. Do not grant them to ordinary-user roles by default.

- [ ] **Step 4: Write authentication-boundary tests**

Using `WebApplicationFactory` with database and storage health checks replaced by test doubles, assert `/health/live` is anonymous, `/health/ready` is anonymous but returns only health status, and representative `/api/101/tasks`, `/api/101/personnel`, `/api/101/documents`, and `/api/101/workflows/tree` requests without JWT return 401.

- [ ] **Step 5: Create the environment contract**

Create `.env.example` exactly as:

```dotenv
TCP101_DB_PASSWORD=
TCP101_FILE_STORAGE_PATH=
TCP101_TEST_ADMIN_ACCOUNT=
TCP101_TEST_ADMIN_PASSWORD=
```

The application itself reads the first two variables. The last two are consumed only by the optional smoke script and never by production startup.

- [ ] **Step 6: Write README with executable setup commands**

README must contain:

1. Required .NET 10 SDK and PostgreSQL prerequisites.
2. SQL to create the database when connected as an authorized PostgreSQL administrator:

```sql
CREATE DATABASE tpc101;
```

3. PowerShell environment setup without a sample password value:

```powershell
$env:TCP101_DB_PASSWORD = Read-Host 'PostgreSQL password' -MaskInput
$env:TCP101_FILE_STORAGE_PATH = 'D:\\tcp101-data\\uploads'
dotnet run --project Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj
```

4. Health URLs, Swagger URL, login requirement, seed-on-empty behavior, upload limit/extensions, and the rule that production should set persistent environment variables through the server's deployment mechanism rather than commit `.env`.
5. The API route table from the approved spec with its fixed permission name beside every route.

- [ ] **Step 7: Implement the secret and artifact scanner**

Create `scripts/Test-Secrets.ps1` to inspect tracked files returned by `git ls-files`. It must fail on:

```text
Password=<non-empty value>
TCP101_DB_PASSWORD=<non-empty value>
Host=<text>;Password=<text>
-----BEGIN PRIVATE KEY-----
*.db, *.pfx, *.p12, .env, appsettings.Production.json
```

Exclude Markdown prose only from generic `Password=` matching, but still scan Markdown for private-key blocks. Print file path and rule name, never the matched secret value.

- [ ] **Step 8: Run permission, auth, hygiene, and secret checks**

Run:

```powershell
dotnet test tests/Admin.NET.Core101.Tests/Admin.NET.Core101.Tests.csproj --filter PermissionSeedTests
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter AnonymousAccessTests
pwsh -File scripts/Test-RepositoryHygiene.ps1
pwsh -File scripts/Test-Secrets.ps1
dotnet build Admin.NET.sln --configuration Release
```

Expected: every command exits 0.

- [ ] **Step 9: Commit permissions and operations documentation**

```powershell
git add Admin.NET.Core101 Admin.NET.Application101 Admin.NET.Application Admin.NET.Web.Entry .env.example README.md scripts tests
git commit -m "docs: add permissions and backend runbook"
```

---

### Task 9: Verify PostgreSQL initialization and the complete authenticated main flow

**Files:**
- Create: `scripts/Test-PostgreSqlSmoke.ps1`
- Create: `tests/Admin.NET.Application101.Tests/Integration/PostgreSqlInitializationTests.cs`
- Create: `tests/Admin.NET.Application101.Tests/Integration/CompletedTaskMutationTests.cs`
- Modify: `README.md`

**Interfaces:**
- Consumes: a reachable PostgreSQL `tpc101` database, `TCP101_DB_PASSWORD`, writable `TCP101_FILE_STORAGE_PATH`, and optional test admin credentials.
- Produces: repeatable database initialization and end-to-end smoke evidence for login → task → plan → preparation → workflow → operation → checks → signature.

- [ ] **Step 1: Add opt-in PostgreSQL integration-test guards**

Tests read `TCP101_RUN_POSTGRES_TESTS`. When it is not exactly `1`, they report skipped with a clear reason. When enabled, they require `TCP101_DB_PASSWORD` and create a unique schema `test_<Guid N>` inside `tpc101`, initialize only 101 test tables there, run tests, then drop only that exact schema in `finally`. They must never drop `public`, any database, or any schema not created by that test run.

- [ ] **Step 2: Write the initialization tests before running them**

Tests prove:

- CodeFirst creates every `t101_` table in the unique test schema.
- the first seed pass inserts task/person/device/document/workflow fixtures;
- the second seed pass does not change row counts or update a user-edited row;
- all declared unique indexes reject duplicates;
- PostgreSQL stores `TaskTransferRecord101.DataJson` as `jsonb` and returns the same object.

Run with the opt-in flag absent and expect skipped tests, not failures:

```powershell
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter PostgreSqlInitializationTests
```

- [ ] **Step 3: Run enabled PostgreSQL initialization tests when credentials are available**

Require the caller to set the password interactively or through server configuration; do not echo it:

```powershell
if ([string]::IsNullOrWhiteSpace($env:TCP101_DB_PASSWORD)) { throw 'Set TCP101_DB_PASSWORD before the PostgreSQL smoke test.' }
$env:TCP101_RUN_POSTGRES_TESTS = '1'
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter PostgreSqlInitializationTests
```

Expected: tests pass and their temporary schema is absent afterward.

- [ ] **Step 4: Write cross-endpoint completed-task mutation tests**

Create one Completed task fixture and issue every child mutation call through the service layer: plan save, personnel save, device save, document save, transfer save, workflow selection save, operation update, check save, signature create, and signature withdrawal. Assert each returns the completed-task conflict and leaves all repository snapshots unchanged.

- [ ] **Step 5: Implement the smoke script without embedded credentials**

`scripts/Test-PostgreSqlSmoke.ps1` parameters are `BaseUrl = 'http://localhost:5000'` and optional `SkipLogin`. It requires `TCP101_TEST_ADMIN_ACCOUNT` and `TCP101_TEST_ADMIN_PASSWORD`, calls the existing Admin.NET login endpoint, extracts the access token from the unified result, and sends bearer-authenticated requests in this exact order:

```text
GET  /health/ready
POST /api/101/tasks
PUT  /api/101/tasks/{taskId}/plan
PUT  /api/101/tasks/{taskId}/personnel
PUT  /api/101/tasks/{taskId}/devices
PUT  /api/101/tasks/{taskId}/documents
GET  /api/101/workflows/tree?department={department}
PUT  /api/101/tasks/{taskId}/workflow
GET  /api/101/tasks/{taskId}/operations
PUT  /api/101/operations/{operationId}/checks
POST /api/101/operations/{operationId}/signatures
```

The script generates unique task content, selects existing seeded person/device/document/workflow IDs returned by read APIs, checks every HTTP status, and prints only IDs plus pass/fail labels. It never prints tokens, passwords, connection strings, or absolute storage paths.

- [ ] **Step 6: Run the complete local verification suite**

Run:

```powershell
dotnet test Admin.NET.sln
dotnet build Admin.NET.sln --configuration Release
pwsh -File scripts/Test-RepositoryHygiene.ps1
pwsh -File scripts/Test-Secrets.ps1
git diff --check
git status --short
```

Expected: tests and build exit 0; both scripts pass; `git diff --check` prints nothing; status contains only intended Task 9 files before commit.

- [ ] **Step 7: Run the live app and authenticated smoke when PostgreSQL is available**

In terminal 1:

```powershell
if ([string]::IsNullOrWhiteSpace($env:TCP101_DB_PASSWORD)) { throw 'TCP101_DB_PASSWORD is required.' }
if ([string]::IsNullOrWhiteSpace($env:TCP101_FILE_STORAGE_PATH)) { throw 'TCP101_FILE_STORAGE_PATH is required.' }
dotnet run --project Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj
```

In terminal 2:

```powershell
pwsh -File scripts/Test-PostgreSqlSmoke.ps1
```

Expected: ready health is healthy and the complete main-flow script exits 0. If PostgreSQL or credentials are not available, record this exact verification as pending rather than claiming it passed; all non-live tests remain mandatory.

- [ ] **Step 8: Inspect the actual diff against the design**

Check each design acceptance criterion explicitly. Confirm there are exactly six production projects, no deferred-domain controllers/entities, no forbidden packages/directories, no password value, all 39 permission names seeded, and all first-milestone routes present.

- [ ] **Step 9: Commit final integration verification**

```powershell
git add scripts/Test-PostgreSqlSmoke.ps1 tests/Admin.NET.Application101.Tests/Integration README.md
git commit -m "test: verify PostgreSQL 101 main flow"
```

---

## Final Verification Checklist

- [ ] `dotnet --list-sdks` includes a `10.0.` SDK.
- [ ] `dotnet test Admin.NET.sln` exits 0 with zero failed tests.
- [ ] `dotnet build Admin.NET.sln --configuration Release` exits 0.
- [ ] `scripts/Test-RepositoryHygiene.ps1` exits 0.
- [ ] `scripts/Test-Secrets.ps1` exits 0.
- [ ] Missing `TCP101_DB_PASSWORD` fails startup without revealing a connection string.
- [ ] `/health/live` and `/health/ready` expose status only.
- [ ] All 39 controller permissions equal all 39 seeded button permissions.
- [ ] PostgreSQL CodeFirst, seed idempotence, `jsonb`, and unique-index tests pass when the test database is available.
- [ ] The authenticated smoke path completes when PostgreSQL and test credentials are available.
- [ ] `git diff --check` prints nothing.
- [ ] `git status --short` is clean after the final commit.

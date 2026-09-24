# 101 后端

这是 101 试验业务的第一版后端：精简 Admin.NET 只负责登录、用户、角色、菜单、字典、配置、缓存和日志；101 的任务、人员、设备、文件、流程、准备、操作与签名位于独立的 `Core101` / `Application101` 模块。

## 安装与准备

需要 Windows、.NET 10 SDK 和 PostgreSQL。可在管理员 PowerShell 中安装 SDK：

```powershell
winget install Microsoft.DotNet.SDK.10
dotnet --version
```

`dotnet --version` 应显示 `10.x`。也可以从 [.NET 10 官方下载页](https://dotnet.microsoft.com/download/dotnet/10.0)安装。PostgreSQL 的地址、端口和用户名以 `Admin.NET.Application/Configuration/Database.json` 中的当前配置为准；如果使用本机数据库，先将其改为 `localhost:5432` 和本机用户名。数据库名固定为 `tpc101`，Schema 固定为 `public`。

使用有建库权限的 PostgreSQL 管理员连接后执行：

```sql
CREATE DATABASE tpc101;
```

## 本机启动

开发机推荐使用 .NET User Secrets 保存密码。项目已设置 `UserSecretsId`；以下命令只需在首次配置或更换密码时执行一次，密码不会出现在命令历史或项目文件中：

```powershell
$project = 'Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj'
$dbPassword = [System.Net.NetworkCredential]::new('', (Read-Host 'PostgreSQL 密码' -AsSecureString)).Password
$adminPassword = [System.Net.NetworkCredential]::new('', (Read-Host '管理员初始密码' -AsSecureString)).Password
@{
  'Tcp101:Database:Password' = $dbPassword
  'Tcp101:InitialAdminPassword' = $adminPassword
} | ConvertTo-Json -Compress | dotnet user-secrets set --project $project
Remove-Variable dbPassword, adminPassword
```

之后每次启动只需：

```powershell
dotnet run --project Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj
```

文件存储目录默认使用 `D:\tcp101-data\uploads`，在 `Admin.NET.Application/Configuration/Database.json` 中配置，不用每次设置环境变量；换机器或部署时可修改此非秘密路径，或用 `TCP101_FILE_STORAGE_PATH` 覆盖。User Secrets 保存在当前 Windows 用户目录，不随仓库提交，也**没有加密**，仅用于本机开发。若当前终端还设置了 `TCP101_DB_PASSWORD` 或 `TCP101_INITIAL_ADMIN_PASSWORD`，环境变量优先；可打开新终端以使用刚保存的 User Secrets。系统启动时会按种子数据更新内置用户（包括 `admin`、`superadmin`）的密码，因此更换管理员初始密码会影响这些账号。

服务默认监听 `http://localhost:5000`：

- 存活检查：`GET /health/live`
- 就绪检查：`GET /health/ready`
- Swagger：`/kapi`

业务接口必须先通过 Admin.NET 登录取得 JWT。首次启动会创建缺失的表；101 业务演示种子仅在对应表为空时整批插入，内置系统用户则会按上述初始密码在启动时更新。上传限制为 100 MB，允许 PDF、Office 文档、文本/CSV 和常见图片扩展名。

生产环境不要使用 User Secrets。由 Windows 服务、容器编排或部署平台的密钥管理系统注入 `TCP101_DB_PASSWORD`、`TCP101_INITIAL_ADMIN_PASSWORD`，并配置可写的 `TCP101_FILE_STORAGE_PATH`；启动时自动读取，无需人工输入。开发、测试、生产使用不同凭据，不要提交 `.env` 或含密码的 `appsettings.Production.json`。

## API 与权限

| 方法与路径 | 权限 |
| --- | --- |
| `GET /api/101/tasks`、`GET /api/101/tasks/{id}` | `101:task:read` |
| `POST /api/101/tasks` | `101:task:create` |
| `PUT /api/101/tasks/{id}` | `101:task:update` |
| `DELETE /api/101/tasks/{id}` | `101:task:delete` |
| `PUT /api/101/tasks/{id}/status` | `101:task:status` |
| `GET /api/101/personnel`、`GET /api/101/personnel/{id}` | `101:person:read` |
| `POST /api/101/personnel` | `101:person:create` |
| `PUT /api/101/personnel/{id}` | `101:person:update` |
| `DELETE /api/101/personnel/{id}` | `101:person:delete` |
| `GET /api/101/devices`、`GET /api/101/devices/{id}` | `101:device:read` |
| `POST /api/101/devices` | `101:device:create` |
| `PUT /api/101/devices/{id}` | `101:device:update` |
| `DELETE /api/101/devices/{id}` | `101:device:delete` |
| `GET /api/101/documents`、`GET /api/101/documents/{id}` | `101:document:read` |
| `POST /api/101/documents` | `101:document:create` |
| `PUT /api/101/documents/{id}` | `101:document:update` |
| `DELETE /api/101/documents/{id}` | `101:document:delete` |
| `POST /api/101/documents/{id}/files` | `101:file:upload` |
| `GET /api/101/files/{id}/download` | `101:file:download` |
| `DELETE /api/101/files/{id}` | `101:file:delete` |
| `GET /api/101/workflows`、`GET /api/101/workflows/tree` | `101:workflow:read` |
| `POST /api/101/workflows` | `101:workflow:create` |
| `PUT /api/101/workflows/{id}` | `101:workflow:update` |
| `DELETE /api/101/workflows/{id}` | `101:workflow:delete` |
| `GET /api/101/tasks/{taskId}/plan` | `101:plan:read` |
| `PUT /api/101/tasks/{taskId}/plan` | `101:plan:update` |
| `GET /api/101/tasks/{taskId}/preparation` | `101:preparation:read` |
| `PUT /api/101/tasks/{taskId}/personnel` | `101:preparation:personnel` |
| `PUT /api/101/tasks/{taskId}/devices` | `101:preparation:device` |
| `PUT /api/101/tasks/{taskId}/documents` | `101:preparation:document` |
| `GET /api/101/tasks/{taskId}/transfers/{tableId}` | `101:preparation:transfer:read` |
| `PUT /api/101/tasks/{taskId}/transfers/{tableId}` | `101:preparation:transfer:update` |
| `GET /api/101/tasks/{taskId}/workflow` | `101:workflow-selection:read` |
| `PUT /api/101/tasks/{taskId}/workflow` | `101:workflow-selection:update` |
| `GET /api/101/tasks/{taskId}/operations`、`GET /api/101/operations/{id}` | `101:operation:read` |
| `PUT /api/101/operations/{id}` | `101:operation:update` |
| `PUT /api/101/operations/{id}/checks` | `101:operation:check` |
| `POST /api/101/operations/{id}/signatures` | `101:operation:sign` |
| `DELETE /api/101/operations/{id}/signatures/{role}` | `101:operation:withdraw-signature` |
| `GET /api/101/tasks/{taskId}/records/{kind}` | `101:task-record:read` |
| `PUT /api/101/tasks/{taskId}/records/{kind}` | `101:task-record:update` |
| `GET /api/101/catalogs/{key}`、`GET /api/101/catalogs/{key}/{id}` | `101:catalog:read` |
| `POST /api/101/catalogs/{key}`、`PUT /api/101/catalogs/{key}/{id}`、`DELETE /api/101/catalogs/{key}/{id}` | `101:catalog:write` |
| `GET /api/101/tasks/{taskId}/signatures/{section}/{qualifier}` | `101:preparation:signature:read` |
| `POST /api/101/tasks/{taskId}/signatures/{section}/{qualifier}` | `101:preparation:signature:sign` |
| `DELETE /api/101/tasks/{taskId}/signatures/{section}/{qualifier}/{role}` | `101:preparation:signature:withdraw` |
| `POST /api/101/devices/{id}/files/{kind}` | `101:file:upload` |
| `POST /api/101/catalogs/training/{id}/files` | `101:file:upload` |

任务记录的 `kind` 支持 `fmeca`、`fmea`、`task-risk`、`summary-overview`、`summary-execution`、`summary-data`、`summary-medium`、`stops`；`PUT` 使用 `{ "rows": [{ "id": "UUID", "order": 1, "data": { ... } }] }` 整表替换。总结与叫停在任务完成后不可修改，风险分析沿用前端的完成后可编辑规则；叫停记录填写恢复时间时必须填写验证情况。

新任务只能以“进行中”状态创建；普通任务更新不能修改状态，完成和终止须调用状态接口。已有操作或准备签名的任务不能删除；无签名任务删除时会一并清理关联记录。同一账号不能在同一操作或准备签署位置签署多个职责。升级已有数据库前，应先核对并处理同一签署位置由同一账号签署多个职责的历史数据，否则新增的唯一索引无法建立。

```sql
SELECT scope, scope_id, signer_user_id, COUNT(*)
FROM t101_operation_signature
GROUP BY scope, scope_id, signer_user_id
HAVING COUNT(*) > 1;
```

准备签署的 `section` 为 `equipment`（`qualifier` 为系统编号 `0`、`1`、`2`）或 `transfer`（`qualifier` 为传递表 ID）；签署请求体为 `{ "role": "配置人" }`。设备附件的 `kind` 为 `certificate` 或 `maintenance`，设备和培训附件通过 `multipart/form-data` 的 `file` 字段上传，下载仍使用 `/api/101/files/{id}/download`。通用目录使用前端对应的目录键，返回 `{ id, data, storedFileId }`；只读风险评价基础表仅提供查询。首次启动会从内置种子初始化这些目录，已有目录数据不会被覆盖。

## 验证

```powershell
dotnet test Admin.NET.sln
dotnet build Admin.NET.sln --configuration Release
pwsh -File scripts/Test-RepositoryHygiene.ps1
pwsh -File scripts/Test-Secrets.ps1
```

真库测试默认跳过。准备好本机 `tpc101` 后，可显式启用；测试只创建并最终删除自己生成的 `test_<guid>` Schema，绝不删除 `public`：

```powershell
$env:TCP101_RUN_POSTGRES_TESTS = '1'
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter PostgreSqlInitializationTests
```

运行中的服务可用 `pwsh -File scripts/Test-PostgreSqlSmoke.ps1` 验证登录到签名的完整主链。该脚本只从 `TCP101_TEST_ADMIN_ACCOUNT` 和 `TCP101_TEST_ADMIN_PASSWORD` 读取测试凭据，不输出凭据或令牌。

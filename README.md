# 101 后端

这是 101 试验业务的第一版后端：精简 Admin.NET 只负责登录、用户、角色、菜单、字典、配置、缓存和日志；101 的任务、人员、设备、文件、流程、准备、操作与签名位于独立的 `Core101` / `Application101` 模块。

## 安装与准备

需要 Windows、.NET 10 SDK 和 PostgreSQL。可在管理员 PowerShell 中安装 SDK：

```powershell
winget install Microsoft.DotNet.SDK.10
dotnet --version
```

`dotnet --version` 应显示 `10.x`。也可以从 [.NET 10 官方下载页](https://dotnet.microsoft.com/download/dotnet/10.0)安装。PostgreSQL 默认连接 `localhost:5432`、用户 `postgres`、数据库 `tcp101`、Schema `public`；可在 `Admin.NET.Application/Configuration/Database.json` 调整除密码外的参数。

使用有建库权限的 PostgreSQL 管理员连接后执行：

```sql
CREATE DATABASE tcp101;
```

## 本机启动

密码只放在当前进程环境变量中，不写配置文件或仓库：

```powershell
$env:TCP101_DB_PASSWORD = Read-Host 'PostgreSQL password' -MaskInput
$env:TCP101_INITIAL_ADMIN_PASSWORD = Read-Host 'Initial Admin.NET password' -MaskInput
$env:TCP101_FILE_STORAGE_PATH = 'D:\tcp101-data\uploads'
dotnet run --project Admin.NET.Web.Entry/Admin.NET.Web.Entry.csproj
```

服务默认监听 `http://localhost:5000`：

- 存活检查：`GET /health/live`
- 就绪检查：`GET /health/ready`
- Swagger：`/kapi`

业务接口必须先通过 Admin.NET 登录取得 JWT。首次启动会创建缺失的表；演示种子仅在对应表为空时整批插入，不覆盖用户修改。上传限制为 100 MB，允许 PDF、Office 文档、文本/CSV 和常见图片扩展名。

生产环境请使用 Windows 服务、容器编排或部署平台的持久环境变量功能设置密钥，不要提交 `.env` 或含密码的 `appsettings.Production.json`。

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

## 验证

```powershell
dotnet test Admin.NET.sln
dotnet build Admin.NET.sln --configuration Release
pwsh -File scripts/Test-RepositoryHygiene.ps1
pwsh -File scripts/Test-Secrets.ps1
```

真库测试默认跳过。准备好本机 `tcp101` 后，可显式启用；测试只创建并最终删除自己生成的 `test_<guid>` Schema，绝不删除 `public`：

```powershell
$env:TCP101_RUN_POSTGRES_TESTS = '1'
dotnet test tests/Admin.NET.Application101.Tests/Admin.NET.Application101.Tests.csproj --filter PostgreSqlInitializationTests
```

运行中的服务可用 `pwsh -File scripts/Test-PostgreSqlSmoke.ps1` 验证登录到签名的完整主链。该脚本只从 `TCP101_TEST_ADMIN_ACCOUNT` 和 `TCP101_TEST_ADMIN_PASSWORD` 读取测试凭据，不输出凭据或令牌。

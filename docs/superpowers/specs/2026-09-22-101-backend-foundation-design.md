# 101 后端底座与首批领域 API 设计

## 1. 目标

在 `101_SERVER` 中创建一个可独立维护的 101 后端。项目以现有本地 Admin.NET 代码为参考基线，保留登录、JWT、用户、角色、组织、菜单和接口权限等通用能力，删除与 101 无关的业务和大体积资源，并通过独立的 `Admin.NET.Core101`、`Admin.NET.Application101` 模块承载 101 业务。

首个里程碑同时交付任务、人员、设备、文件以及试验主流程 API，使现有 Vue 前端可以开始从 `localStorage` 仓储切换到 HTTP 后端。

## 2. 已确认约束

- 运行框架使用 .NET 10。
- ORM 延续 Admin.NET 的 SqlSugar。
- 数据库使用 PostgreSQL，数据库名固定为 `tcp101`，Schema 固定为 `public`。
- 数据库必须由部署人员预先创建；应用负责连接检查、表结构初始化和种子数据初始化，不申请创建数据库所需的超级用户权限。
- 数据库密码只读取环境变量 `TCP101_DB_PASSWORD`，不得出现在仓库文件、示例值、日志或异常详情中。
- 文件本体保存在后端服务器磁盘，数据库只保存文件元数据和相对存储路径。
- 文件根目录只读取环境变量 `TCP101_FILE_STORAGE_PATH`，不得在仓库中写入服务器绝对路径。
- 101 API 复用 Admin.NET 的 JWT、用户、角色、菜单和接口权限体系，不创建第二套用户体系。
- 101 演示数据只在相关业务表为空时初始化，不覆盖、不重复插入真实数据。
- 首个里程碑包含主流程至“工步检查与签署”；风险分析、试验总结和异常叫停延期到后续里程碑。

## 3. 参考基线与技术选择

本地参考基线位于相邻目录 `INSOFWORKS_511DataManagement_ISIP`，当前使用 .NET 10、Furion 4.9.1.23 和 SqlSugarCore 5.1.4.135。新项目采用“从现有基线选择性复制并做减法”的方式，不整体复制旧仓库。

选择该方式的原因：

- 登录权限模型与现有 Admin.NET 保持一致，降低重新实现认证授权的风险。
- 能复用已验证的统一响应、异常处理、参数验证和 SqlSugar 集成。
- 业务代码进入独立模块，不继续扩大 Admin.NET 底座。
- 旧 511、ISIP 和插件代码不会进入新仓库。

不采用以下方案：

- 不从未知版本的在线 Admin.NET 模板重新开始，避免本地参考项目与新模板产生版本和行为差异。
- 不创建普通 ASP.NET Core 项目后再手工移植 Admin.NET 权限体系，避免重复实现底座能力。

## 4. 解决方案结构

新解决方案包含以下生产项目：

```text
Admin.NET.sln
├── Admin.NET.Core
├── Admin.NET.Application
├── Admin.NET.Web.Core
├── Admin.NET.Web.Entry
├── Admin.NET.Core101
└── Admin.NET.Application101
```

职责与依赖方向：

- `Admin.NET.Core`：保留 Admin.NET 系统实体、认证授权、SqlSugar 基础能力和通用基础设施。
- `Admin.NET.Application`：保留系统管理接口，例如登录、用户、角色、组织、菜单和权限管理。
- `Admin.NET.Web.Core`：负责 Web 启动、JWT、权限、跨域、Swagger、统一响应和全局异常处理，并引用两个 Application 项目以完成服务发现。
- `Admin.NET.Web.Entry`：唯一可执行入口，只负责宿主和部署配置。
- `Admin.NET.Core101`：101 实体、枚举、领域规则、数据库初始化规则和种子数据。
- `Admin.NET.Application101`：101 DTO、校验、应用服务、事务、文件存储服务和稳定的 HTTP 路由。

依赖必须保持单向：

```text
Admin.NET.Web.Entry
        ↓
Admin.NET.Web.Core
        ├──→ Admin.NET.Application ──→ Admin.NET.Core
        └──→ Admin.NET.Application101 ──→ Admin.NET.Core101 ──→ Admin.NET.Core
                                      └────────────────────────→ Admin.NET.Core
```

101 模块不得反向依赖 Web 项目，Admin.NET 底座也不得依赖 101 模块。

## 5. Admin.NET 底座瘦身

### 5.1 保留能力

- 登录、JWT 和当前用户上下文。
- 用户、角色、组织、菜单和接口权限。
- 基础字典与系统配置中认证授权实际依赖的部分。
- SqlSugar 数据访问与事务基础能力。
- 统一响应、全局异常、参数验证和 Swagger/OpenAPI。
- 首批 API 真正需要的操作日志能力。

### 5.2 排除能力

- 511 与 ISIP 的全部业务模块。
- GoView、Elsa 等插件。
- OCR、Python 算法、模型文件、FFmpeg 和无关字体资源。
- 微信、支付、短信和第三方 OAuth。
- OSS、ElasticSearch、SignalR。
- 定时任务、作业持久化和调度看板。
- 代码生成、APIJSON。
- Admin.NET 原有客户表单、客户工作流、培训等无关业务。
- 旧数据库、日志、上传文件、发布目录和构建输出。

瘦身遵循可编译依赖边界：先移除注册和项目依赖，再删除只被移除功能使用的代码与包；不为追求目录数量而改写仍被认证授权使用的底座代码。

## 6. 数据库与配置

普通配置文件可保存以下非秘密配置：

- Host，默认 `localhost`。
- Port，默认 `5432`。
- Username，默认 `postgres`。
- Database，固定 `tcp101`。
- SearchPath，固定 `public`。

启动时由代码读取 `TCP101_DB_PASSWORD` 并在内存中组装最终连接字符串。环境变量缺失或为空时，应用在建立数据库服务前快速失败，错误消息只指出缺少变量，不输出部分或完整连接字符串。

101 业务表统一使用 `t101_` 前缀，与 Admin.NET 系统表区分。表结构通过 SqlSugar CodeFirst 初始化。唯一索引至少覆盖业务编号以及各任务关联表的自然唯一组合，防止重复选择产生重复记录。

健康检查至少提供：

- 应用进程存活状态。
- PostgreSQL 可连接状态。
- 文件根目录可访问、可写状态。

健康检查和日志不得暴露数据库密码、完整连接字符串或服务器绝对存储路径。

## 7. 领域模型

101 业务实体使用 `Guid` 主键。JSON 中 Guid 自然序列化为字符串，符合前端稳定字符串 ID 的要求，并避免 JavaScript 大整数精度问题。实体统一记录创建时间、创建用户、更新时间、更新用户和逻辑删除状态；任务的聚合子记录按业务规则级联清理。

首批实体：

| 实体 | 职责 |
| --- | --- |
| `Task101` | 试验任务基本信息、任务状态和任务上下文 |
| `Person101` | 人员基本信息与岗位资质能力矩阵 |
| `Device101` | 设备概要、校检、维保和使用信息 |
| `Document101` | 文件编号、名称、类型、撰写部门、撰写人和发布时间 |
| `StoredFile101` | 原文件名、存储名、相对路径、大小、内容类型、上传人和校验信息 |
| `TaskPlan101` | 任务的准备及实施计划项目 |
| `TaskPerson101` | 任务团队、系统岗位与所选人员 |
| `TaskDevice101` | 某任务按系统选入的设备 |
| `TaskTransferRecord101` | 某任务在指定测量传递表中的通道配置；异构字段以 PostgreSQL `jsonb` 保存 |
| `TaskDocument101` | 某任务按类型选入的文件 |
| `WorkflowNode101` | 可复用的部门、区域、工序、工步和岗位流程节点 |
| `TaskWorkflow101` | 某任务选择的流程节点及顺序 |
| `Operation101` | 根据任务流程生成的工步执行记录 |
| `OperationCheck101` | 工步下的检查项目、技术要求、实际状态和备注 |
| `OperationSignature101` | 工步或准备环节的角色、签署用户和签署时间 |

人员、设备和文件被任务选用时，关联表保存来源实体 ID；允许同时保存必要的业务快照字段，但来源 ID 不得丢失。查询 DTO 按前端现有 `TaskRecord`、人员、设备、文件和流程字段命名提供明确字段，不向前端暴露 SqlSugar 实体。

## 8. 领域规则

- 任务状态限定为“进行中”“已完成”“终止”。
- 任务变为“已完成”后，计划、人员准备、设备准备、文件准备、流程、工步、检查项和签署均禁止修改。
- 删除人员、设备或文件前检查有效任务引用；仍被引用时拒绝删除并返回可理解的业务错误。
- 删除任务时，在一个事务内清理该任务的计划、人员、设备、文件、流程、工步、检查项和签署记录。
- 同一任务不能重复选择相同的人员、设备、文件或流程节点。
- 保存任务流程选择时，根据最终叶子节点集合新增缺失工步，并删除已取消流程对应且尚未产生不可撤销业务结果的工步。
- 流程保存和工步增删必须在同一数据库事务内完成。
- 签署角色必须属于接口允许的角色集合：设备准备使用“配置人、复核人、负责人”，工步执行使用“操作岗、检查岗、检验员会签、委托单位会签、产保会签”；签署用户 ID、显示名和时间由后端根据当前 JWT 用户生成，前端不得指定其他签署人或签署时间。
- 撤销签署只允许签署本人或拥有管理权限的用户执行。
- 完成任务前必须至少通过领域服务执行完整性校验；首个里程碑校验已选流程已生成工步，具体业务完结门槛可在后续迭代扩展。

## 9. HTTP API

所有 101 路由使用 `/api/101` 前缀。除应用健康检查和 Admin.NET 现有登录接口外，均要求 JWT，并接入 Admin.NET 接口权限。

### 9.1 主数据

```text
GET    /api/101/tasks
POST   /api/101/tasks
GET    /api/101/tasks/{id}
PUT    /api/101/tasks/{id}
DELETE /api/101/tasks/{id}
PUT    /api/101/tasks/{id}/status

GET    /api/101/personnel
POST   /api/101/personnel
GET    /api/101/personnel/{id}
PUT    /api/101/personnel/{id}
DELETE /api/101/personnel/{id}

GET    /api/101/devices
POST   /api/101/devices
GET    /api/101/devices/{id}
PUT    /api/101/devices/{id}
DELETE /api/101/devices/{id}

GET    /api/101/documents
POST   /api/101/documents
GET    /api/101/documents/{id}
PUT    /api/101/documents/{id}
DELETE /api/101/documents/{id}

GET    /api/101/workflows
POST   /api/101/workflows
PUT    /api/101/workflows/{id}
DELETE /api/101/workflows/{id}
GET    /api/101/workflows/tree
```

列表接口支持分页、关键词和页面需要的状态或分类筛选。响应继续使用 Admin.NET 统一响应格式。

### 9.2 文件内容

```text
POST   /api/101/documents/{documentId}/files
GET    /api/101/files/{fileId}/download
DELETE /api/101/files/{fileId}
```

上传接口验证权限、文件名、100 MB 默认大小上限和空文件。首版允许 `.pdf`、`.doc`、`.docx`、`.xls`、`.xlsx`、`.ppt`、`.pptx`、`.txt`、`.csv`、`.zip`、`.jpg`、`.jpeg`、`.png`；允许列表和大小上限可通过非秘密配置调整。服务器使用随机 Guid 作为存储名，保留安全扩展名，数据库保存原始文件名和相对路径。下载接口只接受文件记录 ID，不接受磁盘路径。

文件写入采用补偿式一致性：磁盘写入成功而数据库事务失败时删除新文件；数据库记录存在但磁盘文件不存在时返回明确错误并记录服务器日志。文件删除先完成业务引用检查，再更新数据库并删除文件；磁盘删除失败时记录可重试错误，不伪装成完整成功。

### 9.3 任务计划与准备

```text
GET /api/101/tasks/{taskId}/plan
PUT /api/101/tasks/{taskId}/plan

GET /api/101/tasks/{taskId}/preparation
PUT /api/101/tasks/{taskId}/personnel
PUT /api/101/tasks/{taskId}/devices
PUT /api/101/tasks/{taskId}/documents
GET /api/101/tasks/{taskId}/transfers/{tableId}
PUT /api/101/tasks/{taskId}/transfers/{tableId}
```

准备接口接受前端最终选择集合，后端计算新增和删除项，并通过唯一约束保证幂等。接口不得相信前端传入的人员、设备或文件显示文本，必须根据 ID 查询有效来源记录。测量传递表按前端已有 `tableId` 分区保存，每行保留稳定 ID、显示顺序和经过字段白名单校验的 `jsonb` 数据，不接受任意表名或任意服务器字段。

### 9.4 流程、工步与签署

```text
GET    /api/101/tasks/{taskId}/workflow
PUT    /api/101/tasks/{taskId}/workflow
GET    /api/101/tasks/{taskId}/operations
GET    /api/101/operations/{id}
PUT    /api/101/operations/{id}
PUT    /api/101/operations/{id}/checks
POST   /api/101/operations/{id}/signatures
DELETE /api/101/operations/{id}/signatures/{role}
```

流程树按任务所属部门筛选可选节点。保存流程时在后端生成或移除工步，不要求前端自行构造 `Operation101`。工步状态限定为“未开始”“进行中”“已完成”。

签署请求只提交角色，不提交签署人姓名与时间。签署接口需要 `101:operation:sign` 权限；撤销接口除本人外需要单独的管理权限。

## 10. 错误处理与安全

统一区分以下结果：

- 参数无效：HTTP 400，并返回具体字段错误。
- 未登录或令牌失效：HTTP 401。
- 已登录但无接口权限：HTTP 403。
- 记录不存在：HTTP 404。
- 引用冲突、已完成任务修改、重复业务操作：HTTP 409，并返回面向用户的中文消息。
- 未处理的服务器错误：HTTP 500，响应不包含堆栈、连接字符串或磁盘路径；详细信息只进入受控服务器日志。

文件路径必须通过固定根目录与后端生成的相对路径组合，并校验最终规范化路径仍位于根目录内，防止路径穿越。上传原文件名只作为展示元数据，不参与物理路径计算。

所有写接口使用 DTO 白名单映射，禁止前端修改创建人、签署人、删除标记等审计字段。数据库密码不得通过 Swagger 参数、配置接口或健康检查返回。

## 11. 种子数据

保留 Admin.NET 登录、用户、角色、菜单和权限运行所需的最小系统种子。101 模块提供与当前 Vue 前端演示数据等价的任务、人员、设备、文件和流程种子。

每类种子独立检查目标表是否为空：

- 空表时在事务内写入该类完整种子。
- 非空表时完全跳过该类种子，不补写、不更新、不覆盖。
- 任务准备关联种子只在依赖的主数据种子已经存在且关联表为空时写入。
- 种子使用固定 Guid，保证自动化测试和前后端演示引用稳定。

后端不在运行时读取前端 TypeScript 文件。实施时将确认过的演示数据转为 Core101 内部种子定义或 JSON 资源。

## 12. 测试与验证

解决方案增加独立测试项目，至少覆盖：

### 12.1 领域测试

- 已完成任务拒绝修改子数据。
- 被任务引用的人员、设备和文件拒绝删除。
- 相同选择重复保存保持单条记录。
- 流程选择新增工步，取消流程同步处理可移除工步。
- 删除任务清理完整任务聚合。
- 签署使用当前用户身份，不能伪造签署人和时间。

### 12.2 API 与权限测试

- 未登录访问 101 API 返回 401。
- 无权限用户访问受保护接口返回 403。
- 分页、关键词与状态筛选返回稳定结果。
- 不存在的 ID 返回 404。
- 业务冲突返回 409 和可展示消息。

### 12.3 文件测试

- 上传后原文件名与物理存储名分离。
- 路径穿越文件名不能逃逸存储根目录。
- 数据库保存失败时清理刚写入的物理文件。
- 磁盘文件丢失时下载返回明确错误。
- 删除仍被业务引用的文件被拒绝。

### 12.4 交付验证

- `dotnet test Admin.NET.sln` 退出码为 0。
- `dotnet build Admin.NET.sln --configuration Release` 退出码为 0。
- 使用已配置的测试 PostgreSQL 执行启动、CodeFirst、种子和健康检查冒烟验证。
- 使用真实登录令牌验证一条任务创建、准备配置、流程选择、工步查询和签署链路。
- 扫描所有受版本控制文件，确认不存在密码、带密码连接字符串、服务器绝对存储路径、旧数据库、日志、上传文件或构建输出。

## 13. 文档与运行说明

仓库随代码提供：

- `README.md`：环境要求、项目结构、启动步骤和 Swagger 地址。
- `.env.example`：只列出 `TCP101_DB_PASSWORD`、`TCP101_FILE_STORAGE_PATH` 等变量名和说明；密码值保持空白。
- PostgreSQL 初始化前提和环境变量设置示例。
- 前端联调用的 API 路由、认证方式和统一错误约定。
- 演示种子初始化规则与清空测试数据后的恢复方式。

## 14. 首个里程碑验收标准

- 新解决方案只包含四个必要 Admin.NET 底座项目和两个 101 独立模块。
- 旧 511、ISIP、插件、OCR、FFmpeg、第三方业务集成及其大体积资源不在新仓库中。
- 应用在缺少 `TCP101_DB_PASSWORD` 时安全失败，仓库中不存在数据库密码。
- 应用连接 `tcp101` 的 `public` Schema，并能初始化系统表、101 表和空表种子。
- Admin.NET 登录、用户、角色、菜单和接口权限可用。
- 任务、人员、设备、文件、流程、工步、检查项和签署的首批 API 可从 Swagger 调用。
- 文件能够安全上传到服务器目录并通过受保护接口下载。
- 主流程可完成：登录 → 创建或选择任务 → 保存计划 → 配置人员/设备/文件 → 选择流程 → 自动生成工步 → 保存检查项 → 当前用户签署。
- 已完成任务只读、引用删除保护和签署身份防伪由自动化测试证明。
- 完整测试、Release 构建和秘密扫描通过；具备 PostgreSQL 环境时，数据库与主流程冒烟验证通过。

## 15. 延后范围

以下内容不属于首个里程碑：

- 风险分析 API。
- 试验总结 API。
- 异常叫停 API。
- MinIO、云 OSS 或分布式文件存储。
- 移动端离线同步和多节点文件复制。
- 对现有 Vue 前端进行完整 HTTP Repository 改造。
- 将业务角色与 Admin.NET 组织岗位做更细粒度的自动映射；首版通过接口权限和允许角色集合控制签署。

# ABP 9.1.1 Modules 代码关系说明

## 一、整体定位

这个 `modules/` 目录是 ABP Framework 的**底层模块源码**。你用 ABP Studio 脚手架创建的项目，通过 NuGet 包引用的 `Volo.Abp.Identity.*`、`Volo.Abp.Account.*` 等包，源代码就在这里。要改底层行为（加字段、改逻辑、关接口），只能从这里改。

## 二、18 个模块一览

| 模块 | 职责 | 是否被其他模块依赖 |
|------|------|-------------------|
| **users** | 用户抽象层，定义 `IUser` 接口和基础常量 | 是，identity 和 cms-kit 依赖它 |
| **identity** | 用户管理（CRUD）、角色、权限分配，基于 ASP.NET Core Identity | 是，核心模块，被 6+ 模块依赖 |
| **account** | 登录、注册、忘记密码、个人资料管理 | 是，被 blogging/docs/cms-kit 等依赖 |
| **permission-management** | 权限持久化存储和管理 UI | 是，核心模块，被 6+ 模块依赖 |
| **feature-management** | 功能特性管理（付费版/免费版等功能开关） | 是，被 tenant-management 依赖 |
| **tenant-management** | 多租户管理 | 被 cms-kit/openiddict 的 demo 应用依赖 |
| **setting-management** | 系统设置持久化和管理 UI | 被 blogging/cms-kit/docs 依赖 |
| **audit-logging** | 审计日志记录 | 被 cms-kit 依赖 |
| **openiddict** | OAuth2/OpenID Connect 认证（新版推荐） | 被 account 依赖 |
| **identityserver** | IdentityServer4 认证（旧版，逐步淘汰） | 被 account 依赖 |
| **background-jobs** | 后台任务持久化 | 独立，不被其他模块依赖 |
| **blob-storing-database** | 二进制文件存数据库 | 被 blogging/cms-kit 依赖 |
| **basic-theme** | 基础 UI 主题 | 被多个 demo 应用依赖 |
| **blogging** | 博客功能模块 | 独立的业务模块 |
| **cms-kit** | CMS 内容管理套件 | 独立的业务模块（但依赖很多底层模块） |
| **docs** | 文档系统模块 | 独立的业务模块 |
| **client-simulation** | 客户端模拟测试工具 | 独立工具 |
| **virtual-file-explorer** | 虚拟文件系统浏览器 | 独立工具 |

## 三、核心依赖链（最重要，改代码必须理解）

### 第一层：基础抽象（无跨模块依赖）

- `users` — 定义 `IUser` 接口、`AbpUserConsts`（用户名/邮箱等最大长度）
- `permission-management` — 权限持久化，仅依赖 framework
- `feature-management` — 功能开关，仅依赖 framework
- `setting-management` — 设置存储，仅依赖 framework
- `audit-logging` — 审计日志，仅依赖 framework
- `background-jobs` — 后台任务，仅依赖 framework

### 第二层：核心业务（依赖第一层）

- **identity** 依赖 `users` + `permission-management`
  - `IdentityUser` 实现了 `users` 模块定义的 `IUser` 接口
  - `IdentityUserAppService` 管理用户的增删改查
  - Application 层依赖 `permission-management` 做权限校验

- **identityserver** 依赖 `identity`
  - 在 `identity` 的用户体系上提供 IdentityServer4 的 OAuth 能力

- **openiddict** 依赖 `identity`
  - 在 `identity` 的用户体系上提供 OpenIddict 的 OAuth 能力

### 第三层：用户界面（依赖第一、二层）

- **account** 依赖 `identity` + `openiddict`/`identityserver`
  - 提供登录注册页面和个人资料管理
  - `ProfileAppService` 操作的其实是 `IdentityUser` 实体
  - Web.OpenIddict 和 Web.IdentityServer 是两套认证方案的 Web 适配

- **tenant-management** 依赖 `feature-management`
  - 租户管理 UI 中嵌入功能特性管理

### 第四层：完整应用（依赖多个模块）

- `cms-kit` 依赖 `users` + `setting-management` + `blob-storing-database` + 更多
- `blogging` 依赖 `identity` + `permission-management` + `setting-management` + `blob-storing-database`
- `docs` 依赖 `identity` + `permission-management` + `setting-management`

## 四、users 和 identity 的关系（最容易混淆）

`users` 模块是**抽象层**，`identity` 是**实现层**：

- `users` 定义了 `IUser` 接口（UserName、Email、Name、Surname、PhoneNumber 等基础字段）和 `AbpUserConsts`（字段长度限制）
- `identity` 的 `IdentityUser` 实体**实现**了 `IUser` 接口，并在此基础上增加了：密码哈希、安全戳、双因素认证、锁定机制、角色集合、Claims、OpenId 等
- `identity` 的 EF Core 配置**先调用** `users` 模块的 `ConfigureAbpUser()` 映射基础字段，**再配置**自己特有的字段
- `IdentityUserConsts` 的长度常量**引用** `AbpUserConsts`（如 `MaxUserNameLength = AbpUserConsts.MaxUserNameLength`）

**改字段的影响**：如果你要加的字段属于"基础用户信息"（所有用户类型都需要的），改 `users` 模块；如果是"身份认证相关"的字段（如 OpenId），改 `identity` 模块就够了。

### IdentityUser 完整继承链

```
IdentityUser
  └─ FullAuditedAggregateRoot<Guid>    （ABP 框架基类，提供审计字段）
      └─ AggregateRoot<Guid>            （DDD 聚合根）
          └─ Entity<Guid>               （基础实体）

实现的接口：
  ├─ IUser                （来自 users 模块，基础用户属性契约）
  ├─ IHasEntityVersion    （实体版本号，乐观并发）
  ├─ IMultiTenant         （多租户，TenantId）
  └─ IHasExtraProperties  （动态扩展属性）
```

### IUser 接口定义的基础字段

| 字段 | 类型 | 来源 |
|------|------|------|
| Id | Guid | Entity 基类 |
| TenantId | Guid? | IMultiTenant |
| UserName | string | IUser |
| Email | string | IUser |
| Name | string | IUser |
| Surname | string | IUser |
| IsActive | bool | IUser |
| EmailConfirmed | bool | IUser |
| PhoneNumber | string | IUser |
| PhoneNumberConfirmed | bool | IUser |

### IdentityUser 额外增加的字段

| 字段 | 用途 |
|------|------|
| NormalizedUserName / NormalizedEmail | 大写标准化，用于快速查找 |
| PasswordHash / SecurityStamp | 密码安全 |
| TwoFactorEnabled | 双因素认证 |
| LockoutEnd / LockoutEnabled / AccessFailedCount | 账户锁定机制 |
| ShouldChangePasswordOnNextLogin / LastPasswordChangeTime | 密码策略 |
| IsExternal | 是否外部认证用户 |
| OpenId | 微信 OpenId 等（你的 fork 新增的） |
| Roles / Claims / Logins / Tokens / OrganizationUnits | 导航属性集合 |

## 五、每个模块内部的分层结构

每个模块内部都遵循 DDD 分层，层与层之间的依赖关系是**单向的**：

```
Domain.Shared（常量、枚举、共享类型）
    ↑
Domain（实体、仓储接口、领域服务）
    ↑
Application.Contracts（DTO、应用服务接口）
    ↑
Application（应用服务实现、AutoMapper Profile）
    ↑
HttpApi（REST Controller，薄层代理到 Application）
    ↑
HttpApi.Client（远程调用代理）

EntityFrameworkCore / MongoDB（仓储实现，依赖 Domain）
Web / Blazor（UI 层，依赖 Application.Contracts）
```

### 各层职责详解

| 层 | 包含什么 | 修改场景 |
|----|---------|---------|
| **Domain.Shared** | 常量类（`XXXConsts`）、枚举、本地化资源 JSON | 加字段长度限制、加枚举值 |
| **Domain** | 实体类、仓储接口（`IXXXRepository`）、领域服务（`XXXManager`） | 加实体字段、加领域方法 |
| **Application.Contracts** | DTO 类、应用服务接口（`IXXXAppService`）、权限定义 | 加 DTO 字段、定义新接口 |
| **Application** | 应用服务实现、AutoMapper Profile | 改业务逻辑、加 DTO 映射 |
| **HttpApi** | REST Controller | 通常不需要改（薄代理层） |
| **HttpApi.Client** | HTTP 客户端代理 | 通常不需要改（自动生成） |
| **EntityFrameworkCore** | `DbContext`、`ModelBuilder` 配置、仓储实现 | 加数据库列映射、改查询 |
| **MongoDB** | MongoDB 仓储实现 | 同上，MongoDB 版 |
| **Web** | MVC Razor Pages、ViewComponent | 改 MVC 前端 |
| **Blazor** | Blazor 组件 | 改 Blazor 前端 |

### 修改传播规则

- 改 `Domain.Shared`（加常量）→ 被所有上层引用，影响最广但改动最小
- 改 `Domain`（加实体字段）→ 需要同步改 DTO、EF 映射、AppService
- 改 `Application.Contracts`（加 DTO 字段）→ 上层 HttpApi 和 UI 自动获得
- 改 `Application`（改业务逻辑）→ 影响范围最小
- 改 `HttpApi`（改 Controller）→ 只影响 REST 接口

## 六、跨模块修改的波及范围

### 为什么改用户要同时改 identity 和 account？

因为 `identity` 管**用户管理**（管理员在后台增删改查用户），`account` 管**个人资料**（用户自己改自己的信息）。同一个 `IdentityUser` 实体，两个入口都能操作它，所以 DTO 和 AppService 两边都要加。

### 以加 OpenId 字段为例（当前分支的改动）

**identity 模块（6 个文件）：**

| # | 文件 | 改什么 |
|---|------|--------|
| 1 | `IdentityUserConsts.cs` | 加 `MaxOpenIdLength = 128` |
| 2 | `IdentityUser.cs` | 加 `public virtual string OpenId { get; set; }` |
| 3 | `IdentityUserDto.cs` | 加 `public string OpenId { get; set; }` |
| 4 | `IdentityUserCreateOrUpdateDtoBase.cs` | 加 `OpenId` 属性 + `[DynamicStringLength]` 校验 |
| 5 | `IdentityDbContextModelBuilderExtensions.cs` | 加 EF Core 列映射 |
| 6 | `IdentityUserAppService.cs` | 在 `UpdateUserByInput` 加赋值 |

**account 模块（5 个文件）：**

| # | 文件 | 改什么 |
|---|------|--------|
| 7 | `ProfileDto.cs` | 加 `OpenId` 输出字段 |
| 8 | `UpdateProfileDto.cs` | 加 `OpenId` 输入字段 + 校验 |
| 9 | `ProfileAppService.cs` | 在 `UpdateAsync` 加赋值 |
| 10 | `AccountProfilePersonalInfoManagementGroupViewComponent.cs` | Web MVC 模型加字段 |
| 11 | `AccountManage.razor.cs` | Blazor 模型加字段 |

**不需要改的文件：**
- AutoMapper Profile — 同名属性自动映射
- Controller — 薄代理层，不涉及字段
- `IdentityUserCreateDto` / `IdentityUserUpdateDto` — 继承自 Base，自动获得新字段

### 常见修改场景速查

| 场景 | 要改的模块 | 大约文件数 |
|------|-----------|-----------|
| 给用户加字段 | identity + account | ~11 个 |
| 改登录/注册逻辑 | account | 2-4 个 |
| 改用户 CRUD 逻辑 | identity 的 Application 层 | 2-3 个 |
| 加权限定义 | identity（`Permissions.cs` + `PermissionDefinitionProvider.cs`） | 2 个 |
| 改 EF Core 数据库映射 | identity 的 EntityFrameworkCore 层 | 1-2 个 |
| 改认证流程 | openiddict 或 identityserver | 取决于改动范围 |
| 改多租户逻辑 | tenant-management | 2-5 个 |
| 改权限管理 UI | permission-management 的 Web/Blazor 层 | 2-4 个 |
| 改功能开关逻辑 | feature-management | 2-3 个 |

## 七、ABP Module 类的 DependsOn 关系（关键模块）

每个层都有一个 `AbpXxxModule.cs` 类，通过 `[DependsOn]` 特性声明对其他模块层的依赖。这决定了模块加载顺序和 DI 注册顺序。

### identity 模块的依赖链

```
AbpIdentityDomainSharedModule
  ├─ AbpUsersDomainSharedModule          ← 依赖 users 的共享层
  ├─ AbpValidationModule                  ← framework
  └─ AbpFeaturesModule                    ← framework

AbpIdentityDomainModule
  ├─ AbpIdentityDomainSharedModule        ← 自己的共享层
  ├─ AbpUsersDomainModule                 ← 依赖 users 的领域层
  ├─ AbpDddDomainModule                   ← framework DDD 基类
  └─ AbpAutoMapperModule                  ← framework AutoMapper

AbpIdentityApplicationContractsModule
  ├─ AbpIdentityDomainSharedModule
  ├─ AbpUsersAbstractionModule            ← 依赖 users 的抽象层
  ├─ AbpAuthorizationModule               ← framework 授权
  └─ AbpPermissionManagementApplicationContractsModule  ← 依赖权限管理

AbpIdentityApplicationModule
  ├─ AbpIdentityDomainModule
  ├─ AbpIdentityApplicationContractsModule
  ├─ AbpAutoMapperModule
  └─ AbpPermissionManagementApplicationModule  ← 依赖权限管理
```

### account 模块的依赖链

```
AbpAccountApplicationContractsModule
  └─ AbpIdentityApplicationContractsModule    ← 依赖 identity 的契约层

AbpAccountApplicationModule
  ├─ AbpAccountApplicationContractsModule
  ├─ AbpIdentityApplicationModule             ← 依赖 identity 的应用层
  ├─ AbpUiNavigationModule                    ← framework UI 导航
  └─ AbpEmailingModule                        ← framework 邮件

AbpAccountHttpApiModule
  ├─ AbpAccountApplicationContractsModule
  ├─ AbpIdentityHttpApiModule                 ← 依赖 identity 的 HttpApi
  └─ AbpAspNetCoreMvcModule                   ← framework MVC

AbpAccountWebModule
  ├─ AbpAccountApplicationContractsModule
  ├─ AbpIdentityAspNetCoreModule              ← 依赖 identity 的 AspNetCore
  ├─ AbpAutoMapperModule
  └─ AbpAspNetCoreMvcUiThemeSharedModule      ← framework 主题
```

## 八、framework 的关系

`modules/` 同级有个 `framework/` 目录（`../framework/`），它是 ABP 的核心框架代码，提供：

- **DDD 基类**：`Entity`、`AggregateRoot`、`FullAuditedAggregateRoot`
- **基础设施**：`Caching`、`EventBus`、`Authorization`、`Validation`
- **ASP.NET Core 集成**：`AspNetCoreMvc`、`AutoMapper`
- **数据访问抽象**：`EntityFrameworkCore`、`MongoDB`

所有 modules 的 `.csproj` 都通过 `ProjectReference` 引用 framework。如果你发现某个行为需要改但不在 modules 里，很可能在 framework 里。

## 九、NuGet 版本管理

所有 NuGet 包版本集中在仓库根目录的 `Directory.Packages.props` 文件中管理。各模块的 `.csproj` 只写 `<PackageReference Include="xxx" />` 不写版本号。要升级某个依赖，只改根目录这一个文件。

## 十、构建和测试命令

每个模块有独立的 `.sln` 文件，分别构建：

```bash
# 构建单个模块
dotnet build identity/Volo.Abp.Identity.sln

# 运行模块全部测试
dotnet test identity/Volo.Abp.Identity.sln

# 运行单个测试项目
dotnet test identity/test/Volo.Abp.Identity.Application.Tests

# 运行指定测试方法
dotnet test identity/test/Volo.Abp.Identity.Application.Tests --filter "FullyQualifiedName~TestMethodName"
```

## 十一、数据库迁移

本项目没有使用 EF Core Migrations（`dotnet ef migrations add`），数据库结构变更通过**直接执行 SQL** 完成：

```sql
-- 字符串字段（可空）
ALTER TABLE AbpUsers ADD {FieldName} NVARCHAR({MaxLength}) NULL;

-- 字符串字段（必填）
ALTER TABLE AbpUsers ADD {FieldName} NVARCHAR({MaxLength}) NOT NULL DEFAULT '';

-- 整数字段
ALTER TABLE AbpUsers ADD {FieldName} INT NOT NULL DEFAULT 0;

-- 布尔字段
ALTER TABLE AbpUsers ADD {FieldName} BIT NOT NULL DEFAULT 0;
```

---

## 十二、实战教程：给用户表加一个字段

下面以实际项目中加 `OpenId`（string, 可空, 最大长度 128）字段为完整例子，**逐文件**说明改什么、在哪改、代码怎么写。

> 如果你要加的是其他字段，把所有 `OpenId` 替换成你的字段名，`128` 替换成你的最大长度即可。

### 概览：需要改 11 个文件

| 步骤 | 模块 | 文件 | 改动目的 |
|------|------|------|---------|
| 1 | identity | `IdentityUserConsts.cs` | 定义字段最大长度常量 |
| 2 | identity | `IdentityUser.cs` | 给实体类加属性 |
| 3 | identity | `IdentityUserDto.cs` | API 输出 DTO 加字段 |
| 4 | identity | `IdentityUserCreateOrUpdateDtoBase.cs` | API 输入 DTO 加字段 + 校验 |
| 5 | identity | `IdentityDbContextModelBuilderExtensions.cs` | EF Core 数据库列映射 |
| 6 | identity | `IdentityUserAppService.cs` | 用户管理的赋值逻辑 |
| 7 | account | `ProfileDto.cs` | 个人资料输出 DTO |
| 8 | account | `UpdateProfileDto.cs` | 个人资料输入 DTO + 校验 |
| 9 | account | `ProfileAppService.cs` | 个人资料更新赋值 |
| 10 | account | `AccountProfilePersonalInfoManagementGroupViewComponent.cs` | Web MVC 页面模型 |
| 11 | account | `AccountManage.razor.cs` | Blazor 页面模型 |

### 不需要改的文件（理解为什么不用改）

- **AutoMapper Profile** — ABP 使用约定映射，只要 DTO 和实体的属性名一致，自动映射
- **Controller**（`AccountController.cs` 等）— 纯薄代理，透传到 AppService，不涉及字段
- **`IdentityUserCreateDto` / `IdentityUserUpdateDto`** — 继承自 `IdentityUserCreateOrUpdateDtoBase`，自动获得新字段
- **`IIdentityUserAppService` / `IProfileAppService`** — 接口参数用的是上述 DTO，不需要改接口定义

---

### 步骤 1：定义常量 — `IdentityUserConsts.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.Domain.Shared/Volo/Abp/Identity/IdentityUserConsts.cs
```

**在类的末尾加：**

```csharp
/// <summary>
/// Default value: 128
/// </summary>
public static int MaxOpenIdLength { get; set; } = 128;
```

**完整上下文（加在 `MaxLoginProviderLength` 后面）：**

```csharp
public static class IdentityUserConsts
{
    // ... 已有的常量 ...

    /// <summary>
    /// Default value: 16
    /// </summary>
    public static int MaxLoginProviderLength { get; set; } = 16;

    /// <summary>                          // ← 新增开始
    /// Default value: 128
    /// </summary>
    public static int MaxOpenIdLength { get; set; } = 128;
                                                       // ← 新增结束
}
```

**为什么用 `{ get; set; }` 而不是 `const`？** 因为 ABP 的设计允许应用层在启动时覆盖这些值（比如把最大长度改成 256），所以用可写属性。

---

### 步骤 2：加实体属性 — `IdentityUser.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.Domain/Volo/Abp/Identity/IdentityUser.cs
```

**在 `IsExternal` 属性后面加：**

```csharp
[CanBeNull]
public virtual string OpenId { get; set; }
```

**完整上下文：**

```csharp
public virtual bool IsExternal { get; set; }

[CanBeNull]                                    // ← 新增开始
public virtual string OpenId { get; set; }     // ← 新增结束

/// <summary>
/// Gets or sets a telephone number for the user.
/// </summary>
public virtual string PhoneNumber { get; protected internal set; }
```

**注意点：**
- `virtual` — ABP 实体的属性都要加 `virtual`，用于延迟加载和代理
- `[CanBeNull]` — JetBrains 注解，标记可空（如果是必填字段则不加）
- `{ get; set; }` — 普通可读写。如果需要通过 Manager 才能改（像 Email 那样），则用 `{ get; protected internal set; }`

---

### 步骤 3：输出 DTO — `IdentityUserDto.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.Application.Contracts/Volo/Abp/Identity/IdentityUserDto.cs
```

**在 `LockoutEnabled` 后面加：**

```csharp
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
public bool LockoutEnabled { get; set; }

public string OpenId { get; set; }        // ← 新增

public int AccessFailedCount { get; set; }
```

**说明：** 这个 DTO 用于 GET 接口返回用户信息。AutoMapper 会自动把 `IdentityUser.OpenId` 映射到 `IdentityUserDto.OpenId`（同名自动映射）。

---

### 步骤 4：输入 DTO — `IdentityUserCreateOrUpdateDtoBase.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.Application.Contracts/Volo/Abp/Identity/IdentityUserCreateOrUpdateDtoBase.cs
```

**在 `LockoutEnabled` 后面加：**

```csharp
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
public bool LockoutEnabled { get; set; }

[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]  // ← 新增开始
public string OpenId { get; set; }                                                              // ← 新增结束

[CanBeNull]
public string[] RoleNames { get; set; }
```

**注意点：**
- `[DynamicStringLength]` 是 ABP 的动态校验注解，运行时从 `IdentityUserConsts` 读取最大长度
- 不要用 `[MaxLength(128)]` 硬编码，因为 ABP 允许运行时覆盖长度
- 这个 Base 类被 `IdentityUserCreateDto` 和 `IdentityUserUpdateDto` 继承，所以创建和更新接口都自动获得此字段
- 如果是必填字段，加 `[Required]`

---

### 步骤 5：EF Core 列映射 — `IdentityDbContextModelBuilderExtensions.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.EntityFrameworkCore/Volo/Abp/Identity/EntityFrameworkCore/IdentityDbContextModelBuilderExtensions.cs
```

**在 `IsExternal` 映射后面加：**

```csharp
b.Property(u => u.OpenId).HasMaxLength(IdentityUserConsts.MaxOpenIdLength)
    .HasColumnName(nameof(IdentityUser.OpenId));
```

**完整上下文：**

```csharp
b.Property(u => u.IsExternal).IsRequired().HasDefaultValue(false)
    .HasColumnName(nameof(IdentityUser.IsExternal));

b.Property(u => u.OpenId).HasMaxLength(IdentityUserConsts.MaxOpenIdLength)     // ← 新增开始
    .HasColumnName(nameof(IdentityUser.OpenId));                                // ← 新增结束

b.Property(u => u.AccessFailedCount)
    .If(!builder.IsUsingOracle(), p => p.HasDefaultValue(0))
    .HasColumnName(nameof(IdentityUser.AccessFailedCount));
```

**常用配置模板：**

```csharp
// 可空字符串
b.Property(u => u.OpenId).HasMaxLength(IdentityUserConsts.MaxOpenIdLength)
    .HasColumnName(nameof(IdentityUser.OpenId));

// 必填字符串
b.Property(u => u.OpenId).IsRequired().HasMaxLength(IdentityUserConsts.MaxOpenIdLength)
    .HasColumnName(nameof(IdentityUser.OpenId));

// 布尔值（带默认值）
b.Property(u => u.IsVerified).IsRequired().HasDefaultValue(false)
    .HasColumnName(nameof(IdentityUser.IsVerified));

// 整数（带默认值）
b.Property(u => u.Score).HasDefaultValue(0)
    .HasColumnName(nameof(IdentityUser.Score));
```

---

### 步骤 6：AppService 赋值 — `IdentityUserAppService.cs`

**文件路径：**
```
modules/identity/src/Volo.Abp.Identity.Application/Volo/Abp/Identity/IdentityUserAppService.cs
```

**找到 `UpdateUserByInput` 方法，在 `user.Surname` 赋值后面加：**

```csharp
user.OpenId = input.OpenId?.Trim();
```

**完整上下文：**

```csharp
user.Name = input.Name?.Trim();
user.Surname = input.Surname?.Trim();
user.OpenId = input.OpenId?.Trim();          // ← 新增
(await UserManager.UpdateAsync(user)).CheckErrors();
```

**注意：** `CreateAsync` 方法中**不需要**手动赋值 OpenId，因为 `CreateAsync` 使用 AutoMapper 从 DTO 映射到 Entity，同名属性会自动映射。只有 `UpdateUserByInput` 是手动赋值的（因为更新逻辑更复杂，需要逐字段处理）。

---

### 到这里 identity 模块改完了。下面是 account 模块（5 个文件）。

---

### 步骤 7：Profile 输出 DTO — `ProfileDto.cs`

**文件路径：**
```
modules/account/src/Volo.Abp.Account.Application.Contracts/Volo/Abp/Account/ProfileDto.cs
```

**在 `HasPassword` 后面加：**

```csharp
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
public bool HasPassword { get; set; }

public string OpenId { get; set; }          // ← 新增

public string ConcurrencyStamp { get; set; }
```

---

### 步骤 8：Profile 输入 DTO — `UpdateProfileDto.cs`

**文件路径：**
```
modules/account/src/Volo.Abp.Account.Application.Contracts/Volo/Abp/Account/UpdateProfileDto.cs
```

**在 `PhoneNumber` 后面加：**

```csharp
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
public string PhoneNumber { get; set; }

[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]  // ← 新增开始
public string OpenId { get; set; }                                                              // ← 新增结束

public string ConcurrencyStamp { get; set; }
```

**注意：** 这个文件引用了 `Volo.Abp.Identity` 命名空间来使用 `IdentityUserConsts`，`using` 已经存在，不需要额外加。

---

### 步骤 9：ProfileAppService 赋值 — `ProfileAppService.cs`

**文件路径：**
```
modules/account/src/Volo.Abp.Account.Application/Volo/Abp/Account/ProfileAppService.cs
```

**找到 `UpdateAsync` 方法，在 `user.Surname` 赋值后面加：**

```csharp
user.OpenId = input.OpenId?.Trim();
```

**完整上下文：**

```csharp
user.Name = input.Name?.Trim();
user.Surname = input.Surname?.Trim();
user.OpenId = input.OpenId?.Trim();          // ← 新增

input.MapExtraPropertiesTo(user);
```

---

### 步骤 10：Web MVC 页面模型 — `AccountProfilePersonalInfoManagementGroupViewComponent.cs`

**文件路径：**
```
modules/account/src/Volo.Abp.Account.Web/Pages/Account/Components/ProfileManagementGroup/PersonalInfo/AccountProfilePersonalInfoManagementGroupViewComponent.cs
```

**在 `PersonalInfoModel` 内部类的 `PhoneNumber` 后面加：**

```csharp
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]
[Display(Name = "DisplayName:OpenId")]
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
public class PersonalInfoModel : ExtensibleObject, IHasConcurrencyStamp
{
    // ... 已有字段 ...

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
    [Display(Name = "DisplayName:PhoneNumber")]
    public string PhoneNumber { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxOpenIdLength))]  // ← 新增开始
    [Display(Name = "DisplayName:OpenId")]
    public string OpenId { get; set; }                                                              // ← 新增结束

    [HiddenInput] public string ConcurrencyStamp { get; set; }
}
```

**说明：** `[Display(Name = "DisplayName:OpenId")]` 用于本地化显示名。如果需要中文显示，需要在对应的本地化 JSON 文件中加 `"DisplayName:OpenId": "微信OpenId"` 的翻译条目。

---

### 步骤 11：Blazor 页面模型 — `AccountManage.razor.cs`

**文件路径：**
```
modules/account/src/Volo.Abp.Account.Blazor/Pages/Account/AccountManage.razor.cs
```

**在文件底部的 `PersonalInfoModel` 类中，`EmailConfirmed` 后面加：**

```csharp
public string OpenId { get; set; }
```

**完整上下文：**

```csharp
public class PersonalInfoModel : ExtensibleObject
{
    // ... 已有字段 ...

    public bool EmailConfirmed { get; set; }

    public string OpenId { get; set; }          // ← 新增

    public string ConcurrencyStamp { get; set; }
}
```

---

### 步骤 12：数据库加列

代码改完后，需要在数据库执行 SQL：

```sql
-- 可空字符串字段
ALTER TABLE AbpUsers ADD OpenId NVARCHAR(128) NULL;
```

**其他类型的 SQL 模板：**

```sql
-- 必填字符串（带默认值）
ALTER TABLE AbpUsers ADD FieldName NVARCHAR(128) NOT NULL DEFAULT '';

-- 整数
ALTER TABLE AbpUsers ADD FieldName INT NOT NULL DEFAULT 0;

-- 布尔
ALTER TABLE AbpUsers ADD FieldName BIT NOT NULL DEFAULT 0;

-- 日期（可空）
ALTER TABLE AbpUsers ADD FieldName DATETIME2 NULL;
```

---

### 完成后验证

```bash
# 分别构建两个模块，确认编译通过
dotnet build identity/Volo.Abp.Identity.sln
dotnet build account/Volo.Abp.Account.sln
```

---

### 通用模板：给用户加任意字段

把上面的 `OpenId` 替换成你的字段名，按这个表格操作：

| 变量 | 含义 | 示例 |
|------|------|------|
| `{FieldName}` | 字段名（PascalCase） | `OpenId`、`NickName`、`Gender` |
| `{fieldType}` | C# 类型 | `string`、`int`、`bool`、`DateTime?` |
| `{MaxLength}` | 最大长度（仅 string） | `128`、`256`、`64` |
| `{DefaultValue}` | 数据库默认值 | `NULL`、`''`、`0`、`false` |

**string 字段的代码模板：**

```csharp
// 步骤 1: IdentityUserConsts.cs
public static int Max{FieldName}Length { get; set; } = {MaxLength};

// 步骤 2: IdentityUser.cs
[CanBeNull]
public virtual string {FieldName} { get; set; }

// 步骤 3: IdentityUserDto.cs
public string {FieldName} { get; set; }

// 步骤 4: IdentityUserCreateOrUpdateDtoBase.cs
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.Max{FieldName}Length))]
public string {FieldName} { get; set; }

// 步骤 5: IdentityDbContextModelBuilderExtensions.cs
b.Property(u => u.{FieldName}).HasMaxLength(IdentityUserConsts.Max{FieldName}Length)
    .HasColumnName(nameof(IdentityUser.{FieldName}));

// 步骤 6: IdentityUserAppService.cs（UpdateUserByInput 方法内）
user.{FieldName} = input.{FieldName}?.Trim();

// 步骤 7: ProfileDto.cs
public string {FieldName} { get; set; }

// 步骤 8: UpdateProfileDto.cs
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.Max{FieldName}Length))]
public string {FieldName} { get; set; }

// 步骤 9: ProfileAppService.cs（UpdateAsync 方法内）
user.{FieldName} = input.{FieldName}?.Trim();

// 步骤 10: AccountProfilePersonalInfoManagementGroupViewComponent.cs
[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.Max{FieldName}Length))]
[Display(Name = "DisplayName:{FieldName}")]
public string {FieldName} { get; set; }

// 步骤 11: AccountManage.razor.cs
public string {FieldName} { get; set; }
```

**bool 字段的代码模板：**

```csharp
// 步骤 1: IdentityUserConsts.cs — 不需要，bool 没有长度

// 步骤 2: IdentityUser.cs
public virtual bool {FieldName} { get; set; }

// 步骤 3: IdentityUserDto.cs
public bool {FieldName} { get; set; }

// 步骤 4: IdentityUserCreateOrUpdateDtoBase.cs
public bool {FieldName} { get; set; }

// 步骤 5: IdentityDbContextModelBuilderExtensions.cs
b.Property(u => u.{FieldName}).IsRequired().HasDefaultValue(false)
    .HasColumnName(nameof(IdentityUser.{FieldName}));

// 步骤 6: IdentityUserAppService.cs
user.{FieldName} = input.{FieldName};

// 步骤 7-11: 同 string 模板，把类型换成 bool
```

**int 字段的代码模板：**

```csharp
// 步骤 1: IdentityUserConsts.cs — 不需要，int 没有长度

// 步骤 2: IdentityUser.cs
public virtual int {FieldName} { get; set; }

// 步骤 3: IdentityUserDto.cs
public int {FieldName} { get; set; }

// 步骤 4: IdentityUserCreateOrUpdateDtoBase.cs
public int {FieldName} { get; set; }

// 步骤 5: IdentityDbContextModelBuilderExtensions.cs
b.Property(u => u.{FieldName}).HasDefaultValue(0)
    .HasColumnName(nameof(IdentityUser.{FieldName}));

// 步骤 6: IdentityUserAppService.cs
user.{FieldName} = input.{FieldName};

// 步骤 7-11: 同 string 模板，把类型换成 int，去掉校验注解
```

---

## 十三、核心要点总结

1. **users 是抽象，identity 是实现，account 是用户自助入口，permission-management 管权限存储** — 这四个模块关系最紧密
2. **改用户相关功能必须同时改 identity 和 account**，因为它们是同一实体的两个操作入口
3. **每个模块内部严格分层**，改动从 Domain.Shared 开始，逐层向上传播
4. **AutoMapper 基于同名属性自动映射**，加字段时只要命名一致就不需要改 Profile
5. **所有 NuGet 版本集中管理**在根目录 `Directory.Packages.props`
6. **framework 在 `../framework/`**，modules 只是上层业务模块
7. **加用户字段固定 11 个文件**，按本文档步骤 1-12 执行即可

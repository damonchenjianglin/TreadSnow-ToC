# ABP 9.1.1 引用关系说明

## 一、当前引用方式（NuGet 包）

TreadSnow 项目当前通过 **NuGet 包** 引用 ABP 模块，所有 `.csproj` 中是这样的：

```xml
<!-- TreadSnow.EntityFrameworkCore.csproj -->
<PackageReference Include="Volo.Abp.Identity.EntityFrameworkCore" Version="9.1.1" />

<!-- TreadSnow.Domain.csproj -->
<PackageReference Include="Volo.Abp.Identity.Domain" Version="9.1.1" />

<!-- TreadSnow.HttpApi.Host.csproj -->
<PackageReference Include="Volo.Abp.Account.Web.OpenIddict" Version="9.1.1" />
```

这意味着你用的是 NuGet 上发布的**预编译 DLL**，无法修改源码。你现在的做法是改 ABP9.1.1 源码 → 编译 DLL → 手动替换，非常容易出错。

## 二、ABP9.1.1 模块结构

```
ABP9.1.1/
├── framework/              ← ABP 核心框架（一般不需要改）
│   └── Volo.Abp.sln
├── modules/                ← ABP 功能模块（你要改的在这里）
│   ├── identity/           ← 用户管理（IdentityUser、角色、权限）
│   │   └── Volo.Abp.Identity.sln
│   ├── account/            ← 登录注册、个人资料
│   │   └── Volo.Abp.Account.sln
│   ├── openiddict/         ← OAuth2/OIDC 认证
│   ├── tenant-management/  ← 多租户管理
│   ├── permission-management/
│   ├── setting-management/
│   ├── feature-management/
│   ├── audit-logging/
│   ├── users/              ← IUser 接口抽象层
│   └── ...（共18个模块）
└── source-code/
    └── SourceCodes.sln     ← 聚合解决方案
```

## 三、Identity 模块内部分层（你最常改的）

```
identity/src/
├── Volo.Abp.Identity.Domain.Shared     ← 常量、枚举（如 MaxOpenIdLength）
├── Volo.Abp.Identity.Domain            ← 实体（IdentityUser.cs）
├── Volo.Abp.Identity.Application.Contracts ← DTO（IdentityUserDto.cs）
├── Volo.Abp.Identity.Application       ← 服务（IdentityUserAppService.cs）
├── Volo.Abp.Identity.EntityFrameworkCore ← EF映射（ModelBuilder配置）
├── Volo.Abp.Identity.HttpApi           ← REST控制器
└── Volo.Abp.Identity.HttpApi.Client    ← 客户端代理
```

## 四、TreadSnow 与 ABP 模块的依赖关系图

```
TreadSnow.HttpApi.Host
├── TreadSnow.Application
│   ├── TreadSnow.Application.Contracts
│   │   └── TreadSnow.Domain.Shared
│   └── TreadSnow.Domain
│       └── TreadSnow.Domain.Shared
├── TreadSnow.HttpApi
│   └── TreadSnow.Application.Contracts
├── TreadSnow.EntityFrameworkCore
│   └── TreadSnow.Domain
│
│  ===== 以下是 ABP 模块引用（当前是 NuGet 包）=====
│
├── Volo.Abp.Account.Web.OpenIddict       ← 登录注册UI + OpenIddict
├── Volo.Abp.Identity.EntityFrameworkCore  ← 用户表 EF 映射
├── Volo.Abp.Identity.Domain              ← IdentityUser 实体
├── Volo.Abp.Identity.Application         ← 用户管理 AppService
└── ...（其他 ABP 模块）
```

## 五、如何改为项目引用（推荐方案）

### 核心思路

将 NuGet `PackageReference` **替换为** 本地 `ProjectReference`，让 TreadSnow 直接引用 ABP9.1.1 源码项目。

### 步骤

#### 1. 确定你要修改的模块

比如你要改 Identity 和 Account，涉及的项目有：

| NuGet 包名 | 对应源码项目路径 |
|------------|-----------------|
| `Volo.Abp.Identity.Domain.Shared` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Domain.Shared` |
| `Volo.Abp.Identity.Domain` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Domain` |
| `Volo.Abp.Identity.Application.Contracts` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Application.Contracts` |
| `Volo.Abp.Identity.Application` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Application` |
| `Volo.Abp.Identity.EntityFrameworkCore` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.EntityFrameworkCore` |
| `Volo.Abp.Identity.HttpApi` | `ABP9.1.1/modules/identity/src/Volo.Abp.Identity.HttpApi` |
| `Volo.Abp.Account.Web.OpenIddict` | `ABP9.1.1/modules/account/src/Volo.Abp.Account.Web.OpenIddict` |
| `Volo.Abp.Account.Application` | `ABP9.1.1/modules/account/src/Volo.Abp.Account.Application` |
| `Volo.Abp.Account.Application.Contracts` | `ABP9.1.1/modules/account/src/Volo.Abp.Account.Application.Contracts` |

#### 2. 修改 .csproj 文件

以 `TreadSnow.EntityFrameworkCore.csproj` 为例：

```xml
<!-- 之前（NuGet 包） -->
<PackageReference Include="Volo.Abp.Identity.EntityFrameworkCore" Version="9.1.1" />

<!-- 之后（项目引用） -->
<ProjectReference Include="..\..\ABP9.1.1\modules\identity\src\Volo.Abp.Identity.EntityFrameworkCore\Volo.Abp.Identity.EntityFrameworkCore.csproj" />
```

以 `TreadSnow.Domain.csproj` 为例：

```xml
<!-- 之前 -->
<PackageReference Include="Volo.Abp.Identity.Domain" Version="9.1.1" />

<!-- 之后 -->
<ProjectReference Include="..\..\ABP9.1.1\modules\identity\src\Volo.Abp.Identity.Domain\Volo.Abp.Identity.Domain.csproj" />
```

#### 3. 将 ABP 模块项目加入 TreadSnow.sln

```bash
cd E:\TreadSnow\TreadSnow Github2

# Identity 模块
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Domain.Shared/Volo.Abp.Identity.Domain.Shared.csproj --solution-folder ABP-Identity
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Domain/Volo.Abp.Identity.Domain.csproj --solution-folder ABP-Identity
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Application.Contracts/Volo.Abp.Identity.Application.Contracts.csproj --solution-folder ABP-Identity
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.Application/Volo.Abp.Identity.Application.csproj --solution-folder ABP-Identity
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.EntityFrameworkCore/Volo.Abp.Identity.EntityFrameworkCore.csproj --solution-folder ABP-Identity
dotnet sln TreadSnow.sln add ABP9.1.1/modules/identity/src/Volo.Abp.Identity.HttpApi/Volo.Abp.Identity.HttpApi.csproj --solution-folder ABP-Identity

# Account 模块（如果也要改）
dotnet sln TreadSnow.sln add ABP9.1.1/modules/account/src/Volo.Abp.Account.Application.Contracts/Volo.Abp.Account.Application.Contracts.csproj --solution-folder ABP-Account
dotnet sln TreadSnow.sln add ABP9.1.1/modules/account/src/Volo.Abp.Account.Application/Volo.Abp.Account.Application.csproj --solution-folder ABP-Account
dotnet sln TreadSnow.sln add ABP9.1.1/modules/account/src/Volo.Abp.Account.Web.OpenIddict/Volo.Abp.Account.Web.OpenIddict.csproj --solution-folder ABP-Account
```

#### 4. 注意事项

- **只替换你要改的模块**，其他模块继续用 NuGet 包
- ABP 模块之间有内部依赖（如 identity 依赖 users），这些依赖由 ABP 模块自己的 csproj 管理，不需要你额外处理
- 替换后 `dotnet build` 验证一下，有冲突再调
- 以后要升级 ABP 版本时，未修改的模块直接改 NuGet 版本号，修改过的模块需要手动 merge 新版源码

## 六、替换对照表（快速查找）

| TreadSnow 项目 | 当前引用的 ABP NuGet 包 | 对应源码路径 |
|---|---|---|
| Domain | `Volo.Abp.Identity.Domain` | `modules/identity/src/Volo.Abp.Identity.Domain` |
| Domain | `Volo.Abp.OpenIddict.Domain` | `modules/openiddict/src/Volo.Abp.OpenIddict.Domain` |
| Domain | `Volo.Abp.TenantManagement.Domain` | `modules/tenant-management/src/Volo.Abp.TenantManagement.Domain` |
| EntityFrameworkCore | `Volo.Abp.Identity.EntityFrameworkCore` | `modules/identity/src/Volo.Abp.Identity.EntityFrameworkCore` |
| EntityFrameworkCore | `Volo.Abp.OpenIddict.EntityFrameworkCore` | `modules/openiddict/src/Volo.Abp.OpenIddict.EntityFrameworkCore` |
| EntityFrameworkCore | `Volo.Abp.TenantManagement.EntityFrameworkCore` | `modules/tenant-management/src/Volo.Abp.TenantManagement.EntityFrameworkCore` |
| HttpApi.Host | `Volo.Abp.Account.Web.OpenIddict` | `modules/account/src/Volo.Abp.Account.Web.OpenIddict` |

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TreadSnow is a multi-tenant SaaS application built on **ABP Framework 9.1.1** with **DDD (Domain-Driven Design)** layered architecture. Backend is **.NET 9.0 / C#**, frontend is **Angular 19 + ng-zorro-antd**. Authentication uses **OpenIddict**. Database is **SQL Server** with **EF Core 9.0**.

## Build & Run Commands

### Backend

```bash
# Build entire solution
dotnet build TreadSnow.sln

# Run API host (default: https://localhost:44312)
dotnet run --project src/TreadSnow.HttpApi.Host

# Run database migrations + seed data
dotnet run --project src/TreadSnow.DbMigrator

# Run all tests
dotnet test

# Run specific test project
dotnet test test/TreadSnow.Application.Tests
dotnet test test/TreadSnow.Domain.Tests
dotnet test test/TreadSnow.EntityFrameworkCore.Tests

# Add EF migration
dotnet ef migrations add <MigrationName> -p src/TreadSnow.EntityFrameworkCore -s src/TreadSnow.HttpApi.Host
```

### Frontend

```bash
cd angular
npm install
npm run start        # Dev server at http://localhost:4200
npm run build:prod   # Production build
npm run lint         # ESLint
npm test             # Karma/Jasmine tests
```

### ABP CLI

```bash
abp install-libs    # Install client-side libs (run after clone)
```

## Architecture (DDD Layers)

```
TreadSnow.Domain.Shared        → 枚举、常量、本地化资源（被所有层引用）
TreadSnow.Domain                → 实体、聚合根、领域服务、仓储接口
TreadSnow.Application.Contracts → DTO、应用服务接口、权限定义
TreadSnow.Application           → 应用服务实现、AutoMapper配置
TreadSnow.EntityFrameworkCore   → DbContext、迁移、仓储实现
TreadSnow.HttpApi               → REST控制器
TreadSnow.HttpApi.Host          → ASP.NET Core宿主、启动配置
TreadSnow.DbMigrator            → 数据库迁移控制台工具
```

**依赖方向**: Domain.Shared ← Domain ← Application.Contracts ← Application ← HttpApi ← HttpApi.Host。EF Core层横向引用Domain层。

## Domain Entities

| 实体 | 基类 | 说明 | 多租户 |
|------|------|------|--------|
| `Book` | `AuditedAggregateRoot<Guid>` | 图书（Name, Type, PublishDate, Price, AuthorId） | 否 |
| `Author` | `FullAuditedAggregateRoot<Guid>` | 作者（Name, BirthDate, ShortBio） | 否 |
| `Account` | `FullAuditedAggregateRoot<Guid>` | 会员（No自增, Name, Phone, Email, OpenId） | 是 |
| `Pet` | `FullAuditedAggregateRoot<Guid>` | 宠物（No自增, Name, AccountId外键） | 是 |
| `UploadFile` | `FullAuditedAggregateRoot<Guid>` | 附件（EntityName, RecordId, Name, Type, Path） | 是 |

数据库表前缀为 `App`（见 `TreadSnowConsts.DbTablePrefix`）。

## Adding a New Entity Checklist

1. **Domain.Shared**: 添加枚举/常量（如需要）
2. **Domain**: 创建实体类（继承 `FullAuditedAggregateRoot<Guid>`），多租户实体添加 `Guid? TenantId`
3. **EntityFrameworkCore**: `TreadSnowDbContext` 中添加 `DbSet<T>`，在 `OnModelCreating` 配置表映射
4. **Application.Contracts**: 创建 DTO（XxxDto, CreateXxxDto, UpdateXxxDto）、服务接口 `IXxxAppService`
5. **Application**: 实现 `XxxAppService`，在 `TreadSnowApplicationAutoMapperProfile` 添加映射
6. **Application.Contracts/Permissions**: 在 `TreadSnowPermissions` 添加权限常量，在 `TreadSnowPermissionDefinitionProvider` 注册
7. 添加 EF Core 迁移，运行 DbMigrator

## Key Conventions

- **多租户**: `MultiTenancyConsts.IsEnabled = true`，自定义业务实体需加 `TenantId` 并配置 `HasOne<Tenant>` 外键
- **权限**: 遵循 `TreadSnow.{Entity}.{Create|Edit|Delete}` 格式
- **仓储**: 简单CRUD使用 `IRepository<T, Guid>` 泛型仓储，复杂查询自定义仓储接口（如 `IAuthorRepository`）
- **领域服务**: 包含业务规则的操作放在 `XxxManager`（如 `AuthorManager`）
- **自增编号**: Account/Pet 的 No 字段使用 `UseIdentityColumn(1000, 1)` 从1000起步
- **本地化**: 资源文件在 `Domain.Shared/Localization/TreadSnow/`，JSON格式
- **测试框架**: xUnit + NSubstitute + Shouldly

## Connection Strings

开发环境在 `src/TreadSnow.HttpApi.Host/appsettings.json` 和 `src/TreadSnow.DbMigrator/appsettings.json` 中配置，连接字符串键名为 `Default`。

## Frontend (Angular)

- ABP Angular模块体系，使用 `@abp/ng.*` 系列包
- API代理类在 `angular/src/app/proxy/` 下，使用 ABP CLI 生成：`abp generate-proxy -t ng`
- OAuth配置在 `angular/src/environments/environment.ts`，客户端ID为 `TreadSnow_App`
- UI组件库：ng-zorro-antd

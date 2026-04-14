# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ABP Framework 9.1.1 open-source modules repository — a fork from `abpframework/abp` (remote: `gitee.com/kleychen/abp9.1.1.git`). Contains 18 independent but interdependent modules built on ASP.NET Core / .NET 9.0 following Domain-Driven Design (DDD) layered architecture.

## Build & Test Commands

Each module has its own `.sln` file. Build and test per-module:

```bash
# Build a single module
dotnet build modules/identity/Volo.Abp.Identity.sln

# Run all tests in a module
dotnet test modules/identity/Volo.Abp.Identity.sln

# Run a specific test project
dotnet test modules/identity/test/Volo.Abp.Identity.Application.Tests

# Run a single test
dotnet test modules/identity/test/Volo.Abp.Identity.Application.Tests --filter "FullyQualifiedName~TestMethodName"

# Restore packages (centralized versioning via Directory.Packages.props at repo root)
dotnet restore modules/identity/Volo.Abp.Identity.sln
```

There are no top-level build scripts. The `dotnet` CLI is the only build tool.

## Module Architecture

Every major module follows the same DDD layer structure under `src/`:

| Layer | Project Suffix | Target | Purpose |
|-------|---------------|--------|---------|
| Domain.Shared | `.Domain.Shared` | netstandard2.0+ | Constants, enums, shared types |
| Domain | `.Domain` | net9.0 | Entities, repositories, domain services |
| Application.Contracts | `.Application.Contracts` | netstandard2.0+ | DTOs, app service interfaces |
| Application | `.Application` | net9.0 | App services, AutoMapper profiles |
| HttpApi | `.HttpApi` | net9.0 | REST controllers |
| HttpApi.Client | `.HttpApi.Client` | netstandard2.0+ | Client-side proxies |
| EntityFrameworkCore | `.EntityFrameworkCore` | net9.0 | EF Core DbContext, mappings |
| MongoDB | `.MongoDB` | net9.0 | MongoDB data access |
| Web | `.Web` | net9.0 | MVC Razor Pages UI |
| Blazor / Blazor.Server / Blazor.WebAssembly | `.Blazor*` | net9.0 | Blazor UI |
| Installer | `.Installer` | net9.0 | Module installation |

Tests live under `test/` with: `TestBase`, `Domain.Tests`, `Application.Tests`, `EntityFrameworkCore.Tests`, `MongoDB.Tests`.

## Key Shared Configuration

All `.csproj` files import from the **repo root** (`../../..` or `../../../..`):

- `common.props` — Version (`9.1.1`), NuGet metadata, SourceLink, packaging
- `configureawait.props` — Fody ConfigureAwait for Release builds
- `Directory.Build.props` — Auto-detects test projects, adds coverlet.collector
- `Directory.Packages.props` — Centralized NuGet package versions (all `<PackageVersion>` entries)

When adding or updating a NuGet dependency, edit `Directory.Packages.props` at the repo root — individual `.csproj` files only declare `<PackageReference>` without versions.

## Cross-Module Dependencies

Modules reference each other at the project level:
- **account** depends on **identity** (Application.Contracts, Domain.Shared)
- **openiddict** depends on **identity** (Domain)
- **permission-management** has cross-module integration projects: `Volo.Abp.PermissionManagement.Domain.Identity`, `...Domain.IdentityServer`, `...Domain.OpenIddict`

The ABP **framework** is referenced from `../framework/` (sibling directory to `modules/`).

## Adding Fields to IdentityUser

This is a common task in this fork. The `/ABPuserModified` skill documents the full 11-file checklist. In summary, adding a field (e.g. `OpenId`) requires changes across two modules:

**Identity module (6 files):**
1. `IdentityUserConsts.cs` — add max length constant
2. `IdentityUser.cs` — add entity property
3. `IdentityUserDto.cs` — add to output DTO
4. `IdentityUserCreateOrUpdateDtoBase.cs` — add to input DTO with validation
5. `IdentityDbContextModelBuilderExtensions.cs` — add EF Core column mapping
6. `IdentityUserAppService.cs` — add assignment in `UpdateUserByInput`

**Account module (5 files):**
7. `ProfileDto.cs` — profile output DTO
8. `UpdateProfileDto.cs` — profile input DTO with validation
9. `ProfileAppService.cs` — assignment in `UpdateAsync`
10. `AccountProfilePersonalInfoManagementGroupViewComponent.cs` — Web MVC model
11. `AccountManage.razor.cs` — Blazor model

AutoMapper profiles use convention-based mapping — same-name properties are auto-mapped.

## Current Branch Context

Branch `feature-UserCurd` has modifications to:
- Register and forgot-password endpoints commented out in `AccountController.cs` (Chinese comments: 注释注册/忘记密码对外接口)
- An `OpenId` field added to `IdentityUser` following the 11-file pattern above

## Conventions

- **Namespace = folder path**: `Volo.Abp.Identity` namespace maps to `Volo/Abp/Identity/` directory
- **ABP module classes**: Each layer has an `Abp[Module][Layer]Module.cs` that configures DI and dependencies
- **Localization**: JSON resource files embedded via `<EmbeddedResource>` in `.csproj`
- **Permissions**: Defined in `[Module]Permissions.cs` as string constants, registered in `[Module]PermissionDefinitionProvider.cs`
- **Entity configuration**: EF Core mappings live in `[Module]DbContextModelBuilderExtensions.cs`, table prefix controlled by `Abp[Module]DbProperties`
- **Repository interfaces** in Domain, implementations in EntityFrameworkCore/MongoDB
- **DTOs** with `[DynamicStringLength]` attributes reference constants from `Domain.Shared` for validation

---
name: newToBEntity
description: 创建无外键的ToB业务实体全栈CRUD（基于Account模板），覆盖后端Domain/EF/AppService/DTO/权限/本地化 + 前端Angular proxy/组件/路由/菜单，含数据权限、导出、列宽拖动
---

# newToBEntity - 创建无外键ToB业务实体（全栈）

## 使用方式

用户提供表结构（markdown或文字描述），包含：
1. **实体英文名**（PascalCase），如 `Customer`
2. **实体中文名**，如 `客户`
3. **业务字段列表**，如 `name(名称,string,必填,64), phone(手机号码,string,必填,64), email(邮箱,string,必填,64), description(描述,string?,可选,1000)`
4. **菜单图标**（FontAwesome class），如 `fas fa-user`

## 占位符约定

| 占位符 | 含义 | 示例 |
|--------|------|------|
| `{Entity}` | PascalCase实体名 | `Account` |
| `{entity}` | camelCase实体名 | `account` |
| `{entity-kebab}` | kebab-case | `account` |
| `{Entities}` | PascalCase复数 | `Accounts` |
| `{entities}` | camelCase复数 | `accounts` |
| `{entityCn}` | 中文实体名 | `会员` |
| `{icon}` | 菜单图标class | `fas fa-users` |

---

## 全部文件清单（按层级）

### 后端 C#

```
src/TreadSnow.Domain/{Entities}/{Entity}.cs                              → 领域实体
src/TreadSnow.EntityFrameworkCore/EntityFrameworkCore/TreadSnowDbContext.cs → 追加 DbSet + OnModelCreating
src/TreadSnow.Application.Contracts/{Entities}/{Entity}Dto.cs            → 展示DTO
src/TreadSnow.Application.Contracts/{Entities}/Create{Entity}Dto.cs      → 创建DTO
src/TreadSnow.Application.Contracts/{Entities}/Update{Entity}Dto.cs      → 更新DTO
src/TreadSnow.Application.Contracts/{Entities}/Get{Entity}ListDto.cs     → 查询条件DTO
src/TreadSnow.Application.Contracts/{Entities}/I{Entity}AppService.cs    → 应用服务接口
src/TreadSnow.Application/{Entities}/{Entity}AppService.cs               → 应用服务实现
src/TreadSnow.Application/TreadSnowApplicationAutoMapperProfile.cs       → 追加映射
src/TreadSnow.Application.Contracts/Permissions/TreadSnowPermissions.cs  → 追加权限常量
src/TreadSnow.Application.Contracts/Permissions/TreadSnowPermissionDefinitionProvider.cs → 追加权限注册
src/TreadSnow.Domain.Shared/Localization/TreadSnow/en.json              → 追加英文本地化
src/TreadSnow.Domain.Shared/Localization/TreadSnow/zh-Hans.json         → 追加中文本地化
```

### 前端 Angular

```
angular/src/app/proxy/{entities}/models.ts
angular/src/app/proxy/{entities}/{entity}.service.ts
angular/src/app/proxy/{entities}/index.ts
angular/src/app/{entity-kebab}/{entity-kebab}.component.ts
angular/src/app/{entity-kebab}/{entity-kebab}.component.html
angular/src/app/{entity-kebab}/{entity-kebab}.component.scss             (空文件)
angular/src/app/{entity-kebab}/{entity-kebab}.module.ts
angular/src/app/{entity-kebab}/{entity-kebab}-routing.module.ts
angular/src/app/route.provider.ts                                        → 追加菜单路由
angular/src/app/app-routing.module.ts                                    → 追加懒加载路由
angular/src/app/role/data-permission-modal/data-permission-modal.component.ts → 追加数据权限实体注册
```

### EF Core 数据迁移

```bash
dotnet ef migrations add Add{Entity} -p src/TreadSnow.EntityFrameworkCore -s src/TreadSnow.HttpApi.Host
```

---

## 后端文件模板

### 1. Domain/{Entities}/{Entity}.cs

```csharp
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {entityCn}
    /// </summary>
    public class {Entity} : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 编号（自增列）
        /// </summary>
        public int? No { get; set; }

        // === 业务字段 ===
        // 必填字段用 string，选填用 string?
        // 每个字段加 summary 中文注释

        /// <summary>
        /// 负责人Id
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }
    }
}
```

### 2. DbContext — 追加内容

**追加 DbSet（与其他 DbSet 同级）：**

```csharp
public DbSet<{Entity}> {Entities} { get; set; }
```

**追加 OnModelCreating（在 base.OnModelCreating 之后）：**

```csharp
builder.Entity<{Entity}>(b =>
{
    b.ToTable(TreadSnowConsts.DbTablePrefix + "{Entities}", TreadSnowConsts.DbSchema);
    b.ConfigureByConvention();
    b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId);
    b.Property(x => x.No).UseIdentityColumn(1000, 1);
    // === 业务字段约束 ===
    // 必填: b.Property(x => x.{Field}).IsRequired().HasMaxLength({len});
    // 选填: b.Property(x => x.{Field}).HasMaxLength({len});
});
```

**别忘记添加对应的 using 引用。**

### 3. Application.Contracts/{Entities}/{Entity}Dto.cs

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {entityCn}DTO
    /// </summary>
    public class {Entity}Dto : EntityDto<Guid>
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int No { get; set; }

        // === 业务字段（与实体同名同类型） ===

        /// <summary>
        /// 负责人Id
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }

        /// <summary>
        /// 负责人名称（关联查询）
        /// </summary>
        public string? OwnerName { get; set; }

        /// <summary>
        /// 负责团队名称（关联查询）
        /// </summary>
        public string? OwnerTeamName { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 创建人名称（关联查询）
        /// </summary>
        public string? CreatorName { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        public Guid? LastModifierId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 最后修改人名称（关联查询）
        /// </summary>
        public string? LastModifierName { get; set; }

        /// <summary>
        /// 当前用户是否可编辑该记录（数据权限判断）
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// 当前用户是否可删除该记录（数据权限判断）
        /// </summary>
        public bool CanDelete { get; set; }
    }
}
```

### 4. Application.Contracts/{Entities}/Create{Entity}Dto.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// 创建{entityCn}DTO
    /// </summary>
    public class Create{Entity}Dto
    {
        // === 业务字段 ===
        // 必填: [Required] public string {Field} { get; set; } = string.Empty;
        // 选填: public string? {Field} { get; set; }

        /// <summary>
        /// 负责人Id（不传则默认当前用户）
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }
    }
}
```

### 5. Application.Contracts/{Entities}/Update{Entity}Dto.cs

与 `Create{Entity}Dto` 结构相同。

### 6. Application.Contracts/{Entities}/Get{Entity}ListDto.cs

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {entityCn}查询条件DTO
    /// </summary>
    public class Get{Entity}ListDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 名称模糊搜索（可选）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 按负责人筛选（可选）
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 创建时间起始（可选）
        /// </summary>
        public DateTime? StartCreationTime { get; set; }

        /// <summary>
        /// 创建时间截止（可选）
        /// </summary>
        public DateTime? EndCreationTime { get; set; }

        /// <summary>
        /// 修改时间起始（可选）
        /// </summary>
        public DateTime? StartLastModificationTime { get; set; }

        /// <summary>
        /// 修改时间截止（可选）
        /// </summary>
        public DateTime? EndLastModificationTime { get; set; }
    }
}
```

### 7. Application.Contracts/{Entities}/I{Entity}AppService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreadSnow.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {entityCn}应用服务接口
    /// </summary>
    public interface I{Entity}AppService : IApplicationService
    {
        /// <summary>
        /// 获取单条{entityCn}
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        /// <returns>{entityCn}DTO</returns>
        Task<{Entity}Dto> GetAsync(Guid id);

        /// <summary>
        /// 获取{entityCn}分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<{Entity}Dto>> GetListAsync(Get{Entity}ListDto input);

        /// <summary>
        /// 获取所有{entityCn}列表（不分页，用于导出）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        Task<List<{Entity}Dto>> GetExportListAsync(string? name);

        /// <summary>
        /// 创建{entityCn}
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的{entityCn}DTO</returns>
        Task<{Entity}Dto> CreateAsync(Create{Entity}Dto input);

        /// <summary>
        /// 更新{entityCn}
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的{entityCn}DTO</returns>
        Task<{Entity}Dto> UpdateAsync(Guid id, Update{Entity}Dto input);

        /// <summary>
        /// 删除{entityCn}
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 获取用户下拉列表（用于选择负责人）
        /// </summary>
        /// <returns>用户Id和名称列表</returns>
        Task<ListResultDto<UserLookupDto>> GetOwnerLookupAsync();

        /// <summary>
        /// 获取团队下拉列表（用于选择负责团队）
        /// </summary>
        /// <returns>团队Id和名称列表</returns>
        Task<ListResultDto<TeamLookupDto>> GetTeamLookupAsync();
    }
}
```

### 8. Application/{Entities}/{Entity}AppService.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.DataPermissions;
using TreadSnow.Lookups;
using TreadSnow.Permissions;
using TreadSnow.Teams;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {entityCn}应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.{Entities}.Default)]
    public class {Entity}AppService : ApplicationService, I{Entity}AppService
    {
        /// <summary>
        /// {entityCn}仓储
        /// </summary>
        private readonly IRepository<{Entity}, Guid> _repository;

        /// <summary>
        /// 数据权限过滤服务
        /// </summary>
        private readonly DataPermissionService _dataPermissionService;

        /// <summary>
        /// 用户仓储（用于查询负责人名称）
        /// </summary>
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        /// <summary>
        /// 团队仓储（用于查询负责团队名称）
        /// </summary>
        private readonly IRepository<Team, Guid> _teamRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">{entityCn}仓储</param>
        /// <param name="dataPermissionService">数据权限过滤服务</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="teamRepository">团队仓储</param>
        public {Entity}AppService(IRepository<{Entity}, Guid> repository, DataPermissionService dataPermissionService, IRepository<IdentityUser, Guid> userRepository, IRepository<Team, Guid> teamRepository)
        {
            _repository = repository;
            _dataPermissionService = dataPermissionService;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// 获取单条{entityCn}
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        /// <returns>{entityCn}DTO</returns>
        public async Task<{Entity}Dto> GetAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<{Entity}, {Entity}Dto>(entity);
            var dtoList = new List<{Entity}Dto> { dto };
            await FillLookupNamesAsync(dtoList);
            await FillPermissionsAsync(dtoList);
            return dto;
        }

        /// <summary>
        /// 获取{entityCn}分页列表（支持name模糊筛选 + 负责人筛选 + 数据权限过滤）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<{Entity}Dto>> GetListAsync(Get{Entity}ListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                query = query.Where(x => x.Name.Contains(input.Name));
            }

            if (input.OwnerId.HasValue)
            {
                query = query.Where(x => x.OwnerId == input.OwnerId.Value);
            }

            if (input.StartCreationTime.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.StartCreationTime.Value);
            }

            if (input.EndCreationTime.HasValue)
            {
                query = query.Where(x => x.CreationTime < input.EndCreationTime.Value.AddDays(1));
            }

            if (input.StartLastModificationTime.HasValue)
            {
                query = query.Where(x => x.LastModificationTime >= input.StartLastModificationTime.Value);
            }

            if (input.EndLastModificationTime.HasValue)
            {
                query = query.Where(x => x.LastModificationTime < input.EndLastModificationTime.Value.AddDays(1));
            }

            query = await _dataPermissionService.ApplyReadFilterAsync(query, "{entity}", x => x.OwnerId, x => x.OwnerTeamId);

            var totalCount = query.Count();
            var items = query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            var dtos = ObjectMapper.Map<List<{Entity}>, List<{Entity}Dto>>(items);
            await FillLookupNamesAsync(dtos);
            await FillPermissionsAsync(dtos);

            return new PagedResultDto<{Entity}Dto>(totalCount, dtos);
        }

        /// <summary>
        /// 获取所有{entityCn}列表（不分页，用于导出 + 数据权限过滤）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        public async Task<List<{Entity}Dto>> GetExportListAsync(string? name)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            query = await _dataPermissionService.ApplyReadFilterAsync(query, "{entity}", x => x.OwnerId, x => x.OwnerTeamId);

            var items = query.OrderByDescending(x => x.CreationTime).ToList();
            var dtos = ObjectMapper.Map<List<{Entity}>, List<{Entity}Dto>>(items);
            await FillLookupNamesAsync(dtos);
            await FillPermissionsAsync(dtos);
            return dtos;
        }

        /// <summary>
        /// 创建{entityCn}（OwnerId不传则默认当前用户）
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的{entityCn}DTO</returns>
        [Authorize(TreadSnowPermissions.{Entities}.Create)]
        public async Task<{Entity}Dto> CreateAsync(Create{Entity}Dto input)
        {
            var entity = ObjectMapper.Map<Create{Entity}Dto, {Entity}>(input);
            entity.TenantId = CurrentTenant.Id;
            entity.OwnerId = input.OwnerId ?? CurrentUser.Id;
            await _repository.InsertAsync(entity);
            return ObjectMapper.Map<{Entity}, {Entity}Dto>(entity);
        }

        /// <summary>
        /// 更新{entityCn}（含写权限校验）
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的{entityCn}DTO</returns>
        [Authorize(TreadSnowPermissions.{Entities}.Edit)]
        public async Task<{Entity}Dto> UpdateAsync(Guid id, Update{Entity}Dto input)
        {
            var entity = await _repository.GetAsync(id);

            var hasPermission = await _dataPermissionService.CheckWritePermissionAsync("{entity}", entity.OwnerId, entity.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的编辑权限");

            ObjectMapper.Map(input, entity);
            await _repository.UpdateAsync(entity);
            return ObjectMapper.Map<{Entity}, {Entity}Dto>(entity);
        }

        /// <summary>
        /// 删除{entityCn}（含删除权限校验）
        /// </summary>
        /// <param name="id">{entityCn}Id</param>
        [Authorize(TreadSnowPermissions.{Entities}.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);

            var hasPermission = await _dataPermissionService.CheckDeletePermissionAsync("{entity}", entity.OwnerId, entity.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的删除权限");

            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// 获取用户下拉列表（用于选择负责人）
        /// </summary>
        /// <returns>用户Id和名称列表</returns>
        public async Task<ListResultDto<UserLookupDto>> GetOwnerLookupAsync()
        {
            var queryable = await _userRepository.GetQueryableAsync();
            var users = await AsyncExecuter.ToListAsync(queryable);
            var items = users.Select(u => new UserLookupDto { Id = u.Id, Name = u.UserName }).ToList();
            return new ListResultDto<UserLookupDto>(items);
        }

        /// <summary>
        /// 获取团队下拉列表（用于选择负责团队）
        /// </summary>
        /// <returns>团队Id和名称列表</returns>
        public async Task<ListResultDto<TeamLookupDto>> GetTeamLookupAsync()
        {
            var teams = await _teamRepository.GetListAsync();
            var items = teams.Select(t => new TeamLookupDto { Id = t.Id, Name = t.Name }).ToList();
            return new ListResultDto<TeamLookupDto>(items);
        }

        /// <summary>
        /// 批量填充每条记录的编辑/删除权限标识
        /// </summary>
        /// <param name="dtos">DTO列表</param>
        private async Task FillPermissionsAsync(List<{Entity}Dto> dtos)
        {
            var records = dtos.Select(d => (d.OwnerId, d.OwnerTeamId)).ToList();
            var permissions = await _dataPermissionService.BatchCheckPermissionsAsync("{entity}", records);
            for (var i = 0; i < dtos.Count; i++)
            {
                dtos[i].CanEdit = permissions[i].CanEdit;
                dtos[i].CanDelete = permissions[i].CanDelete;
            }
        }

        /// <summary>
        /// 批量填充负责人、负责团队、创建人、修改人名称
        /// </summary>
        /// <param name="dtos">DTO列表</param>
        private async Task FillLookupNamesAsync(List<{Entity}Dto> dtos)
        {
            var ownerIds = dtos.Where(d => d.OwnerId.HasValue).Select(d => d.OwnerId!.Value).Distinct().ToList();
            var creatorIds = dtos.Where(d => d.CreatorId.HasValue).Select(d => d.CreatorId!.Value).Distinct().ToList();
            var modifierIds = dtos.Where(d => d.LastModifierId.HasValue).Select(d => d.LastModifierId!.Value).Distinct().ToList();
            var allUserIds = ownerIds.Union(creatorIds).Union(modifierIds).Distinct().ToList();

            var teamIds = dtos.Where(d => d.OwnerTeamId.HasValue).Select(d => d.OwnerTeamId!.Value).Distinct().ToList();

            var userDict = new Dictionary<Guid, string>();
            var teamDict = new Dictionary<Guid, string>();

            if (allUserIds.Any())
            {
                var userQueryable = await _userRepository.GetQueryableAsync();
                var users = await AsyncExecuter.ToListAsync(userQueryable.Where(u => allUserIds.Contains(u.Id)));
                userDict = users.ToDictionary(u => u.Id, u => u.UserName);
            }

            if (teamIds.Any())
            {
                var teamQueryable = await _teamRepository.GetQueryableAsync();
                var teams = await AsyncExecuter.ToListAsync(teamQueryable.Where(t => teamIds.Contains(t.Id)));
                teamDict = teams.ToDictionary(t => t.Id, t => t.Name);
            }

            foreach (var dto in dtos)
            {
                if (dto.OwnerId.HasValue && userDict.TryGetValue(dto.OwnerId.Value, out var ownerName)) dto.OwnerName = ownerName;
                if (dto.OwnerTeamId.HasValue && teamDict.TryGetValue(dto.OwnerTeamId.Value, out var teamName)) dto.OwnerTeamName = teamName;
                if (dto.CreatorId.HasValue && userDict.TryGetValue(dto.CreatorId.Value, out var creatorName)) dto.CreatorName = creatorName;
                if (dto.LastModifierId.HasValue && userDict.TryGetValue(dto.LastModifierId.Value, out var modifierName)) dto.LastModifierName = modifierName;
            }
        }
    }
}
```

### 9. AutoMapper — 追加到 TreadSnowApplicationAutoMapperProfile 构造函数

```csharp
// {entityCn}
CreateMap<{Entity}, {Entity}Dto>();
CreateMap<Create{Entity}Dto, {Entity}>();
CreateMap<Update{Entity}Dto, {Entity}>();
```

### 10. Permissions — TreadSnowPermissions.cs 追加

```csharp
/// <summary>
/// {entityCn}权限
/// </summary>
public static class {Entities}
{
    public const string Default = GroupName + ".{Entities}";
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
}
```

### 11. Permissions — TreadSnowPermissionDefinitionProvider.cs 追加

在 `Define` 方法末尾追加：

```csharp
var {entities}Permission = myGroup.AddPermission(TreadSnowPermissions.{Entities}.Default, L("Permission:{Entities}"));
{entities}Permission.AddChild(TreadSnowPermissions.{Entities}.Create, L("Permission:{Entities}.Create"));
{entities}Permission.AddChild(TreadSnowPermissions.{Entities}.Edit, L("Permission:{Entities}.Edit"));
{entities}Permission.AddChild(TreadSnowPermissions.{Entities}.Delete, L("Permission:{Entities}.Delete"));
```

### 12. Localization — en.json 追加

```json
"//{entity} entity": "",
"Menu:{Entities}": "{Entities}",
"New{Entity}": "New {entity}",
"Permission:{Entities}": "{Entity} Management",
"Permission:{Entities}.Create": "Creating new {entities}",
"Permission:{Entities}.Edit": "Editing the {entities}",
"Permission:{Entities}.Delete": "Deleting the {entities}",
// 每个业务字段的本地化键
"{Field}": "{FieldEnglish}"
```

### 13. Localization — zh-Hans.json 追加

```json
"Menu:{Entities}": "{entityCn}管理",
"New{Entity}": "新增{entityCn}",
// 每个业务字段的中文
"{Field}": "{fieldCn}"
```

**注意**：权限相关的本地化键（`Permission:{Entities}` 等）只需在 `en.json` 中添加，ABP权限管理界面使用英文键，`zh-Hans.json` 不需要重复添加权限相关键。

---

## 前端文件模板

### 14. proxy/{entities}/models.ts

```typescript
import type { EntityDto } from '@abp/ng.core';

/** {entityCn}DTO，用于列表展示和详情查看 */
export interface {Entity}Dto extends EntityDto<string> {
  /** 租户ID */
  tenantId?: string;
  /** 自增编号 */
  no: number;
  // === 业务字段 ===
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
  /** 负责人名称 */
  ownerName?: string;
  /** 负责团队名称 */
  ownerTeamName?: string;
  /** 创建人Id */
  creatorId?: string;
  /** 创建时间 */
  creationTime?: string;
  /** 创建人名称 */
  creatorName?: string;
  /** 最后修改人Id */
  lastModifierId?: string;
  /** 最后修改时间 */
  lastModificationTime?: string;
  /** 最后修改人名称 */
  lastModifierName?: string;
  /** 当前用户是否可编辑该记录 */
  canEdit?: boolean;
  /** 当前用户是否可删除该记录 */
  canDelete?: boolean;
}

/** 创建{entityCn}DTO */
export interface Create{Entity}Dto {
  // === 业务字段 ===
  /** 负责人Id（不传则默认当前用户） */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}

/** 更新{entityCn}DTO */
export interface Update{Entity}Dto {
  // === 与Create{Entity}Dto结构相同 ===
}

/** 用户下拉选项DTO */
export interface UserLookupDto extends EntityDto<string> {
  /** 用户名 */
  name?: string;
}

/** 团队下拉选项DTO */
export interface TeamLookupDto extends EntityDto<string> {
  /** 团队名称 */
  name?: string;
}
```

### 15. proxy/{entities}/{entity}.service.ts

```typescript
import type { {Entity}Dto, Create{Entity}Dto, Update{Entity}Dto, UserLookupDto, TeamLookupDto } from './models';
import { RestService } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

/**
 * {entityCn}管理API服务
 * 提供{entityCn}的增删改查接口调用
 */
@Injectable({
  providedIn: 'root',
})
export class {Entity}Service {
  /** API名称 */
  apiName = 'Default';

  /** 创建{entityCn} */
  create = (input: Create{Entity}Dto) =>
    this.restService.request<any, {Entity}Dto>({ method: 'POST', url: '/api/app/{entity}', body: input }, { apiName: this.apiName });

  /** 删除{entityCn} */
  delete = (id: string) =>
    this.restService.request<any, void>({ method: 'DELETE', url: `/api/app/{entity}/${id}` }, { apiName: this.apiName });

  /** 获取单个{entityCn}详情 */
  get = (id: string) =>
    this.restService.request<any, {Entity}Dto>({ method: 'GET', url: `/api/app/{entity}/${id}` }, { apiName: this.apiName });

  /** 获取{entityCn}分页列表 */
  getList = (input: PagedAndSortedResultRequestDto & { name?: string; ownerId?: string; startCreationTime?: string; endCreationTime?: string; startLastModificationTime?: string; endLastModificationTime?: string }) =>
    this.restService.request<any, PagedResultDto<{Entity}Dto>>({ method: 'GET', url: '/api/app/{entity}', params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, name: input.name, ownerId: input.ownerId, startCreationTime: input.startCreationTime, endCreationTime: input.endCreationTime, startLastModificationTime: input.startLastModificationTime, endLastModificationTime: input.endLastModificationTime } }, { apiName: this.apiName });

  /** 获取所有{entityCn}列表（不分页，用于导出） */
  getAllList = (name?: string) =>
    this.restService.request<any, {Entity}Dto[]>({ method: 'GET', url: '/api/app/{entity}/export-list', params: { name } }, { apiName: this.apiName });

  /** 更新{entityCn} */
  update = (id: string, input: Update{Entity}Dto) =>
    this.restService.request<any, {Entity}Dto>({ method: 'PUT', url: `/api/app/{entity}/${id}`, body: input }, { apiName: this.apiName });

  /** 获取用户下拉列表（用于选择负责人） */
  getOwnerLookup = () =>
    this.restService.request<any, ListResultDto<UserLookupDto>>({ method: 'GET', url: '/api/app/{entity}/owner-lookup' }, { apiName: this.apiName });

  /** 获取团队下拉列表（用于选择负责团队） */
  getTeamLookup = () =>
    this.restService.request<any, ListResultDto<TeamLookupDto>>({ method: 'GET', url: '/api/app/{entity}/team-lookup' }, { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
```

### 16. proxy/{entities}/index.ts

```typescript
export * from './{entity}.service';
export * from './models';
```

### 17. {entity-kebab}/{entity-kebab}-routing.module.ts

```typescript
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { {Entity}Component } from './{entity-kebab}.component';

/** {entityCn}模块路由配置 */
const routes: Routes = [
  { path: '', component: {Entity}Component, canActivate: [authGuard, permissionGuard] },
];

/**
 * {entityCn}路由模块
 * 配置{entityCn}管理页面的路由和守卫
 */
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class {Entity}RoutingModule {}
```

### 18. {entity-kebab}/{entity-kebab}.module.ts

```typescript
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { {Entity}RoutingModule } from './{entity-kebab}-routing.module';
import { {Entity}Component } from './{entity-kebab}.component';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';

/**
 * {entityCn}模块
 * 负责{entityCn}功能相关的组件声明和依赖导入
 */
@NgModule({
  declarations: [{Entity}Component],
  imports: [{Entity}RoutingModule, SharedModule, NzTableModule, NzButtonModule, NzIconModule, NzDropDownModule, NzModalModule, NzFormModule, NzInputModule, NzSelectModule, NzCardModule, NzMenuModule, NzGridModule, NzMessageModule, NzToolTipModule, NzDatePickerModule, FormsModule]
})
export class {Entity}Module { }
```

按需追加的模块（仅当模板中实际使用时才添加）：
- `NzTagModule` — `<nz-tag>` 标签展示
- `NzSwitchModule` — `<nz-switch>` 开关
- `NzCheckboxModule` — `<nz-checkbox>` 复选框

### 19. {entity-kebab}/{entity-kebab}.component.ts

完全参照 `account.component.ts` 的模式。关键结构：

- `implements OnInit, AfterViewInit`
- `setupColumnResize()` 列宽拖动（与Account完全一致）
- `sortBy{Field}` 排序函数（每个业务字段 + ownerName, ownerTeamName, creatorName, creationTime, lastModifierName, lastModificationTime）
- `nameFilter` + `nameFilterVisible` 名称搜索
- `ownerFilter` + `ownerFilters` 负责人漏斗筛选（后端）
- `owners` / `teams` 下拉数据
- `create{Entity}()`, `edit{Entity}(id)`, `delete(id)`, `save()`, `buildForm()`
- `exportCurrent()`, `exportAll()`, `downloadXlsx()` 导出
- `onPageIndexChange()`, `onPageSizeChange()` 分页
- 构造函数注入：`ListService, {Entity}Service, FormBuilder, ConfirmationService, ConfigStateService, NzMessageService, ElementRef, Renderer2`

### 20. {entity-kebab}/{entity-kebab}.component.html

完全参照 `account.component.html` 的模式。关键结构：

- `nz-card` + `#cardTitle`（新增按钮 + 导出按钮组）
- `nz-table`（服务端分页、横向滚动、loading）
- `thead`：No列 + Name列（带 `nz-filter-trigger` 搜索）+ 业务列 + Owner列（带 `nzShowFilter`）+ OwnerTeam列 + 审计列（CreatorName/LastModifierName带漏斗筛选，CreationTime/LastModificationTime带日期范围筛选）+ Actions列（`nzRight`）
- `tbody`：每个 `<td>` 用 `<span class="cell-ellipsis" nz-tooltip>` 包裹
- `nz-dropdown-menu #nameFilterMenu`：名称搜索弹出框
- `nz-modal`：双列表单（`nz-row` + `nz-col [nzSpan]="12"`），含业务字段 + 负责人/团队选择器
- 弹窗标题根据 `selected{Entity}.id` 区分新建/编辑
- `[nzOkText]` 根据 `canEdit` 控制是否显示保存按钮
- 删除按钮同时检查 `*abpPermission` 和 `row.canDelete`

### 21. route.provider.ts — 追加菜单

在 `routes.add([...])` 数组中追加：

```typescript
{
  path: '/{entities}',
  name: '::Menu:{Entities}',
  iconClass: '{icon}',
  layout: eLayoutType.application,
  requiredPolicy: 'TreadSnow.{Entities}',
},
```

### 22. app-routing.module.ts — 追加懒加载

在 `routes` 数组中追加：

```typescript
{ path: '{entities}', loadChildren: () => import('./{entity-kebab}/{entity-kebab}.module').then(m => m.{Entity}Module) },
```

### 23. data-permission-modal.component.ts — 追加数据权限实体注册

**路径：** `angular/src/app/role/data-permission-modal/data-permission-modal.component.ts`

在 `ENTITY_LIST` 数组中追加：

```typescript
{ entityName: '{entity}', labelKey: '::Menu:{Entities}' },
```

此步骤确保在角色管理-数据权限页面中能看到该实体的读/写/删除权限配置行。

### 24. EF Core 数据迁移

所有代码编写完成后执行：

```bash
dotnet ef migrations add Add{Entity} -p src/TreadSnow.EntityFrameworkCore -s src/TreadSnow.HttpApi.Host
```

---

## 数据权限机制说明

| 方法 | 用途 | 调用位置 |
|------|------|----------|
| `ApplyReadFilterAsync(query, "{entity}", ownerSelector, teamSelector)` | 列表查询时按角色过滤可见数据 | GetListAsync, GetExportListAsync |
| `CheckWritePermissionAsync("{entity}", ownerId, teamId)` | 编辑前检查当前用户是否有写权限 | UpdateAsync |
| `CheckDeletePermissionAsync("{entity}", ownerId, teamId)` | 删除前检查当前用户是否有删除权限 | DeleteAsync |
| `BatchCheckPermissionsAsync("{entity}", records)` | 批量检查每条记录的 canEdit/canDelete | FillPermissionsAsync |

第一个参数 `"{entity}"` 是数据权限配置中的资源标识（小写实体名）。

## 生成注意事项

1. **中文注释**：所有 C# 类/方法/属性用 `/// <summary>` 注释，TypeScript 用 `/** */` 注释
2. **参数不换行**：构造函数和方法参数全部写在同一行
3. **权限格式**：`TreadSnow.{Entities}.Create|Edit|Delete`
4. **本地化键**：`::New{Entity}`, `::Edit`, `::Save`, `::Delete`, `::Actions`, `::No`, `::Name`, `::{Field}`, `::OwnerId`, `::OwnerTeamId`, `::CreatorName`, `::CreationTime`, `::LastModifierName`, `::LastModificationTime`
5. **弹窗只读**：当 `selected{Entity}.canEdit === false` 时隐藏保存按钮
6. **删除双重检查**：前端 `*abpPermission` + `row.canDelete`，后端 `[Authorize]` + `CheckDeletePermissionAsync`
7. **负责人默认值**：创建时 `OwnerId` 默认当前用户（后端和前端都处理）
8. **导出Excel**：使用 `xlsx` 库，字段名映射为中文表头
9. **数据迁移**：代码全部完成后执行 `dotnet ef migrations add`

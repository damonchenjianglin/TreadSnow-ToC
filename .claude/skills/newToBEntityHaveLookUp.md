---
name: newToBEntityHaveLookUp
description: 创建有外键关联的ToB业务实体全栈CRUD（基于Pet模板），在newToBEntity基础上增加外键Lookup查询、关联名称填充、前端下拉选择和筛选
---

# newToBEntityHaveLookUp - 创建有外键ToB业务实体（全栈）

## 与 newToBEntity 的关系

本模板**继承 newToBEntity 的全部内容**，在其基础上增加外键关联的处理。生成时先按 newToBEntity 模板生成全部文件，再按本文档的差异部分修改/追加。

## 额外输入

除 newToBEntity 所需信息外，用户还需提供：
- **外键字段列表**，如 `accountId → Account(会员)` 表示 `accountId` 字段关联 `Account` 实体

## 额外占位符

| 占位符 | 含义 | 示例 |
|--------|------|------|
| `{FK}` | 外键实体PascalCase | `Account` |
| `{fk}` | 外键实体camelCase | `account` |
| `{FKs}` | 外键实体复数PascalCase | `Accounts` |
| `{fkId}` | 外键字段名 | `accountId` / `AccountId` |
| `{fkName}` | 外键关联名称字段 | `accountName` / `AccountName` |
| `{fkCn}` | 外键实体中文名 | `会员` |

---

## 差异文件清单（相对于 newToBEntity）

需要额外创建的文件：
```
src/TreadSnow.Application.Contracts/{Entities}/{FK}LookupDto.cs    → 外键Lookup DTO
```

需要修改的文件（在 newToBEntity 生成结果基础上改动）：
```
后端：Entity.cs, DbContext, {Entity}Dto.cs, Create/Update Dto, I{Entity}AppService.cs, {Entity}AppService.cs, AutoMapperProfile
前端：proxy models.ts, proxy service.ts, component.ts, component.html
```

---

## 后端差异模板

### 1. Domain/{Entities}/{Entity}.cs — 增加外键属性

```csharp
/// <summary>
/// {fkCn}Id（外键）
/// </summary>
public Guid {fkId} { get; set; }
```

### 2. DbContext OnModelCreating — 增加外键约束

```csharp
b.HasOne<{FK}>().WithMany().HasForeignKey(x => x.{fkId}).IsRequired();
```

### 3. Application.Contracts/{Entities}/{FK}LookupDto.cs — 新建文件

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.{Entities}
{
    /// <summary>
    /// {fkCn}下拉选项DTO（用于{entityCn}表单选择{fkCn}）
    /// </summary>
    public class {FK}LookupDto : EntityDto<Guid>
    {
        /// <summary>
        /// {fkCn}名称
        /// </summary>
        public string Name { get; set; }
    }
}
```

### 4. {Entity}Dto.cs — 增加外键字段

```csharp
/// <summary>
/// {fkCn}Id（外键）
/// </summary>
public Guid {fkId} { get; set; }

/// <summary>
/// {fkCn}名称（关联查询）
/// </summary>
public string? {fkName} { get; set; }
```

### 5. Create{Entity}Dto / Update{Entity}Dto — 增加外键字段

```csharp
[Required]
public Guid {fkId} { get; set; }
```

### 6. I{Entity}AppService.cs — 增加 Lookup 方法

```csharp
/// <summary>
/// 获取{fkCn}下拉列表（用于{entityCn}表单选择{fkCn}）
/// </summary>
/// <returns>{fkCn}Id和名称列表</returns>
Task<ListResultDto<{FK}LookupDto>> Get{FK}LookupAsync();
```

接口 using 中增加：不需要额外 using，`{FK}LookupDto` 在同一命名空间。

### 7. {Entity}AppService.cs — 核心差异

**增加仓储注入：**

```csharp
/// <summary>
/// {fkCn}仓储
/// </summary>
private readonly IRepository<{FK}, Guid> _{fk}Repository;

/// <summary>
/// 异步查询执行器
/// </summary>
private readonly IAsyncQueryableExecuter _asyncExecuter;
```

构造函数增加参数：`IRepository<{FK}, Guid> {fk}Repository, IAsyncQueryableExecuter asyncExecuter`

增加 using：`using TreadSnow.{FKs};` 和 `using Volo.Abp.Linq;`

**GetAsync — 增加关联名称查询：**

```csharp
public async Task<{Entity}Dto> GetAsync(Guid id)
{
    var entity = await _repository.GetAsync(id);
    var dto = ObjectMapper.Map<{Entity}, {Entity}Dto>(entity);

    var {fk} = await _{fk}Repository.FindAsync(entity.{fkId});
    dto.{fkName} = {fk}?.Name;

    var dtoList = new List<{Entity}Dto> { dto };
    await FillLookupNamesAsync(dtoList);
    await FillPermissionsAsync(dtoList);
    return dto;
}
```

**GetListAsync / GetExportListAsync — 增加批量关联查询：**

在 `ObjectMapper.Map` 之后、`FillLookupNamesAsync` 之前插入：

```csharp
var {fk}Ids = items.Select(p => p.{fkId}).Distinct().ToList();
var {fk}Queryable = await _{fk}Repository.GetQueryableAsync();
var {fk}s = await _asyncExecuter.ToListAsync({fk}Queryable.Where(a => {fk}Ids.Contains(a.Id)));
var {fk}Dict = {fk}s.ToDictionary(a => a.Id, a => a.Name);

foreach (var dto in dtos)
{
    {fk}Dict.TryGetValue(dto.{fkId}, out var {fk}Name);
    dto.{fkName} = {fk}Name;
}
```

**注意**：GetListAsync 中的日期范围过滤、`totalCount` 和分页查询应使用 `_asyncExecuter`。完整的查询构建流程如下（在 Name/OwnerId 过滤之后、`ApplyReadFilterAsync` 之前插入日期范围过滤）：

```csharp
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

var totalCount = await _asyncExecuter.CountAsync(query);
query = query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount);
var items = await _asyncExecuter.ToListAsync(query);
```

GetExportListAsync 同样需要在 `ApplyReadFilterAsync` 之前插入上述4个日期范围过滤条件。

**CreateAsync — 增加外键存在性校验：**

```csharp
[Authorize(TreadSnowPermissions.{Entities}.Create)]
public async Task<{Entity}Dto> CreateAsync(Create{Entity}Dto input)
{
    var {fk} = await _{fk}Repository.FindAsync(input.{fkId});
    if ({fk} == null)
    {
        throw new UserFriendlyException("{FK} not found");
    }
    var entity = ObjectMapper.Map<Create{Entity}Dto, {Entity}>(input);
    entity.TenantId = CurrentTenant.Id;
    entity.OwnerId = input.OwnerId ?? CurrentUser.Id;
    await _repository.InsertAsync(entity);
    return ObjectMapper.Map<{Entity}, {Entity}Dto>(entity);
}
```

增加 using: `using Volo.Abp;`

**增加 Lookup 方法：**

```csharp
/// <summary>
/// 获取{fkCn}下拉列表（用于{entityCn}表单选择{fkCn}）
/// </summary>
/// <returns>{fkCn}Id和名称列表</returns>
public async Task<ListResultDto<{FK}LookupDto>> Get{FK}LookupAsync()
{
    var items = await _{fk}Repository.GetListAsync();
    return new ListResultDto<{FK}LookupDto>(
        ObjectMapper.Map<List<{FK}>, List<{FK}LookupDto>>(items)
    );
}
```

### 8. AutoMapper — 额外追加映射

```csharp
CreateMap<{FK}, {FK}LookupDto>();
```

---

## 前端差异模板

### 9. proxy/{entities}/models.ts — 增加字段和 LookupDto

`{Entity}Dto` 增加：

```typescript
/** 所属{fkCn}ID */
{fkId}?: string;
/** {fkCn}名称（关联查询） */
{fkName}?: string;
```

`Create{Entity}Dto` 和 `Update{Entity}Dto` 增加：

```typescript
/** 所属{fkCn}ID（必填） */
{fkId}: string;
```

新增 LookupDto：

```typescript
/** {fkCn}下拉选项DTO（用于{entityCn}表单选择{fkCn}） */
export interface {FK}LookupDto extends EntityDto<string> {
  /** {fkCn}名称 */
  name?: string;
}
```

### 10. proxy/{entities}/{entity}.service.ts — 增加 Lookup 方法

import 中增加 `{FK}LookupDto`。

增加方法：

```typescript
/** 获取{fkCn}下拉列表（用于选择{fkCn}） */
get{FK}Lookup = () =>
  this.restService.request<any, ListResultDto<{FK}LookupDto>>({ method: 'GET', url: '/api/app/{entity}/{fk}-lookup' }, { apiName: this.apiName });
```

### 11. {entity-kebab}/{entity-kebab}.component.ts — 增加外键相关

**增加属性：**

```typescript
/** {fkCn}下拉列表数据（用于选择{fkCn}） */
{fk}s: { id?: string; name?: string }[] = [];

/** {fkCn}筛选项列表（用于表格漏斗筛选） */
{fk}Filters: { text: string; value: string }[] = [];

/** {fkCn}筛选函数（前端筛选，按名称匹配） */
{fk}FilterFn = (selectedValues: string[], item: {Entity}Dto) => selectedValues.includes(item.{fkName} ?? '');
```

**增加排序函数：**

```typescript
/** 按{fkCn}名称排序函数 */
sortBy{FK}Name = (a: {Entity}Dto, b: {Entity}Dto) => (a.{fkName} ?? '').localeCompare(b.{fkName} ?? '');
```

**loadLookups() 增加：**

```typescript
this.{entity}Service.get{FK}Lookup().subscribe((response) => {
  this.{fk}s = response.items ?? [];
  this.{fk}Filters = (response.items ?? []).map((a) => ({ text: a.name ?? '', value: a.name ?? '' }));
});
```

**注意**：外键筛选的 `value` 使用 `name`（前端文本匹配），而负责人筛选使用 `id`（后端ID筛选）。

**buildForm() 增加外键字段：**

```typescript
{fkId}: [this.selected{Entity}.{fkId} || null, Validators.required],
```

**downloadXlsx() 增加外键名称：**

```typescript
'{fkCn}': d.{fkName} ?? '',
```

### 12. {entity-kebab}/{entity-kebab}.component.html — 增加外键列和表单字段

**thead 增加外键列**（带漏斗前端筛选）：

```html
<th nzWidth="120px" nzShowSort nzShowFilter [nzSortFn]="sortBy{FK}Name" [nzFilters]="{fk}Filters" [nzFilterFn]="{fk}FilterFn">{{ '::{fkId}' | abpLocalization }}</th>
```

**tbody 增加外键单元格：**

```html
<td><span class="cell-ellipsis" nz-tooltip [nzTooltipTitle]="row.{fkName}">{{ row.{fkName} }}</span></td>
```

**弹窗表单增加外键选择器：**

```html
<nz-form-item>
  <nz-form-label nzRequired nzFor="{entity}-{fkId}">{{ '::{fkId}' | abpLocalization }}</nz-form-label>
  <nz-form-control [nzErrorTip]="{fk}ErrorTpl">
    <nz-select id="{entity}-{fkId}" formControlName="{fkId}" nzPlaceHolder="Select a {fkCn}" nzShowSearch>
      <nz-option *ngFor="let {fk} of {fk}s" [nzValue]="{fk}.id" [nzLabel]="{fk}.name"></nz-option>
    </nz-select>
    <ng-template #{fk}ErrorTpl let-control>
      <ng-container *ngIf="control.hasError('required')">{{ '::{fkId}' | abpLocalization }} is required</ng-container>
    </ng-template>
  </nz-form-control>
</nz-form-item>
```

**弹窗宽度选择**：字段少（4个以内）用 `nzWidth="520px"` 单列布局，字段多用 `nzWidth="620px"` 双列布局。单列布局时不需要 `nz-row`/`nz-col`，也不需要在 module 中导入 `NzGridModule`。

---

## 筛选机制对照表

| 筛选类型 | 适用列 | 实现方式 | 数据流 |
|----------|--------|----------|--------|
| **名称搜索** | Name列 | `nz-filter-trigger` + `nameFilter` | 发送到后端重新查询 |
| **前端漏斗** | 外键列 | `[nzFilterFn]` 纯函数 | 仅过滤当前页，不请求后端 |
| **后端漏斗** | 负责人列 | `(nzFilterChange)` + `list.get()` | 发送 ownerId 到后端重新查询 |
| **日期范围筛选** | 创建时间/修改时间列 | `nzCustomFilter` + `nz-range-picker` + `list.get()` | 发送 startXxx/endXxx 到后端重新查询 |

## 多个外键的处理

如果实体有多个外键（如 `accountId` + `categoryId`），对每个外键分别：
1. Domain: 增加 `Guid {fkId}` 属性
2. DbContext: 增加 `b.HasOne<{FK}>()` 配置
3. DTO: 增加 `{fkId}` + `{fkName}` 字段
4. AppService: 注入对应仓储、GetAsync/GetListAsync/GetExportListAsync 中关联查询、增加 `Get{FK}LookupAsync()`、CreateAsync 中校验存在性
5. AutoMapper: 增加 `CreateMap<{FK}, {FK}LookupDto>()`
6. 前端: 各自的 `{fk}s` 数组、`{fk}Filters`、`{fk}FilterFn`、表格列、表单字段

## 生成注意事项

继承 newToBEntity 的全部注意事项，另外：
1. **外键必填**：C# 用 `[Required]` + `Guid`（非 `Guid?`），TypeScript 用 `Validators.required`
2. **前端筛选 value 用 name**：`{fk}Filters` 的 `value` 是 `name`（文本匹配），不是 `id`
3. **关联名称只读**：`{fkName}` 字段只在列表展示，不在表单中编辑
4. **创建时校验外键**：AppService 的 `CreateAsync` 中要先 `FindAsync` 校验外键实体存在
5. **Lookup 在 ngOnInit 加载**：所有 Lookup 数据在 `loadLookups()` 中一次性加载
6. **数据迁移**：因为有外键约束，迁移时目标表必须已存在
7. **外键字段本地化键必须使用实体前缀**：外键字段（如 `AccountId`）在不同实体中含义不同（Pet 中是"主人"，Opportunity 中是"客户"），因此前端 HTML 中使用 `::` 前缀的本地化键时，**必须使用 `'{Entity}:{fkId}'` 格式**（如 `'::Opportunity:AccountId'`），而不是共享的 `'::{fkId}'`。对应的本地化资源中也需添加实体前缀键（如 `"Opportunity:AccountId": "Customer"` / `"客户"`）。导出 Excel 的中文表头同样使用用户在表结构文档中给出的字段描述，不要照搬其他实体的描述
8. **AutoMapper 命名空间冲突**：当多个实体模块都有同名的 `AccountLookupDto` 时，`TreadSnowApplicationAutoMapperProfile.cs` 中的 `CreateMap<Account, AccountLookupDto>()` 会产生歧义。必须使用完全限定名，如 `CreateMap<Account, Pets.AccountLookupDto>()` 和 `CreateMap<Account, Opportunities.AccountLookupDto>()`

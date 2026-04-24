---
name: modifyToBEntity
description: 修改已有ToB业务实体：对比表结构与Domain实体类，识别新增字段（普通字段或外键字段），自动修改全栈对应文件
---

# modifyToBEntity - 修改已有ToB业务实体（新增字段）

## 使用方式

用户提供**实体英文名** + **最新完整表结构**（markdown表格），AI 执行以下流程：

1. 读取现有 `src/TreadSnow.Domain/{Entities}/{Entity}.cs`，提取已有字段列表
2. 对比用户提供的表结构，**仅识别新增字段**（忽略 TenantId、No、OwnerId、OwnerTeamId 等基础设施字段）
3. 对每个新增字段进行分类：**普通字段** 或 **外键字段**
4. 按字段类型分别执行对应的修改清单

**不处理删除字段**，只做增量修改。

## 字段分类规则

### 普通字段
类型为 `string`、`int`、`bool`、`decimal`、`DateTime`、`double`、`float`、`long` 等基础类型。

### 外键字段
同时满足以下条件：
- C# 类型为 `Guid`（非 `Guid?` 表示必填外键，`Guid?` 表示选填外键）
- 字段名以 `Id` 结尾（如 `AccountId`、`CategoryId`）
- 用户在表结构的**备注列**中指明了关联实体（如 "关联Account" 或直接写关联实体中文名如 "客户"）

如果 Guid 类型字段不以 `Id` 结尾或无关联实体说明，视为普通 Guid 字段。

---

## 占位符约定

继承 newToBEntity 的占位符，另外新增字段使用：

| 占位符 | 含义 | 示例 |
|--------|------|------|
| `{Field}` | 新增字段PascalCase | `Phone` |
| `{field}` | 新增字段camelCase | `phone` |
| `{fieldCn}` | 字段中文名 | `手机号码` |
| `{FieldType}` | C#类型 | `string` |
| `{len}` | 字段长度（string类型） | `64` |
| `{FK}` | 外键关联实体PascalCase | `Account` |
| `{fk}` | 外键关联实体camelCase | `account` |
| `{fkId}` | 外键字段名PascalCase | `AccountId` |
| `{fkName}` | 外键关联名称字段 | `AccountName` |
| `{fkCn}` | 外键实体中文名（取用户表结构中的字段描述） | `客户` |

---

## 一、普通字段修改清单

对于每个新增普通字段，依次修改以下文件：

### 1. Domain/{Entities}/{Entity}.cs — 添加属性

在 `OwnerTeamId` 属性**之前**、最后一个业务字段**之后**插入：

```csharp
/// <summary>
/// {fieldCn}
/// </summary>
public {FieldType} {Field} { get; set; }
```

类型映射：
- 必填 string → `public string {Field} { get; set; }`
- 选填 string → `public string? {Field} { get; set; }`
- 必填 int → `public int {Field} { get; set; }`
- 选填 int → `public int? {Field} { get; set; }`
- bool → `public bool {Field} { get; set; }`
- 必填 decimal → `public decimal {Field} { get; set; }`
- 选填 DateTime → `public DateTime? {Field} { get; set; }`

### 2. DbContext OnModelCreating — 添加字段约束

在 `builder.Entity<{Entity}>(b => { ... })` 块中，现有约束之后追加：

```csharp
// 字段类型约束映射：
// 必填 string:  b.Property(x => x.{Field}).IsRequired().HasMaxLength({len}); //{fieldCn}
// 选填 string:  b.Property(x => x.{Field}).HasMaxLength({len}); //{fieldCn}
// 必填 decimal: b.Property(x => x.{Field}).HasColumnType("decimal(18,2)"); //{fieldCn}
// bool:         b.Property(x => x.{Field}).HasDefaultValue(false); //{fieldCn}
// int/DateTime: 通常不需要额外约束（EF Core 自动处理）
```

### 3. {Entity}Dto.cs — 添加属性

在 `OwnerId` 之前、现有业务字段之后追加：

```csharp
/// <summary>
/// {fieldCn}
/// </summary>
public {FieldType} {Field} { get; set; }
```

### 4. Create{Entity}Dto.cs — 添加属性

```csharp
// 必填字段：
/// <summary>
/// {fieldCn}
/// </summary>
[Required]
public {FieldType} {Field} { get; set; } = {默认值};

// 选填字段：
/// <summary>
/// {fieldCn}
/// </summary>
public {FieldType}? {Field} { get; set; }
```

默认值映射：`string` → `string.Empty`，`int` → `0`，`bool` → `false`，`decimal` → `0m`

### 5. Update{Entity}Dto.cs — 与 Create 相同

### 6. Localization en.json — 追加字段键

在该实体的本地化键组中追加：

```json
"{Field}": "{FieldEnglish}"
```

**注意**：如果该键已存在（被其他实体使用且含义相同），则不重复添加。如果同一键在不同实体中含义不同，使用 `{Entity}:{Field}` 前缀格式。

### 7. Localization zh-Hans.json — 追加字段键

```json
"{Field}": "{fieldCn}"
```

同上，含义冲突时使用 `{Entity}:{Field}` 前缀格式。

### 8. 前端 models.ts — 添加接口字段

**{Entity}Dto 接口**中追加：

```typescript
/** {fieldCn} */
{field}?: {tsType};
```

TypeScript 类型映射：`string` → `string`，`int/decimal/double` → `number`，`bool` → `boolean`，`DateTime` → `string`，`Guid` → `string`

**Create{Entity}Dto / Update{Entity}Dto 接口**中追加：

```typescript
// 必填：
/** {fieldCn}（必填） */
{field}: {tsType};

// 选填：
/** {fieldCn} */
{field}?: {tsType};
```

### 9. 前端 component.ts — 添加排序 + 表单 + 导出

**排序函数**（在现有排序函数之后追加）：

```typescript
// string:
/** 按{fieldCn}排序函数 */
sortBy{Field} = (a: {Entity}Dto, b: {Entity}Dto) => (a.{field} ?? '').localeCompare(b.{field} ?? '');

// number:
/** 按{fieldCn}排序函数 */
sortBy{Field} = (a: {Entity}Dto, b: {Entity}Dto) => (a.{field} ?? 0) - (b.{field} ?? 0);

// boolean 不需要排序函数
```

**buildForm() 中追加表单字段**：

```typescript
// 必填：
{field}: [this.selected{Entity}.{field} || {默认值}, Validators.required],

// 选填：
{field}: [this.selected{Entity}.{field} || null],
```

TypeScript 默认值：`string` → `''`，`number` → `null`，`boolean` → `false`

**downloadXlsx() 中追加导出列**：

```typescript
'{fieldCn}': d.{field} ?? '',
```

### 10. 前端 component.html — 添加表格列 + 表单项

**thead 追加列**（在 OwnerId 列之前，业务字段区域）：

```html
<th nzWidth="120px" nzShowSort [nzSortFn]="sortBy{Field}">{{ '::{Field}' | abpLocalization }}</th>
```

**tbody 追加单元格**（对应位置）：

```html
<!-- string/number: -->
<td><span class="cell-ellipsis" nz-tooltip [nzTooltipTitle]="row.{field}">{{ row.{field} }}</span></td>

<!-- DateTime: -->
<td><span class="cell-ellipsis" nz-tooltip [nzTooltipTitle]="row.{field} | date:'yyyy-MM-dd HH:mm'">{{ row.{field} | date:'yyyy-MM-dd HH:mm' }}</span></td>

<!-- boolean: -->
<td><nz-tag [nzColor]="row.{field} ? 'green' : 'default'">{{ row.{field} ? '是' : '否' }}</nz-tag></td>
```

**弹窗表单追加表单项**（在 OwnerId 表单项之前）：

```html
<!-- 必填 string/number input: -->
<nz-form-item>
  <nz-form-label nzRequired nzFor="{entity}-{field}">{{ '::{Field}' | abpLocalization }}</nz-form-label>
  <nz-form-control [nzErrorTip]="{field}ErrorTpl">
    <input nz-input id="{entity}-{field}" formControlName="{field}" />
    <ng-template #{field}ErrorTpl let-control>
      <ng-container *ngIf="control.hasError('required')">{{ '::{Field}' | abpLocalization }} is required</ng-container>
    </ng-template>
  </nz-form-control>
</nz-form-item>

<!-- 选填 string input: -->
<nz-form-item>
  <nz-form-label nzFor="{entity}-{field}">{{ '::{Field}' | abpLocalization }}</nz-form-label>
  <nz-form-control>
    <input nz-input id="{entity}-{field}" formControlName="{field}" />
  </nz-form-control>
</nz-form-item>

<!-- 选填长文本 textarea（字段长度 > 256 时使用）: -->
<nz-form-item>
  <nz-form-label nzFor="{entity}-{field}">{{ '::{Field}' | abpLocalization }}</nz-form-label>
  <nz-form-control>
    <textarea nz-input id="{entity}-{field}" formControlName="{field}" [nzAutosize]="{ minRows: 3, maxRows: 6 }"></textarea>
  </nz-form-control>
</nz-form-item>

<!-- boolean switch: -->
<nz-form-item>
  <nz-form-label nzFor="{entity}-{field}">{{ '::{Field}' | abpLocalization }}</nz-form-label>
  <nz-form-control>
    <nz-switch id="{entity}-{field}" formControlName="{field}"></nz-switch>
  </nz-form-control>
</nz-form-item>

<!-- DateTime date-picker: -->
<nz-form-item>
  <nz-form-label nzFor="{entity}-{field}">{{ '::{Field}' | abpLocalization }}</nz-form-label>
  <nz-form-control>
    <nz-date-picker id="{entity}-{field}" formControlName="{field}" style="width: 100%;"></nz-date-picker>
  </nz-form-control>
</nz-form-item>
```

### 11. module.ts — 按需追加 ng-zorro 模块

如果新增字段使用了新的 UI 组件，需要在 module.ts 的 imports 中追加：

| 组件 | 模块 | 触发条件 |
|------|------|----------|
| `<nz-tag>` | `NzTagModule` | bool 字段 |
| `<nz-date-picker>` | `NzDatePickerModule` | DateTime 字段 |
| `<nz-switch>` | `NzSwitchModule` | bool 字段 |
| `<nz-input-number>` | `NzInputNumberModule` | 需要步进器的 number 字段 |

### 12. nz-table nzScroll 宽度调整

每新增一个列，`[nzScroll]="{ x: 'XXXpx' }"` 的宽度值应增加对应列宽（通常 100-150px）。

---

## 二、外键字段修改清单

对于每个新增外键字段，**先执行上述普通字段清单的第 1-5 步**（Entity、DbContext、DTO），然后执行以下额外步骤：

### FK-1. Domain/{Entities}/{Entity}.cs — 外键属性

```csharp
/// <summary>
/// {fkCn}Id（外键）
/// </summary>
public Guid {fkId} { get; set; }
```

### FK-2. DbContext OnModelCreating — 外键约束

```csharp
b.HasOne<{FK}>().WithMany().HasForeignKey(x => x.{fkId}).IsRequired(); //{fkCn}
```

选填外键去掉 `.IsRequired()`。

### FK-3. {Entity}Dto.cs — 外键 + 关联名称

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

### FK-4. Create{Entity}Dto / Update{Entity}Dto — 外键字段

```csharp
/// <summary>
/// {fkCn}Id（必填）
/// </summary>
[Required]
public Guid {fkId} { get; set; }
```

### FK-5. 新建 {FK}LookupDto.cs（如果同命名空间下不存在）

**路径：** `src/TreadSnow.Application.Contracts/{Entities}/{FK}LookupDto.cs`

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

### FK-6. I{Entity}AppService.cs — 添加 Lookup 方法

```csharp
/// <summary>
/// 获取{fkCn}下拉列表（用于{entityCn}表单选择{fkCn}）
/// </summary>
/// <returns>{fkCn}Id和名称列表</returns>
Task<ListResultDto<{FK}LookupDto>> Get{FK}LookupAsync();
```

### FK-7. {Entity}AppService.cs — 多处修改

**增加字段和构造函数参数：**

```csharp
/// <summary>
/// {fkCn}仓储
/// </summary>
private readonly IRepository<{FK}, Guid> _{fk}Repository;
```

构造函数追加 `IRepository<{FK}, Guid> {fk}Repository` 参数和赋值。

如果 `_asyncExecuter` 尚未注入，同时追加 `IAsyncQueryableExecuter asyncExecuter`。

增加 using：`using TreadSnow.{FKs};`（FK实体的命名空间），`using Volo.Abp.Linq;`（如果新增 _asyncExecuter）

**GetAsync — 追加关联名称查询：**

在 `ObjectMapper.Map` 之后、`FillLookupNamesAsync` 之前：

```csharp
var {fk} = await _{fk}Repository.FindAsync(entity.{fkId});
dto.{fkName} = {fk}?.Name;
```

**GetListAsync / GetExportListAsync — 追加批量关联查询：**

在 `ObjectMapper.Map` 之后、`FillLookupNamesAsync` 之前：

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

**CreateAsync — 追加外键存在性校验：**

在 `ObjectMapper.Map` 之前：

```csharp
var {fk} = await _{fk}Repository.FindAsync(input.{fkId});
if ({fk} == null)
{
    throw new UserFriendlyException("{FK} not found");
}
```

增加 using：`using Volo.Abp;`（如果尚未存在）

**追加 Lookup 方法：**

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

### FK-8. AutoMapperProfile — 追加映射

```csharp
CreateMap<{FK}, {Entities}.{FK}LookupDto>();
```

**必须使用完全限定名**（`{Entities}.{FK}LookupDto`），防止多实体同名 LookupDto 产生歧义。

### FK-9. Localization — 外键字段本地化

**必须使用实体前缀**格式，避免与其他实体的同名外键冲突：

en.json:
```json
"{Entity}:{fkId}": "{fkEnglish}"
```

zh-Hans.json:
```json
"{Entity}:{fkId}": "{fkCn}"
```

### FK-10. 前端 models.ts — 外键字段 + LookupDto

**{Entity}Dto** 追加：
```typescript
/** 所属{fkCn}ID */
{fkId}?: string;
/** {fkCn}名称（关联查询） */
{fkName}?: string;
```

**Create{Entity}Dto / Update{Entity}Dto** 追加：
```typescript
/** 所属{fkCn}ID（必填） */
{fkId}: string;
```

**新增 LookupDto**（如果不存在）：
```typescript
/** {fkCn}下拉选项DTO */
export interface {FK}LookupDto extends EntityDto<string> {
  /** {fkCn}名称 */
  name?: string;
}
```

### FK-11. 前端 service.ts — 追加 Lookup 方法

import 中追加 `{FK}LookupDto`。

```typescript
/** 获取{fkCn}下拉列表（用于选择{fkCn}） */
get{FK}Lookup = () =>
  this.restService.request<any, ListResultDto<{FK}LookupDto>>({ method: 'GET', url: '/api/app/{entity}/{fk}-lookup' }, { apiName: this.apiName });
```

### FK-12. 前端 component.ts — 下拉数据 + 筛选 + 表单

**追加属性：**
```typescript
/** {fkCn}下拉列表数据 */
{fk}s: { id?: string; name?: string }[] = [];

/** {fkCn}筛选项列表 */
{fk}Filters: { text: string; value: string }[] = [];

/** {fkCn}筛选函数（前端筛选，按名称匹配） */
{fk}FilterFn = (selectedValues: string[], item: {Entity}Dto) => selectedValues.includes(item.{fkName} ?? '');
```

**追加排序函数：**
```typescript
/** 按{fkCn}名称排序函数 */
sortBy{FK}Name = (a: {Entity}Dto, b: {Entity}Dto) => (a.{fkName} ?? '').localeCompare(b.{fkName} ?? '');
```

**loadLookups() 追加：**
```typescript
this.{entity}Service.get{FK}Lookup().subscribe((response) => {
  this.{fk}s = response.items ?? [];
  this.{fk}Filters = (response.items ?? []).map((a) => ({ text: a.name ?? '', value: a.name ?? '' }));
});
```

**buildForm() 追加：**
```typescript
{fkId}: [this.selected{Entity}.{fkId} || null, Validators.required],
```

**downloadXlsx() 追加：**
```typescript
'{fkCn}': d.{fkName} ?? '',
```

### FK-13. 前端 component.html — 外键列 + 选择器

**thead 追加外键列**（带漏斗前端筛选，在 OwnerId 列之前）：
```html
<th nzWidth="120px" nzShowSort nzShowFilter [nzSortFn]="sortBy{FK}Name" [nzFilters]="{fk}Filters" [nzFilterFn]="{fk}FilterFn">{{ '::{Entity}:{fkId}' | abpLocalization }}</th>
```

**tbody 追加单元格：**
```html
<td><span class="cell-ellipsis" nz-tooltip [nzTooltipTitle]="row.{fkName}">{{ row.{fkName} }}</span></td>
```

**弹窗表单追加选择器**（在 OwnerId 选择器之前）：
```html
<nz-form-item>
  <nz-form-label nzRequired nzFor="{entity}-{fkId}">{{ '::{Entity}:{fkId}' | abpLocalization }}</nz-form-label>
  <nz-form-control [nzErrorTip]="{fk}ErrorTpl">
    <nz-select id="{entity}-{fkId}" formControlName="{fkId}" nzPlaceHolder="Select a {fkCn}" nzShowSearch>
      <nz-option *ngFor="let {fk} of {fk}s" [nzValue]="{fk}.id" [nzLabel]="{fk}.name"></nz-option>
    </nz-select>
    <ng-template #{fk}ErrorTpl let-control>
      <ng-container *ngIf="control.hasError('required')">{{ '::{Entity}:{fkId}' | abpLocalization }} is required</ng-container>
    </ng-template>
  </nz-form-control>
</nz-form-item>
```

---

## 三、最终步骤

### EF Core 数据迁移

所有代码修改完成后执行：

```bash
dotnet ef migrations add Modify{Entity}_Add{Field1}{Field2}... -p src/TreadSnow.EntityFrameworkCore -s src/TreadSnow.HttpApi.Host
```

迁移名称格式：`Modify{Entity}_Add` + 新增字段名组合（如 `ModifyOpportunity_AddPhoneEmail`）。

### 编译验证

```bash
dotnet build TreadSnow.sln
```

---

## 执行注意事项

1. **仅做增量**：不删除、不重命名已有字段，只添加新字段
2. **字段插入位置**：新业务字段插在 OwnerId/OwnerTeamId 之前，保持基础设施字段在最后
3. **本地化键冲突**：同一字段名在不同实体中含义不同时，必须用 `{Entity}:{Field}` 前缀格式
4. **AutoMapper 无需改动**：普通字段同名映射自动处理，仅外键 LookupDto 需要手动添加映射
5. **弹窗布局调整**：字段总数超过 4 个时，考虑从 `nzWidth="520px"` 单列改为 `nzWidth="620px"` 双列布局（使用 `nz-row` + `nz-col [nzSpan]="12"`），并在 module.ts 中导入 `NzGridModule`
6. **表格滚动宽度**：每新增一列，更新 `nzScroll` 的 `x` 值（每列约 100-150px）
7. **中文注释**：所有 C# 属性用 `/// <summary>` 注释，TypeScript 用 `/** */` 注释
8. **参数不换行**：构造函数和方法参数全部写在同一行
9. **字段描述以用户表结构为准**：不要照搬其他实体对同名字段的描述，以用户当前提供的"字段描述"列为准

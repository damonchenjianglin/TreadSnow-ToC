# TreadSnow.Elasticsearch.Logging 使用说明

## 一、快速接入（3步）

### 1. 添加项目引用

在你的宿主项目（如 `HttpApi.Host`）的 `.csproj` 中添加：

```xml
<ProjectReference Include="..\TreadSnow.Elasticsearch.Logging\TreadSnow.Elasticsearch.Logging.csproj" />
```

### 2. 添加ABP模块依赖

在宿主模块类上添加 `DependsOn`：

```csharp
using TreadSnow.Elasticsearch.Logging;

[DependsOn(
    typeof(TreadSnowElasticsearchLoggingModule),
    // ... 其他模块
)]
public class YourHostModule : AbpModule { }
```

### 3. 配置 appsettings.json + Program.cs

**appsettings.json** 添加配置节：

```json
{
  "ElasticsearchLogging": {
    "Urls": ["http://你的ES地址:9200"],
    "Username": "elastic",
    "Password": "你的密码",
    "DataStream": "logs-你的应用名-default",
    "MaxExportSize": 1000,
    "MaxLifeTime": "00:00:05",
    "MaxConcurrency": 10,
    "MaxInflight": 100000,
    "MinimumLevel": "Information",
    "FullMode": "Wait"
  }
}
```

**Program.cs** 在 `UseSerilog` 配置中添加一行：

```csharp
using TreadSnow.Elasticsearch.Logging.Extensions;

// 在 UseSerilog 回调的最后加一行
.WriteToElasticsearch(context.Configuration, services);
```

完整示例：

```csharp
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Warning()
        .Enrich.FromLogContext()
        .WriteTo.Async(c => c.File("Logs/logs.txt"))
        .WriteTo.Async(c => c.Console())
        .WriteToElasticsearch(context.Configuration, services);
});
```

---

## 二、配置参数详解

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Urls` | string[] | 必填 | ES节点地址列表，支持多节点 |
| `Username` | string | 可选 | Basic Auth用户名 |
| `Password` | string | 可选 | Basic Auth密码 |
| `DataStream` | string | `logs-app-default` | Data Stream名称，格式 `type-dataset-namespace` |
| `MaxExportSize` | int | 1000 | 每批次最大导出文档数 |
| `MaxLifeTime` | TimeSpan | 5秒 | 缓冲区最大等待时间，超时即刷新 |
| `MaxConcurrency` | int | 10 | 并发写入ES的线程数 |
| `MaxInflight` | int | 100000 | 内存中最大待发送事件数（背压控制） |
| `MinimumLevel` | LogEventLevel | Information | 写入ES的最低日志级别 |
| `FullMode` | string | Wait | 队列满时行为：Wait（等待）或 DropOldest |

**异步批量写入原理：**

```
应用代码写日志 → 进入内存缓冲区（零阻塞）
                → 后台线程每隔 MaxLifeTime 或缓冲区满 MaxExportSize 条
                → 批量通过 _bulk API 写入 ES
                → MaxConcurrency 个线程并发消费
                → MaxInflight 控制背压上限
```

主线程写日志是完全无阻塞的，不会影响API响应速度。

---

## 三、业务日志最佳实践

### 结构化日志写法

使用Serilog结构化日志语法，属性名用 `{PropertyName}` 占位符：

```csharp
// 好 — 结构化，可在Kibana按字段搜索
Logger.LogInformation("客户创建成功 AccountId:{AccountId} Name:{Name} Phone:{Phone}",
    account.Id, account.Name, account.Phone);

// 坏 — 字符串拼接，无法按字段搜索
Logger.LogInformation($"客户创建成功 {account.Id} {account.Name}");
```

### 日志级别选择建议

| 级别 | 使用场景 | 示例 |
|------|---------|------|
| `Debug` | 调试信息，生产环境通常关闭 | 方法参数值、SQL语句 |
| `Information` | 关键业务操作成功 | 客户创建、订单完成 |
| `Warning` | 异常但可恢复的情况 | 重试操作、降级处理 |
| `Error` | 操作失败需要关注 | 外部API调用失败、数据不一致 |
| `Fatal` | 系统无法继续运行 | 数据库连接断开、关键配置缺失 |

### 多租户日志自动注入

`TenantEnricher` 会自动在每条日志中注入：

| 字段 | 说明 | 示例值 |
|------|------|--------|
| `TenantId` | 当前租户ID | `3a0f...`（GUID） |
| `TenantName` | 当前租户名称 | `某某公司` |

无需手动传入，Enricher在日志产生时自动从ABP的 `ICurrentTenant` 捕获。

**不同场景的TenantId值：**
- 有租户登录：`TenantId` = 租户GUID
- 宿主管理员（Host）：`TenantId` = `host`
- 后台任务/无HttpContext：`TenantId` = `system`

---

## 四、Kibana查询完整教学

> 以下教学基于 Kibana 9.x，ES 地址 http://139.9.83.8:9200

### 4.1 创建 Data View（必须先做）

1. 打开 Kibana（通常是 http://139.9.83.8:5601）
2. 左侧菜单栏 → 最下方 **Stack Management**（齿轮图标）
3. 左侧选择 **Kibana** → **Data Views**
4. 点击右上角蓝色按钮 **Create data view**
5. 填写以下信息：
   - **Name**: `treadsnow-logs`
   - **Index pattern**: `logs-treadsnow-default`（与appsettings中的DataStream一致）
   - **Timestamp field**: 选择 `@timestamp`
6. 点击 **Save data view to Kibana**

### 4.2 Discover 页面 — 基础查询

1. 左侧菜单 → **Discover**（指南针图标）
2. 左上角选择刚创建的 `treadsnow-logs` Data View
3. 右上角时间选择器，选择 **Last 24 hours**（或你需要的时间范围）

**添加显示列（让表格更清晰）：**
1. 左侧 Available fields 列表中找到以下字段
2. 鼠标悬停 → 点击 **+** 号添加为显示列：
   - `log.level`
   - `message`
   - `labels.TenantName`
   - `labels.SourceContext`

**KQL查询语法（在搜索栏输入）：**

| 需求 | KQL查询 |
|------|---------|
| 查看所有日志 | 不输入任何内容，直接查看 |
| 查某个租户的日志 | `labels.TenantId: "你的租户guid"` |
| 查客户创建记录 | `message: "客户创建成功"` |
| 查某个手机号 | `labels.Phone: "13800138001"` |
| 查某个客户ID | `labels.AccountId: "guid值"` |
| 只看错误日志 | `log.level: "Error"` |
| 只看警告日志 | `log.level: "Warning"` |
| 查某个服务类的日志 | `labels.SourceContext: "*AccountAppService*"` |
| 组合查询 | `log.level: "Error" and labels.TenantId: "xxx"` |
| 排除某级别 | `not log.level: "Debug"` |
| 时间范围+条件 | 先在时间选择器选范围，再输入KQL |

**注意：** ECS（Elastic Common Schema）格式下，自定义属性在 `labels.*` 命名空间下。

### 4.3 创建 Dashboard — 5个监控面板

点击左侧菜单 → **Dashboard** → **Create dashboard**

---

#### 面板1：日志趋势折线图

1. 点击 **Create visualization**
2. 选择 `treadsnow-logs` Data View
3. 左侧 Visualization type 选择 **Line**
4. 配置：
   - **Horizontal axis**: 点击 → 选择 `@timestamp` → Function: `Date histogram` → Minimum interval: `Auto`
   - **Vertical axis**: 保持默认 `Count`
   - **Break down by**: 点击 → 选择 `log.level` → Top values → Number of values: `5`
5. 点击右上角 **Save and return**
6. 在面板标题处点击编辑图标，改名为 **日志趋势**

**效果：** 一条折线图显示各级别日志随时间的变化趋势，每个级别一种颜色。

---

#### 面板2：错误来源TOP10柱状图

1. 点击 **Create visualization**
2. Visualization type: **Bar horizontal**
3. 配置：
   - **Vertical axis**: 选择 `labels.SourceContext.keyword` → Top values → Number: `10`
   - **Horizontal axis**: `Count`
4. 点击搜索栏，输入过滤条件：`log.level: "Error"`
5. **Save and return** → 改名 **错误来源TOP10**

**效果：** 横向柱状图显示产生Error最多的10个类/服务。

---

#### 面板3：租户日志分布饼图

1. 点击 **Create visualization**
2. Visualization type: **Pie**
3. 配置：
   - **Slice by**: 选择 `labels.TenantName.keyword` → Top values → Number: `10`
   - **Size by**: `Count`
4. **Save and return** → 改名 **租户日志分布**

**效果：** 饼图直观展示各租户日志量占比。

---

#### 面板4：最近错误列表

1. 点击 **Create visualization**
2. Visualization type: **Table**
3. 配置：
   - **Columns**（点击 + 号逐个添加）:
     - `@timestamp`
     - `log.level`
     - `message`
     - `labels.TenantName`
     - `labels.SourceContext`
   - **Rows per page**: `50`
4. 搜索栏输入：`log.level: "Error"`
5. 点击 `@timestamp` 列头 → 排序：降序（最新在前）
6. **Save and return** → 改名 **最近错误列表**

**效果：** 表格展示最近50条错误日志，包含时间、消息、租户、来源。

---

#### 面板5：今日业务操作统计

1. 点击 **Create visualization**
2. Visualization type: **Metric**
3. 配置：
   - **Metric**: `Count`
4. 搜索栏输入：`message: "客户创建成功"`
5. 右上角时间选择器选 **Today**
6. **Save and return** → 改名 **今日客户创建数**

**效果：** 一个大数字显示今天创建了多少客户。

---

#### 保存Dashboard

1. 点击右上角 **Save**
2. **Title**: `TreadSnow运维监控面板`
3. **Description**: `日志趋势、错误监控、租户分布、业务统计`
4. 点击 **Save**

以后每次打开 Dashboard 就能看到实时监控面板了。

### 4.4 日常使用技巧

**快捷过滤：**
- 在任何表格中点击某个字段值 → 选择 **Filter for value**（+号）→ 自动添加过滤条件
- 点击 **Filter out value**（-号）→ 排除该值

**时间范围：**
- 右上角时间选择器支持：Last 15 min / 1h / 24h / 7d / 自定义范围
- 支持拖拽折线图选择时间范围

**保存搜索：**
- 在 Discover 页面配置好查询条件和列后
- 点击 **Save** → 输入名称 → 以后可以快速打开

---

## 五、故障排查

### ES连接失败不影响主程序

本类库使用 `BootstrapMethod.Silent`，即使ES不可达，应用也能正常启动和运行。日志会丢失，但不会崩溃。

### 如何查看Sink内部错误

如果怀疑日志没有写入ES，检查以下几点：

1. **ES是否可达**：

```bash
curl -u elastic:密码 "http://ES地址:9200/_cluster/health?pretty"
```

应返回 `status: green` 或 `yellow`。

2. **Data Stream是否创建**：

```bash
curl -u elastic:密码 "http://ES地址:9200/_data_stream/logs-treadsnow-default?pretty"
```

3. **查看最新日志**：

```bash
curl -u elastic:密码 "http://ES地址:9200/logs-treadsnow-default/_search?pretty&size=3&sort=@timestamp:desc"
```

4. **Serilog自身日志**：在 `Program.cs` 中添加 Serilog 自诊断：

```csharp
Serilog.Debugging.SelfLog.Enable(Console.Error);
```

### 日志延迟说明

日志不是实时出现在ES中，有最大 `MaxLifeTime`（默认5秒）的缓冲延迟。这是为了批量写入提高性能。如果需要更实时，可以将 `MaxLifeTime` 调小（但会增加ES写入压力）。

### 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| Kibana看不到数据 | Data View未创建或pattern不匹配 | 检查Index pattern是否为 `logs-treadsnow-default` |
| 有数据但搜不到 | KQL语法错误 | ECS格式下自定义字段在 `labels.*` 下 |
| 日志级别不对 | MinimumLevel配置 | appsettings中配的是写入ES的最低级别 |
| ES磁盘满 | 日志量太大 | 配置ILM策略或定期清理旧索引 |

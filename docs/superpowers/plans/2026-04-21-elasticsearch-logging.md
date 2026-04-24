# TreadSnow.Elasticsearch.Logging 实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 创建独立类库 `TreadSnow.Elasticsearch.Logging`，将Serilog结构化日志异步批量写入ES 9.3.3，支持ABP多租户字段隔离。

**Architecture:** 基于官方 `Elastic.Serilog.Sinks` 9.0.0（支持ES 9.x），使用Data Stream模式，通过TenantEnricher将租户信息注入日志字段，Kibana按TenantId字段过滤实现租户隔离。类库只依赖ABP抽象层，不引用业务代码。

**Tech Stack:** .NET 9.0, Elastic.Serilog.Sinks 9.0.0, Volo.Abp.Core 9.1.1, Volo.Abp.MultiTenancy.Abstractions 9.1.1, Serilog.Settings.Configuration

**设计变更说明:** 原设计使用已废弃的 `Serilog.Sinks.Elasticsearch`（不支持ES 9.x）。改用官方 `Elastic.Serilog.Sinks` 9.0.0，采用Data Stream + 字段级租户隔离（而非per-tenant索引），这是ES 9.x推荐模式，避免索引爆炸。

---

## 文件结构

```
src/TreadSnow.Elasticsearch.Logging/                     # 新建类库项目
├── TreadSnow.Elasticsearch.Logging.csproj                # 项目文件
├── ElasticsearchLoggingOptions.cs                        # 配置选项类
├── Enrichers/
│   └── TenantEnricher.cs                                 # ABP多租户Enricher
├── Extensions/
│   └── SerilogElasticsearchExtensions.cs                 # 扩展方法入口
└── TreadSnowElasticsearchLoggingModule.cs                # ABP模块

src/TreadSnow.HttpApi.Host/Program.cs                     # 修改：添加ES Sink调用
src/TreadSnow.HttpApi.Host/TreadSnowHttpApiHostModule.cs  # 修改：添加模块依赖
src/TreadSnow.HttpApi.Host/TreadSnow.HttpApi.Host.csproj  # 修改：添加项目引用
src/TreadSnow.HttpApi.Host/appsettings.json               # 修改：添加ES配置节
src/TreadSnow.Application/Accounts/AccountAppService.cs   # 修改：添加业务日志
TreadSnow.sln                                             # 修改：添加新项目
```

---

### Task 1: 创建类库项目和csproj

**Files:**
- Create: `src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj`

- [ ] **Step 1: 创建项目目录结构**

```bash
mkdir -p src/TreadSnow.Elasticsearch.Logging/Enrichers
mkdir -p src/TreadSnow.Elasticsearch.Logging/Extensions
```

- [ ] **Step 2: 创建csproj文件**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Import Project="..\..\common.props" />

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <RootNamespace>TreadSnow.Elasticsearch.Logging</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Elastic.Serilog.Sinks" Version="9.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
    <PackageReference Include="Volo.Abp.Core" Version="9.1.1" />
    <PackageReference Include="Volo.Abp.MultiTenancy.Abstractions" Version="9.1.1" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: 将项目添加到解决方案**

```bash
dotnet sln TreadSnow.sln add src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj --solution-folder src
```

- [ ] **Step 4: 验证项目编译通过**

```bash
dotnet build src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj
```

Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj TreadSnow.sln
git commit -m "feat: 创建TreadSnow.Elasticsearch.Logging类库项目骨架"
```

---

### Task 2: 实现 ElasticsearchLoggingOptions 配置选项

**Files:**
- Create: `src/TreadSnow.Elasticsearch.Logging/ElasticsearchLoggingOptions.cs`

- [ ] **Step 1: 创建配置选项类**

```csharp
using Serilog.Events;

namespace TreadSnow.Elasticsearch.Logging
{
    /// <summary>
    /// Elasticsearch日志配置选项，对应appsettings.json中的ElasticsearchLogging节点
    /// </summary>
    public class ElasticsearchLoggingOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "ElasticsearchLogging";

        /// <summary>
        /// ES节点地址列表
        /// </summary>
        public string[] Urls { get; set; } = [];

        /// <summary>
        /// Basic Auth用户名（可选）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Basic Auth密码（可选）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Data Stream名称，格式为 type-dataset-namespace，默认 logs-app-default
        /// </summary>
        public string DataStream { get; set; } = "logs-app-default";

        /// <summary>
        /// 每批最大导出条数
        /// </summary>
        public int MaxExportSize { get; set; } = 1000;

        /// <summary>
        /// 批量刷新最大等待时间
        /// </summary>
        public TimeSpan MaxLifeTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 并发消费者数量
        /// </summary>
        public int MaxConcurrency { get; set; } = 10;

        /// <summary>
        /// 内存队列中最大待发送事件数（背压控制）
        /// </summary>
        public int MaxInflight { get; set; } = 100000;

        /// <summary>
        /// 写入ES的最低日志级别
        /// </summary>
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        /// 队列满时的行为：Wait（等待）或 DropOldest（丢弃最旧）
        /// </summary>
        public string FullMode { get; set; } = "Wait";
    }
}
```

- [ ] **Step 2: 验证编译通过**

```bash
dotnet build src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj
```

Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add src/TreadSnow.Elasticsearch.Logging/ElasticsearchLoggingOptions.cs
git commit -m "feat: 添加ElasticsearchLoggingOptions配置选项类"
```

---

### Task 3: 实现 TenantEnricher 多租户Enricher

**Files:**
- Create: `src/TreadSnow.Elasticsearch.Logging/Enrichers/TenantEnricher.cs`

- [ ] **Step 1: 创建TenantEnricher**

关键设计点：在日志产生时（主线程）从ICurrentTenant捕获TenantId，写入LogEvent Properties。后台Sink消费线程不需要再访问HttpContext。

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using System;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Elasticsearch.Logging.Enrichers
{
    /// <summary>
    /// ABP多租户日志Enricher，在日志产生时捕获当前租户信息写入LogEvent属性
    /// </summary>
    public class TenantEnricher : ILogEventEnricher
    {
        /// <summary>
        /// 服务提供者，用于解析Scoped的ICurrentTenant
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public TenantEnricher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 将当前租户Id和名称注入日志事件属性
        /// </summary>
        /// <param name="logEvent">日志事件</param>
        /// <param name="propertyFactory">属性工厂</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var currentTenant = scope.ServiceProvider.GetService<ICurrentTenant>();
                if (currentTenant == null) return;

                var tenantId = currentTenant.Id?.ToString() ?? "host";
                var tenantName = currentTenant.Name ?? "host";

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantName", tenantName));
            }
            catch
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", "system"));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantName", "system"));
            }
        }
    }
}
```

- [ ] **Step 2: 验证编译通过**

```bash
dotnet build src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj
```

Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add src/TreadSnow.Elasticsearch.Logging/Enrichers/TenantEnricher.cs
git commit -m "feat: 实现TenantEnricher多租户日志上下文注入"
```

---

### Task 4: 实现 SerilogElasticsearchExtensions 扩展方法

**Files:**
- Create: `src/TreadSnow.Elasticsearch.Logging/Extensions/SerilogElasticsearchExtensions.cs`

- [ ] **Step 1: 创建扩展方法**

```csharp
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Linq;
using TreadSnow.Elasticsearch.Logging.Enrichers;

namespace TreadSnow.Elasticsearch.Logging.Extensions
{
    /// <summary>
    /// Serilog LoggerConfiguration的Elasticsearch扩展方法
    /// </summary>
    public static class SerilogElasticsearchExtensions
    {
        /// <summary>
        /// 将日志写入Elasticsearch，从IConfiguration读取ElasticsearchLogging配置节
        /// </summary>
        /// <param name="loggerConfiguration">Serilog日志配置</param>
        /// <param name="configuration">应用配置</param>
        /// <param name="serviceProvider">服务提供者，用于解析多租户上下文</param>
        /// <returns>日志配置（链式调用）</returns>
        public static LoggerConfiguration WriteToElasticsearch(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var options = new ElasticsearchLoggingOptions();
            configuration.GetSection(ElasticsearchLoggingOptions.SectionName).Bind(options);

            if (options.Urls.Length == 0) return loggerConfiguration;

            var nodes = options.Urls.Select(u => new Uri(u)).ToArray();

            loggerConfiguration.Enrich.With(new TenantEnricher(serviceProvider));

            loggerConfiguration.MinimumLevel.Is(options.MinimumLevel);

            loggerConfiguration.WriteTo.Elasticsearch(nodes, opts =>
            {
                var parts = options.DataStream.Split('-');
                if (parts.Length >= 3)
                {
                    opts.DataStream = new DataStreamName(parts[0], parts[1], string.Join("-", parts.Skip(2)));
                }
                else
                {
                    opts.DataStream = new DataStreamName("logs", "app", "default");
                }

                opts.BootstrapMethod = BootstrapMethod.Silent;

                opts.ConfigureChannel = channelOpts =>
                {
                    channelOpts.BufferOptions = new BufferOptions
                    {
                        ExportMaxConcurrency = options.MaxConcurrency,
                        InboundBufferMaxSize = options.MaxInflight,
                        OutboundBufferMaxSize = options.MaxExportSize,
                        OutboundBufferMaxLifetime = options.MaxLifeTime,
                    };
                };

                if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                {
                    opts.Transport = new DistributedTransport(new TransportConfiguration(new StaticNodePool(nodes)).Authentication(new BasicAuthentication(options.Username, options.Password)));
                }
            });

            return loggerConfiguration;
        }
    }
}
```

- [ ] **Step 2: 验证编译通过**

```bash
dotnet build src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj
```

Expected: Build succeeded。如果有API不匹配的编译错误，根据 `Elastic.Serilog.Sinks` 9.0.0 的实际API调整代码。

- [ ] **Step 3: Commit**

```bash
git add src/TreadSnow.Elasticsearch.Logging/Extensions/SerilogElasticsearchExtensions.cs
git commit -m "feat: 实现WriteToElasticsearch扩展方法"
```

---

### Task 5: 实现 ABP Module

**Files:**
- Create: `src/TreadSnow.Elasticsearch.Logging/TreadSnowElasticsearchLoggingModule.cs`

- [ ] **Step 1: 创建ABP模块类**

```csharp
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Elasticsearch.Logging
{
    /// <summary>
    /// Elasticsearch日志ABP模块，在任何ABP项目中通过DependsOn一行启用
    /// </summary>
    [DependsOn(typeof(AbpMultiTenancyAbstractionsModule))]
    public class TreadSnowElasticsearchLoggingModule : AbpModule
    {
    }
}
```

- [ ] **Step 2: 验证编译通过**

```bash
dotnet build src/TreadSnow.Elasticsearch.Logging/TreadSnow.Elasticsearch.Logging.csproj
```

Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add src/TreadSnow.Elasticsearch.Logging/TreadSnowElasticsearchLoggingModule.cs
git commit -m "feat: 创建TreadSnowElasticsearchLoggingModule ABP模块"
```

---

### Task 6: 集成到HttpApi.Host

**Files:**
- Modify: `src/TreadSnow.HttpApi.Host/TreadSnow.HttpApi.Host.csproj`
- Modify: `src/TreadSnow.HttpApi.Host/TreadSnowHttpApiHostModule.cs:47-59`
- Modify: `src/TreadSnow.HttpApi.Host/Program.cs:27-37`
- Modify: `src/TreadSnow.HttpApi.Host/appsettings.json`

- [ ] **Step 1: 添加项目引用到HttpApi.Host.csproj**

在 `TreadSnow.HttpApi.Host.csproj` 的 `<ItemGroup>` 中添加（与其他ProjectReference同级）：

```xml
<ProjectReference Include="..\TreadSnow.Elasticsearch.Logging\TreadSnow.Elasticsearch.Logging.csproj" />
```

- [ ] **Step 2: 添加模块依赖到TreadSnowHttpApiHostModule.cs**

在 `[DependsOn(...)]` 列表末尾加一行：

```csharp
typeof(TreadSnowElasticsearchLoggingModule)
```

同时在文件顶部添加 using：

```csharp
using TreadSnow.Elasticsearch.Logging;
```

- [ ] **Step 3: 修改Program.cs的UseSerilog配置**

将 Program.cs 第27-37行的 `.UseSerilog(...)` 替换为：

```csharp
.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Warning()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Async(c => c.File("Logs/logs.txt"))
        .WriteTo.Async(c => c.Console())
        .WriteTo.Async(c => c.AbpStudio(services))
        .WriteToElasticsearch(context.Configuration, services.GetRequiredService<IServiceProvider>());
});
```

在文件顶部添加 using：

```csharp
using TreadSnow.Elasticsearch.Logging.Extensions;
```

注意：`services` 参数在 `UseSerilog` 回调中是 `IServiceProvider` 类型，可以直接传入。

- [ ] **Step 4: 在appsettings.json添加ES配置节**

在 `appsettings.json` 根级别添加：

```json
"ElasticsearchLogging": {
    "Urls": ["http://139.9.83.8:9200"],
    "Username": "elastic",
    "Password": "xiaoxiaoxi11",
    "DataStream": "logs-treadsnow-default",
    "MaxExportSize": 1000,
    "MaxLifeTime": "00:00:05",
    "MaxConcurrency": 10,
    "MaxInflight": 100000,
    "MinimumLevel": "Information",
    "FullMode": "Wait"
}
```

- [ ] **Step 5: 验证整体编译通过**

```bash
dotnet build TreadSnow.sln
```

Expected: Build succeeded

- [ ] **Step 6: Commit**

```bash
git add src/TreadSnow.HttpApi.Host/TreadSnow.HttpApi.Host.csproj src/TreadSnow.HttpApi.Host/TreadSnowHttpApiHostModule.cs src/TreadSnow.HttpApi.Host/Program.cs src/TreadSnow.HttpApi.Host/appsettings.json
git commit -m "feat: 集成Elasticsearch日志到HttpApi.Host"
```

---

### Task 7: 在AccountAppService中添加业务日志

**Files:**
- Modify: `src/TreadSnow.Application/Accounts/AccountAppService.cs:153-161`

- [ ] **Step 1: 在CreateAsync方法中添加结构化日志**

在 `AccountAppService.CreateAsync` 方法中，`await _repository.InsertAsync(account);` 之后、`return` 之前添加：

```csharp
Logger.LogInformation("客户创建成功 AccountId:{AccountId} Name:{Name} Phone:{Phone}", account.Id, account.Name, account.Phone);
```

完整的 CreateAsync 方法变为：

```csharp
[Authorize(TreadSnowPermissions.Accounts.Create)]
public async Task<AccountDto> CreateAsync(CreateAccountDto input)
{
    var account = ObjectMapper.Map<CreateAccountDto, Account>(input);
    account.TenantId = CurrentTenant.Id;
    account.OwnerId = input.OwnerId ?? CurrentUser.Id;
    await _repository.InsertAsync(account);
    Logger.LogInformation("客户创建成功 AccountId:{AccountId} Name:{Name} Phone:{Phone}", account.Id, account.Name, account.Phone);
    return ObjectMapper.Map<Account, AccountDto>(account);
}
```

注意：ABP的 `ApplicationService` 基类已内置 `Logger` 属性（`ILogger`），无需额外注入。

- [ ] **Step 2: 验证编译通过**

```bash
dotnet build src/TreadSnow.Application/TreadSnow.Application.csproj
```

Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add src/TreadSnow.Application/Accounts/AccountAppService.cs
git commit -m "feat: 在客户创建时添加结构化业务日志"
```

---

### Task 8: 启动验证和ES连通性测试

**Files:** 无新文件，运行时验证

- [ ] **Step 1: 启动后端API**

```bash
dotnet run --project src/TreadSnow.HttpApi.Host
```

Expected: 应用启动成功，控制台无ES连接错误。如果ES连接失败，Serilog sink会静默失败（BootstrapMethod.Silent），不影响主程序。

- [ ] **Step 2: 通过Swagger创建几条客户测试数据**

打开 https://localhost:44312/swagger，找到 `/api/app/account` POST接口，创建3条测试客户：

```json
{"name": "张三", "phone": "13800138001", "email": "zhangsan@test.com"}
{"name": "李四", "phone": "13800138002", "email": "lisi@test.com"}
{"name": "王五", "phone": "13800138003", "email": "wangwu@test.com"}
```

- [ ] **Step 3: 验证ES中收到日志数据**

```bash
curl -u elastic:xiaoxiaoxi11 "http://139.9.83.8:9200/logs-treadsnow-default/_search?pretty&size=5"
```

Expected: 返回JSON包含客户创建的日志文档，每条包含 `TenantId`、`TenantName`、`AccountId`、`Name`、`Phone` 字段。

- [ ] **Step 4: 如果Step 3没有数据，检查Data Stream是否创建**

```bash
curl -u elastic:xiaoxiaoxi11 "http://139.9.83.8:9200/_data_stream/logs-treadsnow-default?pretty"
```

如果不存在，检查ES版本兼容性和认证信息。查看应用控制台日志是否有Serilog相关错误。

---

### Task 9: 编写使用说明文档（含Kibana教学）

**Files:**
- Create: `docs/elasticsearch-logging-guide.md`

- [ ] **Step 1: 创建完整的使用说明文档**

文档结构：

```markdown
# TreadSnow.Elasticsearch.Logging 使用说明

## 一、快速接入（3步）
### 1. 添加项目引用
### 2. 添加模块依赖
### 3. 配置appsettings.json + Program.cs

## 二、配置参数详解
（ElasticsearchLoggingOptions所有属性说明表格）

## 三、业务日志最佳实践
### 结构化日志写法
### 日志级别选择建议
### 多租户日志自动注入说明

## 四、Kibana查询完整教学

### 4.1 创建Data View
1. 打开 Kibana（http://139.9.83.8:5601）
2. 左侧菜单 → Stack Management → Data Views
3. 点击 Create data view
4. Name: treadsnow-logs
5. Index pattern: logs-treadsnow-default
6. Timestamp field: @timestamp
7. 点击 Save data view to Kibana

### 4.2 Discover页面基础查询
1. 左侧菜单 → Discover
2. 选择刚创建的 treadsnow-logs Data View
3. 时间范围选择 Last 24 hours

常用KQL查询语法：
| 需求 | KQL |
|------|-----|
| 查某租户日志 | labels.TenantId: "租户guid" |
| 查客户创建 | message: "客户创建成功" |
| 查某手机号 | labels.Phone: "13800138000" |
| 查错误日志 | log.level: "Error" |
| 组合查询 | log.level: "Error" and labels.TenantId: "xxx" |

### 4.3 创建Dashboard（5个面板）

#### 面板1：日志趋势折线图
1. Dashboard → Create dashboard → Create visualization
2. 类型选择 Line
3. Horizontal axis: @timestamp (Date Histogram, Auto interval)
4. Vertical axis: Count
5. Break down by: log.level (Top 5)
6. 保存为 "日志趋势"

#### 面板2：错误TOP10柱状图
1. Create visualization → Bar horizontal
2. Vertical axis: labels.SourceContext.keyword (Top 10)
3. Horizontal axis: Count
4. Filter: log.level: "Error"
5. 保存为 "错误来源TOP10"

#### 面板3：租户日志分布饼图
1. Create visualization → Pie
2. Slice by: labels.TenantName.keyword (Top 10)
3. Size by: Count
4. 保存为 "租户日志分布"

#### 面板4：最近错误列表
1. Create visualization → Table
2. Columns: @timestamp, log.level, message, labels.TenantName
3. Filter: log.level: "Error"
4. Sort: @timestamp desc
5. 保存为 "最近错误列表"

#### 面板5：今日业务操作统计
1. Create visualization → Metric
2. Metric: Count
3. Filter: message: "客户创建成功"
4. 保存为 "今日客户创建数"

### 4.4 保存Dashboard
1. 点击 Save
2. Title: TreadSnow运维监控面板
3. Description: 日志趋势、错误监控、租户分布、业务统计

## 五、故障排查
### ES连接失败不影响主程序
### 如何查看Sink内部错误
### 日志延迟（BatchLifeTime配置）
```

- [ ] **Step 2: 填写完整内容并保存**

根据上述结构编写完整文档内容，确保每个Kibana操作步骤都具体到"点哪里、填什么"。

- [ ] **Step 3: Commit**

```bash
git add docs/elasticsearch-logging-guide.md
git commit -m "docs: 添加Elasticsearch日志使用说明和Kibana教学"
```

---

## 自检结果

1. **Spec覆盖**: 所有设计文档要求均有对应Task — 类库结构(T1-T5)、集成(T6)、业务日志(T7)、验证(T8)、使用说明+Kibana教学(T9)
2. **Placeholder扫描**: 无TBD/TODO，所有代码步骤包含完整代码
3. **类型一致性**: `ElasticsearchLoggingOptions` 属性名、`TenantEnricher` 类名、`WriteToElasticsearch` 扩展方法名在所有Task中一致
4. **设计变更**: 已在计划头部说明从 `Serilog.Sinks.Elasticsearch` 改为 `Elastic.Serilog.Sinks` 的原因（ES 9.x兼容性）

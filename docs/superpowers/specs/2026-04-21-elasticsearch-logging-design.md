# TreadSnow.Elasticsearch.Logging 设计文档

> 日期：2026-04-21
> 状态：已批准
> 方案：方案C — Serilog官方Sink + 自定义Enricher + ABP Module

## 1. 概述

创建独立类库 `TreadSnow.Elasticsearch.Logging`，基于 Serilog.Sinks.Elasticsearch 实现结构化日志异步批量写入 Elasticsearch，支持ABP多租户按月索引隔离，可在任何ABP项目中一行 `DependsOn` 启用。

## 2. 类库结构

```
TreadSnow.Elasticsearch.Logging/
├── TreadSnowElasticsearchLoggingModule.cs   # ABP模块
├── ElasticsearchLoggingOptions.cs            # 配置选项
├── Enrichers/
│   └── TenantEnricher.cs                     # 多租户上下文注入
├── IndexDeciders/
│   └── TenantMonthlyIndexDecider.cs          # 租户+月份动态索引
├── Extensions/
│   └── SerilogElasticsearchExtensions.cs     # LoggerConfiguration扩展方法
└── TreadSnow.Elasticsearch.Logging.csproj
```

## 3. 核心依赖

| 包 | 用途 |
|----|------|
| Serilog.Sinks.Elasticsearch | 批量异步写入ES核心Sink |
| Volo.Abp.MultiTenancy.Abstractions | ICurrentTenant接口 |
| Volo.Abp.Core | ABP模块基类 |

不引用TreadSnow业务代码，保持独立可复用。

## 4. 配置模型

`ElasticsearchLoggingOptions`：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Urls | string[] | 必填 | ES节点地址列表 |
| Username | string | 可选 | Basic Auth用户名 |
| Password | string | 可选 | Basic Auth密码 |
| IndexPrefix | string | "app-logs" | 索引前缀 |
| BatchPostingLimit | int | 50 | 每批最大条数 |
| Period | TimeSpan | 5秒 | 批量刷新间隔 |
| QueueSizeLimit | int | 100000 | 内存队列上限（背压控制） |
| MinimumLevel | LogEventLevel | Information | 写入ES的最低级别 |

配置来源：`appsettings.json` 的 `ElasticsearchLogging` 节点。

## 5. 异步批量写入机制

利用 Serilog.Sinks.Elasticsearch 内置的 PeriodicBatchingSink：

1. 日志事件进入 `Channel<T>` 无锁队列（主线程，零阻塞）
2. 后台线程按 Period 间隔从队列消费
3. 每批通过 ES `_bulk` API 一次性写入
4. QueueSizeLimit 控制背压，超限丢弃最旧日志（不阻塞主线程）
5. 网络异常时自动重试

## 6. 多租户索引路由

### TenantEnricher

- 在日志产生时（主线程）从 `ICurrentTenant` 捕获 TenantId 和 TenantName
- 写入 LogEvent.Properties，确保后台线程消费时仍能访问
- 通过 `IServiceProvider` 解析 Scoped 的 `ICurrentTenant`

### IndexDecider 索引命名规则

| 场景 | 索引名 | 说明 |
|------|--------|------|
| 有租户 | `{prefix}-{tenantId}-yyyy-MM` | 租户数据物理隔离 |
| 宿主(Host) | `{prefix}-host-yyyy-MM` | 宿主端日志 |
| 无租户上下文 | `{prefix}-system-yyyy-MM` | 后台任务等 |

## 7. ABP模块集成

### 模块注册

```csharp
[DependsOn(typeof(TreadSnowElasticsearchLoggingModule))]
public class YourHostModule : AbpModule { }
```

### appsettings.json 配置

```json
{
  "ElasticsearchLogging": {
    "Urls": ["http://139.9.83.8:9200"],
    "Username": "elastic",
    "Password": "xiaoxiaoxi11",
    "IndexPrefix": "treadsnow-logs",
    "BatchPostingLimit": 50,
    "Period": "00:00:05",
    "QueueSizeLimit": 100000,
    "MinimumLevel": "Information"
  }
}
```

### Program.cs 集成

```csharp
.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Warning()
        .Enrich.FromLogContext()
        .WriteTo.Async(c => c.File("Logs/logs.txt"))
        .WriteTo.Async(c => c.Console())
        .WriteToElasticsearch(context.Configuration, services);
});
```

设计理由：不自动劫持Serilog pipeline，用户显式调用扩展方法，最小惊讶原则。

## 8. Account创建日志测试

在 `AccountAppService.CreateAsync` 中添加结构化业务日志：

```csharp
Logger.LogInformation("客户创建成功 AccountId:{AccountId} Name:{Name} Phone:{Phone}",
    account.Id, account.Name, account.Phone);
```

写入ES后文档结构：

```json
{
  "@timestamp": "2026-04-21T10:30:00.000Z",
  "level": "Information",
  "messageTemplate": "客户创建成功 AccountId:{AccountId} Name:{Name} Phone:{Phone}",
  "fields": {
    "AccountId": "guid-xxx",
    "Name": "张三",
    "Phone": "13800138000",
    "TenantId": "tenant-guid",
    "TenantName": "某公司",
    "SourceContext": "TreadSnow.Accounts.AccountAppService"
  }
}
```

## 9. Kibana查询教学（写入使用说明）

### 创建 Data View
1. Stack Management → Data Views → Create data view
2. Index pattern: `treadsnow-logs-*`
3. Timestamp field: `@timestamp`

### KQL常用查询

| 需求 | KQL |
|------|-----|
| 某租户日志 | `fields.TenantId: "guid"` |
| 客户创建 | `message: "客户创建成功"` |
| 某手机号 | `fields.Phone: "13800138000"` |
| 错误日志 | `level: "Error"` |
| 组合查询 | `level: "Error" and fields.SourceContext: "*AccountAppService*"` |

### Dashboard面板

| 面板 | 类型 | 说明 |
|------|------|------|
| 日志趋势 | Line Chart | 按时间聚合日志量，分级别着色 |
| 错误TOP10 | Horizontal Bar | 按SourceContext聚合Error数量 |
| 租户日志分布 | Pie Chart | 各租户日志占比 |
| 最近错误列表 | Data Table | 最近50条Error日志详情 |
| 业务操作统计 | Metric | 今日客户创建数等 |

使用说明文档包含每个面板的完整创建步骤。

## 10. ES环境信息

- 地址：http://139.9.83.8:9200
- 账号：elastic
- 密码：xiaoxiaoxi11
- 版本：9.3.3
- Kibana：已部署

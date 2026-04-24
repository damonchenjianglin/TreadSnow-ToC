using Serilog.Events;
using System;

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

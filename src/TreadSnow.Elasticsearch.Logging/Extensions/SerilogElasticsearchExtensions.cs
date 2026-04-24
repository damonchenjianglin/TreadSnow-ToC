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

            Action<TransportConfigurationDescriptor>? configureTransport = null;
            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
            {
                var username = options.Username;
                var password = options.Password;
                configureTransport = transport =>
                {
                    transport.Authentication(new BasicAuthentication(username, password));
                };
            }

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
            }, configureTransport);

            return loggerConfiguration;
        }
    }
}

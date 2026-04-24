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

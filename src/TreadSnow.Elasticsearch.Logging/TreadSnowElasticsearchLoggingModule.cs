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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreadSnow.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Opportunities
{
    /// <summary>
    /// 商机应用服务接口
    /// </summary>
    public interface IOpportunityAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条商机
        /// </summary>
        /// <param name="id">商机Id</param>
        /// <returns>商机DTO</returns>
        Task<OpportunityDto> GetAsync(Guid id);

        /// <summary>
        /// 获取商机分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<OpportunityDto>> GetListAsync(GetOpportunityListDto input);

        /// <summary>
        /// 获取所有商机列表（不分页，用于导出）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        Task<List<OpportunityDto>> GetExportListAsync(string? name);

        /// <summary>
        /// 创建商机
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的商机DTO</returns>
        Task<OpportunityDto> CreateAsync(CreateOpportunityDto input);

        /// <summary>
        /// 更新商机
        /// </summary>
        /// <param name="id">商机Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的商机DTO</returns>
        Task<OpportunityDto> UpdateAsync(Guid id, UpdateOpportunityDto input);

        /// <summary>
        /// 删除商机
        /// </summary>
        /// <param name="id">商机Id</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 获取客户下拉列表（用于商机表单选择客户）
        /// </summary>
        /// <returns>客户Id和名称列表</returns>
        Task<ListResultDto<AccountLookupDto>> GetAccountLookupAsync();

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

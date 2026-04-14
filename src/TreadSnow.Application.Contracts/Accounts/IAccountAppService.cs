using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 会员应用服务接口
    /// </summary>
    public interface IAccountAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条会员
        /// </summary>
        /// <param name="id">会员Id</param>
        /// <returns>会员DTO</returns>
        Task<AccountDto> GetAsync(Guid id);

        /// <summary>
        /// 获取会员分页列表（支持name模糊筛选）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<AccountDto>> GetListAsync(GetAccountListDto input);

        /// <summary>
        /// 获取所有会员列表（不分页，用于导出）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        Task<List<AccountDto>> GetExportListAsync(string? name);

        /// <summary>
        /// 创建会员
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的会员DTO</returns>
        Task<AccountDto> CreateAsync(CreateAccountDto input);

        /// <summary>
        /// 更新会员
        /// </summary>
        /// <param name="id">会员Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的会员DTO</returns>
        Task<AccountDto> UpdateAsync(Guid id, UpdateAccountDto input);

        /// <summary>
        /// 删除会员
        /// </summary>
        /// <param name="id">会员Id</param>
        Task DeleteAsync(Guid id);
    }
}

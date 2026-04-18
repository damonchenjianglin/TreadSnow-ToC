using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreadSnow.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物应用服务接口
    /// </summary>
    public interface IPetAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条宠物
        /// </summary>
        /// <param name="id">宠物Id</param>
        /// <returns>宠物DTO</returns>
        Task<PetDto> GetAsync(Guid id);

        /// <summary>
        /// 获取宠物分页列表（支持name模糊筛选）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<PetDto>> GetListAsync(GetPetListDto input);

        /// <summary>
        /// 获取所有宠物列表（不分页，用于导出）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        Task<List<PetDto>> GetExportListAsync(string? name);

        /// <summary>
        /// 创建宠物
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的宠物DTO</returns>
        Task<PetDto> CreateAsync(CreatePetDto input);

        /// <summary>
        /// 更新宠物
        /// </summary>
        /// <param name="id">宠物Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的宠物DTO</returns>
        Task<PetDto> UpdateAsync(Guid id, UpdatePetDto input);

        /// <summary>
        /// 删除宠物
        /// </summary>
        /// <param name="id">宠物Id</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 获取会员下拉列表（用于宠物表单选择主人）
        /// </summary>
        /// <returns>会员Id和名称列表</returns>
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

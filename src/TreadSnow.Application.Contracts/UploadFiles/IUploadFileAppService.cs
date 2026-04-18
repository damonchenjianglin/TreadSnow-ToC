using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreadSnow.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.UploadFiles
{
    /// <summary>
    /// 附件应用服务接口
    /// </summary>
    public interface IUploadFileAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条附件
        /// </summary>
        /// <param name="id">附件Id</param>
        /// <returns>附件DTO</returns>
        Task<UploadFileDto> GetAsync(Guid id);

        /// <summary>
        /// 获取附件分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<UploadFileDto>> GetListAsync(GetUploadFileListDto input);

        /// <summary>
        /// 根据实体获取附件列表
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="recordId">记录Id</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<UploadFileDto>> GetListByEntityAsync(string entityName, string recordId);

        /// <summary>
        /// 创建附件
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的附件DTO</returns>
        Task<UploadFileDto> CreateAsync(CreateUploadFileDto input);

        /// <summary>
        /// 更新附件
        /// </summary>
        /// <param name="id">附件Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的附件DTO</returns>
        Task<UploadFileDto> UpdateAsync(Guid id, UpdateUploadFileDto input);

        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="id">附件Id</param>
        Task DeleteAsync(Guid id);

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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队应用服务接口
    /// </summary>
    public interface ITeamAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条团队
        /// </summary>
        /// <param name="id">团队Id</param>
        /// <returns>团队DTO</returns>
        Task<TeamDto> GetAsync(Guid id);

        /// <summary>
        /// 获取团队分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<TeamDto>> GetListAsync(GetTeamListDto input);

        /// <summary>
        /// 创建团队
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的团队DTO</returns>
        Task<TeamDto> CreateAsync(CreateTeamDto input);

        /// <summary>
        /// 更新团队
        /// </summary>
        /// <param name="id">团队Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的团队DTO</returns>
        Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto input);

        /// <summary>
        /// 删除团队
        /// </summary>
        /// <param name="id">团队Id</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 获取团队用户列表
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <returns>团队用户列表</returns>
        Task<List<TeamUserDto>> GetUsersAsync(Guid teamId);

        /// <summary>
        /// 添加团队用户
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="userId">用户Id</param>
        Task AddUserAsync(Guid teamId, Guid userId);

        /// <summary>
        /// 移除团队用户
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="userId">用户Id</param>
        Task RemoveUserAsync(Guid teamId, Guid userId);

        /// <summary>
        /// 获取团队角色列表
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <returns>团队角色列表</returns>
        Task<List<TeamRoleDto>> GetRolesAsync(Guid teamId);

        /// <summary>
        /// 添加团队角色
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="roleId">角色Id</param>
        Task AddRoleAsync(Guid teamId, Guid roleId);

        /// <summary>
        /// 移除团队角色
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="roleId">角色Id</param>
        Task RemoveRoleAsync(Guid teamId, Guid roleId);
    }
}

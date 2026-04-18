using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 角色数据权限应用服务接口
    /// </summary>
    public interface IRoleDataPermissionAppService : IApplicationService
    {
        /// <summary>
        /// 获取角色的数据权限配置
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns>角色数据权限配置</returns>
        Task<RoleDataPermissionDto> GetAsync(Guid roleId);

        /// <summary>
        /// 更新角色的数据权限配置
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="configs">权限配置列表</param>
        Task UpdateAsync(Guid roleId, List<DataPermissionConfigDto> configs);

        /// <summary>
        /// 获取当前登录用户的有效数据权限（多角色合并取最大值）
        /// </summary>
        /// <returns>用户有效权限</returns>
        Task<UserEffectivePermissionDto> GetCurrentUserPermissionsAsync();
    }
}

using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TreadSnow.Permissions;
using TreadSnow.Teams;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 角色数据权限应用服务
    /// </summary>
    [Authorize]
    public class RoleDataPermissionAppService : ApplicationService, IRoleDataPermissionAppService
    {
        /// <summary>
        /// 角色数据权限仓储
        /// </summary>
        private readonly IRepository<RoleDataPermission, Guid> _repository;

        /// <summary>
        /// 用户角色查询仓储
        /// </summary>
        private readonly IIdentityUserRepository _userRepository;

        /// <summary>
        /// 团队用户仓储
        /// </summary>
        private readonly IRepository<TeamUser> _teamUserRepository;

        /// <summary>
        /// 团队角色仓储
        /// </summary>
        private readonly IRepository<TeamRole> _teamRoleRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">角色数据权限仓储</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="teamUserRepository">团队用户仓储</param>
        /// <param name="teamRoleRepository">团队角色仓储</param>
        public RoleDataPermissionAppService(IRepository<RoleDataPermission, Guid> repository, IIdentityUserRepository userRepository, IRepository<TeamUser> teamUserRepository, IRepository<TeamRole> teamRoleRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _teamUserRepository = teamUserRepository;
            _teamRoleRepository = teamRoleRepository;
        }

        /// <summary>
        /// 获取角色的数据权限配置
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns>角色数据权限配置</returns>
        [Authorize(TreadSnowPermissions.DataPermissions.Default)]
        public async Task<RoleDataPermissionDto> GetAsync(Guid roleId)
        {
            var queryable = await _repository.GetQueryableAsync();
            var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.RoleId == roleId));

            var dto = new RoleDataPermissionDto { RoleId = roleId };
            if (entity != null && !string.IsNullOrWhiteSpace(entity.ConfigJson))
            {
                dto.Configs = JsonSerializer.Deserialize<List<DataPermissionConfigDto>>(entity.ConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<DataPermissionConfigDto>();
            }
            return dto;
        }

        /// <summary>
        /// 更新角色的数据权限配置
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="configs">权限配置列表</param>
        [Authorize(TreadSnowPermissions.DataPermissions.Manage)]
        public async Task UpdateAsync(Guid roleId, List<DataPermissionConfigDto> configs)
        {
            var queryable = await _repository.GetQueryableAsync();
            var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.RoleId == roleId));
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (entity == null)
            {
                await _repository.InsertAsync(new RoleDataPermission { TenantId = CurrentTenant.Id, RoleId = roleId, ConfigJson = json });
            }
            else
            {
                entity.ConfigJson = json;
                await _repository.UpdateAsync(entity);
            }
        }

        /// <summary>
        /// 获取当前登录用户的有效数据权限（多角色合并取最大值）
        /// </summary>
        /// <returns>用户有效权限</returns>
        public async Task<UserEffectivePermissionDto> GetCurrentUserPermissionsAsync()
        {
            var userId = CurrentUser.Id!.Value;
            var roleIds = await GetAllRoleIdsForUserAsync(userId);

            var allConfigs = new List<DataPermissionConfigDto>();
            foreach (var roleId in roleIds)
            {
                var queryable = await _repository.GetQueryableAsync();
                var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.RoleId == roleId));
                if (entity != null && !string.IsNullOrWhiteSpace(entity.ConfigJson))
                {
                    var configs = JsonSerializer.Deserialize<List<DataPermissionConfigDto>>(entity.ConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (configs != null) allConfigs.AddRange(configs);
                }
            }

            var merged = allConfigs
                .GroupBy(x => x.EntityName)
                .Select(g => new DataPermissionConfigDto
                {
                    EntityName = g.Key,
                    ReadLevel = g.Max(x => x.ReadLevel),
                    WriteLevel = g.Max(x => x.WriteLevel),
                    DeleteLevel = g.Max(x => x.DeleteLevel)
                })
                .ToList();

            return new UserEffectivePermissionDto { Configs = merged };
        }

        /// <summary>
        /// 获取用户所有角色Id（直接角色 + 团队角色）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>角色Id列表</returns>
        private async Task<List<Guid>> GetAllRoleIdsForUserAsync(Guid userId)
        {
            var roleIds = new HashSet<Guid>();

            var user = await _userRepository.GetAsync(userId);
            foreach (var role in user.Roles)
            {
                roleIds.Add(role.RoleId);
            }

            var teamUserQueryable = await _teamUserRepository.GetQueryableAsync();
            var userTeams = await AsyncExecuter.ToListAsync(teamUserQueryable.Where(x => x.UserId == userId));

            if (userTeams.Any())
            {
                var teamIds = userTeams.Select(x => x.TeamId).ToList();
                var teamRoleQueryable = await _teamRoleRepository.GetQueryableAsync();
                var teamRoles = await AsyncExecuter.ToListAsync(teamRoleQueryable.Where(x => teamIds.Contains(x.TeamId)));
                foreach (var tr in teamRoles)
                {
                    roleIds.Add(tr.RoleId);
                }
            }

            return roleIds.ToList();
        }
    }
}

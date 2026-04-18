using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Departments;
using TreadSnow.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.Teams.Default)]
    public class TeamAppService : ApplicationService, ITeamAppService
    {
        /// <summary>
        /// 团队仓储
        /// </summary>
        private readonly IRepository<Team, Guid> _repository;

        /// <summary>
        /// 团队用户仓储
        /// </summary>
        private readonly IRepository<TeamUser> _teamUserRepository;

        /// <summary>
        /// 团队角色仓储
        /// </summary>
        private readonly IRepository<TeamRole> _teamRoleRepository;

        /// <summary>
        /// 部门仓储
        /// </summary>
        private readonly IRepository<Department, Guid> _departmentRepository;

        /// <summary>
        /// 用户仓储
        /// </summary>
        private readonly IIdentityUserRepository _userRepository;

        /// <summary>
        /// 角色仓储
        /// </summary>
        private readonly IIdentityRoleRepository _roleRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">团队仓储</param>
        /// <param name="teamUserRepository">团队用户仓储</param>
        /// <param name="teamRoleRepository">团队角色仓储</param>
        /// <param name="departmentRepository">部门仓储</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="roleRepository">角色仓储</param>
        public TeamAppService(IRepository<Team, Guid> repository, IRepository<TeamUser> teamUserRepository, IRepository<TeamRole> teamRoleRepository, IRepository<Department, Guid> departmentRepository, IIdentityUserRepository userRepository, IIdentityRoleRepository roleRepository)
        {
            _repository = repository;
            _teamUserRepository = teamUserRepository;
            _teamRoleRepository = teamRoleRepository;
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// 获取单条团队
        /// </summary>
        /// <param name="id">团队Id</param>
        /// <returns>团队DTO</returns>
        public async Task<TeamDto> GetAsync(Guid id)
        {
            var team = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<Team, TeamDto>(team);
            if (team.DepartmentId.HasValue)
            {
                var dept = await _departmentRepository.FindAsync(team.DepartmentId.Value);
                dto.DepartmentName = dept?.Name;
            }
            return dto;
        }

        /// <summary>
        /// 获取团队分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<TeamDto>> GetListAsync(GetTeamListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                query = query.Where(x => x.Name.Contains(input.Name));
            }

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.OrderBy(x => x.No).Skip(input.SkipCount).Take(input.MaxResultCount);
            var teams = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<TeamDto>(totalCount, ObjectMapper.Map<List<Team>, List<TeamDto>>(teams));
        }

        /// <summary>
        /// 创建团队
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的团队DTO</returns>
        [Authorize(TreadSnowPermissions.Teams.Create)]
        public async Task<TeamDto> CreateAsync(CreateTeamDto input)
        {
            var team = ObjectMapper.Map<CreateTeamDto, Team>(input);
            team.TenantId = CurrentTenant.Id;
            await _repository.InsertAsync(team);
            return ObjectMapper.Map<Team, TeamDto>(team);
        }

        /// <summary>
        /// 更新团队
        /// </summary>
        /// <param name="id">团队Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的团队DTO</returns>
        [Authorize(TreadSnowPermissions.Teams.Edit)]
        public async Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto input)
        {
            var team = await _repository.GetAsync(id);
            ObjectMapper.Map(input, team);
            await _repository.UpdateAsync(team);
            return ObjectMapper.Map<Team, TeamDto>(team);
        }

        /// <summary>
        /// 删除团队
        /// </summary>
        /// <param name="id">团队Id</param>
        [Authorize(TreadSnowPermissions.Teams.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// 获取团队用户列表
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <returns>团队用户列表</returns>
        public async Task<List<TeamUserDto>> GetUsersAsync(Guid teamId)
        {
            var queryable = await _teamUserRepository.GetQueryableAsync();
            var teamUsers = await AsyncExecuter.ToListAsync(queryable.Where(x => x.TeamId == teamId));
            var result = new List<TeamUserDto>();
            foreach (var tu in teamUsers)
            {
                var user = await _userRepository.FindAsync(tu.UserId);
                result.Add(new TeamUserDto { UserId = tu.UserId, UserName = user?.UserName });
            }
            return result;
        }

        /// <summary>
        /// 添加团队用户
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="userId">用户Id</param>
        [Authorize(TreadSnowPermissions.Teams.Edit)]
        public async Task AddUserAsync(Guid teamId, Guid userId)
        {
            var exists = await _teamUserRepository.AnyAsync(x => x.TeamId == teamId && x.UserId == userId);
            if (!exists)
            {
                await _teamUserRepository.InsertAsync(new TeamUser { TenantId = CurrentTenant.Id, TeamId = teamId, UserId = userId });
            }
        }

        /// <summary>
        /// 移除团队用户
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="userId">用户Id</param>
        [Authorize(TreadSnowPermissions.Teams.Edit)]
        public async Task RemoveUserAsync(Guid teamId, Guid userId)
        {
            await _teamUserRepository.DeleteAsync(x => x.TeamId == teamId && x.UserId == userId);
        }

        /// <summary>
        /// 获取团队角色列表
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <returns>团队角色列表</returns>
        public async Task<List<TeamRoleDto>> GetRolesAsync(Guid teamId)
        {
            var queryable = await _teamRoleRepository.GetQueryableAsync();
            var teamRoles = await AsyncExecuter.ToListAsync(queryable.Where(x => x.TeamId == teamId));
            var result = new List<TeamRoleDto>();
            foreach (var tr in teamRoles)
            {
                var role = await _roleRepository.FindAsync(tr.RoleId);
                result.Add(new TeamRoleDto { RoleId = tr.RoleId, RoleName = role?.Name });
            }
            return result;
        }

        /// <summary>
        /// 添加团队角色
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="roleId">角色Id</param>
        [Authorize(TreadSnowPermissions.Teams.Edit)]
        public async Task AddRoleAsync(Guid teamId, Guid roleId)
        {
            var exists = await _teamRoleRepository.AnyAsync(x => x.TeamId == teamId && x.RoleId == roleId);
            if (!exists)
            {
                await _teamRoleRepository.InsertAsync(new TeamRole { TenantId = CurrentTenant.Id, TeamId = teamId, RoleId = roleId });
            }
        }

        /// <summary>
        /// 移除团队角色
        /// </summary>
        /// <param name="teamId">团队Id</param>
        /// <param name="roleId">角色Id</param>
        [Authorize(TreadSnowPermissions.Teams.Edit)]
        public async Task RemoveRoleAsync(Guid teamId, Guid roleId)
        {
            await _teamRoleRepository.DeleteAsync(x => x.TeamId == teamId && x.RoleId == roleId);
        }
    }
}

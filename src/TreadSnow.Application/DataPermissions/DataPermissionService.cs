using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TreadSnow.Departments;
using TreadSnow.Teams;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 数据权限过滤服务（提供给各AppService调用的内部服务）
    /// </summary>
    public class DataPermissionService : ITransientDependency
    {
        /// <summary>
        /// 当前用户
        /// </summary>
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// 用户仓储
        /// </summary>
        private readonly IIdentityUserRepository _userRepository;

        /// <summary>
        /// 用户查询仓储
        /// </summary>
        private readonly IRepository<IdentityUser, Guid> _userQueryRepository;

        /// <summary>
        /// 异步查询执行器
        /// </summary>
        private readonly IAsyncQueryableExecuter _asyncExecuter;

        /// <summary>
        /// 角色数据权限仓储
        /// </summary>
        private readonly IRepository<RoleDataPermission, Guid> _roleDataPermissionRepository;

        /// <summary>
        /// 团队用户仓储
        /// </summary>
        private readonly IRepository<TeamUser> _teamUserRepository;

        /// <summary>
        /// 团队角色仓储
        /// </summary>
        private readonly IRepository<TeamRole> _teamRoleRepository;

        /// <summary>
        /// 团队仓储
        /// </summary>
        private readonly IRepository<Team, Guid> _teamRepository;

        /// <summary>
        /// 部门仓储
        /// </summary>
        private readonly IRepository<Department, Guid> _departmentRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="currentUser">当前用户</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="userQueryRepository">用户查询仓储</param>
        /// <param name="asyncExecuter">异步查询执行器</param>
        /// <param name="roleDataPermissionRepository">角色数据权限仓储</param>
        /// <param name="teamUserRepository">团队用户仓储</param>
        /// <param name="teamRoleRepository">团队角色仓储</param>
        /// <param name="teamRepository">团队仓储</param>
        /// <param name="departmentRepository">部门仓储</param>
        public DataPermissionService(ICurrentUser currentUser, IIdentityUserRepository userRepository, IRepository<IdentityUser, Guid> userQueryRepository, IAsyncQueryableExecuter asyncExecuter, IRepository<RoleDataPermission, Guid> roleDataPermissionRepository, IRepository<TeamUser> teamUserRepository, IRepository<TeamRole> teamRoleRepository, IRepository<Team, Guid> teamRepository, IRepository<Department, Guid> departmentRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _userQueryRepository = userQueryRepository;
            _asyncExecuter = asyncExecuter;
            _roleDataPermissionRepository = roleDataPermissionRepository;
            _teamUserRepository = teamUserRepository;
            _teamRoleRepository = teamRoleRepository;
            _teamRepository = teamRepository;
            _departmentRepository = departmentRepository;
        }

        /// <summary>
        /// 获取当前用户对指定实体的有效权限等级（读/写/删除）
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <returns>读/写/删除权限等级元组</returns>
        public async Task<(int ReadLevel, int WriteLevel, int DeleteLevel)> GetEffectivePermissionAsync(string entityName)
        {
            if (!_currentUser.IsAuthenticated) return (0, 0, 0);

            var roleIds = await GetAllRoleIdsAsync(_currentUser.Id!.Value);
            var queryable = await _roleDataPermissionRepository.GetQueryableAsync();
            var permissions = await _asyncExecuter.ToListAsync(queryable.Where(x => roleIds.Contains(x.RoleId)));

            int maxRead = 0, maxWrite = 0, maxDelete = 0;
            foreach (var perm in permissions)
            {
                if (string.IsNullOrWhiteSpace(perm.ConfigJson)) continue;
                var configs = JsonSerializer.Deserialize<List<DataPermissionConfigDto>>(perm.ConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (configs == null) continue;
                var config = configs.FirstOrDefault(x => string.Equals(x.EntityName, entityName, StringComparison.OrdinalIgnoreCase));
                if (config == null) continue;
                maxRead = Math.Max(maxRead, config.ReadLevel);
                maxWrite = Math.Max(maxWrite, config.WriteLevel);
                maxDelete = Math.Max(maxDelete, config.DeleteLevel);
            }

            return (maxRead, maxWrite, maxDelete);
        }

        /// <summary>
        /// 根据读权限等级过滤IQueryable（泛型方法，要求实体有OwnerId和OwnerTeamId属性）
        /// </summary>
        /// <param name="query">原始查询</param>
        /// <param name="entityName">实体名称</param>
        /// <param name="ownerIdSelector">获取OwnerId的表达式</param>
        /// <param name="ownerTeamIdSelector">获取OwnerTeamId的表达式</param>
        /// <returns>过滤后的查询，如果权限为0则返回null表示无权限</returns>
        public async Task<IQueryable<T>> ApplyReadFilterAsync<T>(IQueryable<T> query, string entityName, System.Linq.Expressions.Expression<Func<T, Guid?>> ownerIdSelector, System.Linq.Expressions.Expression<Func<T, Guid?>> ownerTeamIdSelector)
        {
            var (readLevel, _, _) = await GetEffectivePermissionAsync(entityName);

            if (readLevel >= 4) return query;
            if (readLevel <= 0) return query.Where(x => false);

            var userId = _currentUser.Id!.Value;
            var ownerIdFunc = ownerIdSelector.Compile();
            var ownerTeamIdFunc = ownerTeamIdSelector.Compile();

            if (readLevel == 1)
            {
                var myTeamIds = await GetUserTeamIdsAsync(userId);
                var items = query.AsEnumerable().Where(x => ownerIdFunc(x) == userId || (ownerTeamIdFunc(x).HasValue && myTeamIds.Contains(ownerTeamIdFunc(x)!.Value)));
                return items.AsQueryable();
            }

            if (readLevel == 2)
            {
                var deptUserIds = await GetDepartmentUserIdsAsync(userId, false);
                var deptTeamIds = await GetDepartmentTeamIdsAsync(userId, false);
                var items = query.AsEnumerable().Where(x => (ownerIdFunc(x).HasValue && deptUserIds.Contains(ownerIdFunc(x)!.Value)) || (ownerTeamIdFunc(x).HasValue && deptTeamIds.Contains(ownerTeamIdFunc(x)!.Value)));
                return items.AsQueryable();
            }

            if (readLevel == 3)
            {
                var deptUserIds = await GetDepartmentUserIdsAsync(userId, true);
                var deptTeamIds = await GetDepartmentTeamIdsAsync(userId, true);
                var items = query.AsEnumerable().Where(x => (ownerIdFunc(x).HasValue && deptUserIds.Contains(ownerIdFunc(x)!.Value)) || (ownerTeamIdFunc(x).HasValue && deptTeamIds.Contains(ownerTeamIdFunc(x)!.Value)));
                return items.AsQueryable();
            }

            return query;
        }

        /// <summary>
        /// 校验当前用户对指定记录的写权限
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="ownerId">记录负责人Id</param>
        /// <param name="ownerTeamId">记录负责团队Id</param>
        /// <returns>是否有写权限</returns>
        public async Task<bool> CheckWritePermissionAsync(string entityName, Guid? ownerId, Guid? ownerTeamId)
        {
            return await CheckPermissionByLevelAsync(entityName, ownerId, ownerTeamId, "write");
        }

        /// <summary>
        /// 校验当前用户对指定记录的删除权限
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="ownerId">记录负责人Id</param>
        /// <param name="ownerTeamId">记录负责团队Id</param>
        /// <returns>是否有删除权限</returns>
        public async Task<bool> CheckDeletePermissionAsync(string entityName, Guid? ownerId, Guid? ownerTeamId)
        {
            return await CheckPermissionByLevelAsync(entityName, ownerId, ownerTeamId, "delete");
        }

        /// <summary>
        /// 按等级校验权限
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="ownerId">记录负责人Id</param>
        /// <param name="ownerTeamId">记录负责团队Id</param>
        /// <param name="type">权限类型（write/delete）</param>
        /// <returns>是否有权限</returns>
        private async Task<bool> CheckPermissionByLevelAsync(string entityName, Guid? ownerId, Guid? ownerTeamId, string type)
        {
            if (!_currentUser.IsAuthenticated) return false;
            var (_, writeLevel, deleteLevel) = await GetEffectivePermissionAsync(entityName);
            var level = type == "delete" ? deleteLevel : writeLevel;

            if (level >= 4) return true;
            if (level <= 0) return false;

            var userId = _currentUser.Id!.Value;

            if (level == 1)
            {
                if (ownerId == userId) return true;
                if (ownerTeamId.HasValue)
                {
                    var myTeamIds = await GetUserTeamIdsAsync(userId);
                    return myTeamIds.Contains(ownerTeamId.Value);
                }
                return false;
            }

            if (level == 2)
            {
                var deptUserIds = await GetDepartmentUserIdsAsync(userId, false);
                var deptTeamIds = await GetDepartmentTeamIdsAsync(userId, false);
                return (ownerId.HasValue && deptUserIds.Contains(ownerId.Value)) || (ownerTeamId.HasValue && deptTeamIds.Contains(ownerTeamId.Value));
            }

            if (level == 3)
            {
                var deptUserIds = await GetDepartmentUserIdsAsync(userId, true);
                var deptTeamIds = await GetDepartmentTeamIdsAsync(userId, true);
                return (ownerId.HasValue && deptUserIds.Contains(ownerId.Value)) || (ownerTeamId.HasValue && deptTeamIds.Contains(ownerTeamId.Value));
            }

            return false;
        }

        /// <summary>
        /// 获取用户所有角色Id（直接角色+团队角色）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>角色Id列表</returns>
        private async Task<List<Guid>> GetAllRoleIdsAsync(Guid userId)
        {
            var roleIds = new HashSet<Guid>();
            var user = await _userRepository.GetAsync(userId);
            foreach (var role in user.Roles) roleIds.Add(role.RoleId);

            var teamUserQ = await _teamUserRepository.GetQueryableAsync();
            var userTeams = await _asyncExecuter.ToListAsync(teamUserQ.Where(x => x.UserId == userId));
            if (userTeams.Any())
            {
                var teamIds = userTeams.Select(x => x.TeamId).ToList();
                var teamRoleQ = await _teamRoleRepository.GetQueryableAsync();
                var teamRoles = await _asyncExecuter.ToListAsync(teamRoleQ.Where(x => teamIds.Contains(x.TeamId)));
                foreach (var tr in teamRoles) roleIds.Add(tr.RoleId);
            }

            return roleIds.ToList();
        }

        /// <summary>
        /// 获取用户所在的所有团队Id
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>团队Id列表</returns>
        private async Task<List<Guid>> GetUserTeamIdsAsync(Guid userId)
        {
            var q = await _teamUserRepository.GetQueryableAsync();
            var items = await _asyncExecuter.ToListAsync(q.Where(x => x.UserId == userId));
            return items.Select(x => x.TeamId).ToList();
        }

        /// <summary>
        /// 获取用户所在部门（及下级部门）的所有用户Id
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="includeChildren">是否包含下级部门</param>
        /// <returns>用户Id集合</returns>
        private async Task<HashSet<Guid>> GetDepartmentUserIdsAsync(Guid userId, bool includeChildren)
        {
            var user = await _userRepository.GetAsync(userId);
            var deptId = user.GetProperty<Guid?>("DepartmentId");
            if (!deptId.HasValue) return new HashSet<Guid> { userId };

            var deptIds = includeChildren ? await GetDepartmentAndChildrenIdsAsync(deptId.Value) : new HashSet<Guid> { deptId.Value };
            var userQ = await _userQueryRepository.GetQueryableAsync();
            var allUsers = await _asyncExecuter.ToListAsync(userQ);
            var result = new HashSet<Guid>();
            foreach (var u in allUsers)
            {
                var uDeptId = u.GetProperty<Guid?>("DepartmentId");
                if (uDeptId.HasValue && deptIds.Contains(uDeptId.Value)) result.Add(u.Id);
            }
            return result;
        }

        /// <summary>
        /// 获取用户所在部门（及下级部门）的所有团队Id
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="includeChildren">是否包含下级部门</param>
        /// <returns>团队Id集合</returns>
        private async Task<HashSet<Guid>> GetDepartmentTeamIdsAsync(Guid userId, bool includeChildren)
        {
            var user = await _userRepository.GetAsync(userId);
            var deptId = user.GetProperty<Guid?>("DepartmentId");
            if (!deptId.HasValue) return new HashSet<Guid>();

            var deptIds = includeChildren ? await GetDepartmentAndChildrenIdsAsync(deptId.Value) : new HashSet<Guid> { deptId.Value };
            var teamQ = await _teamRepository.GetQueryableAsync();
            var teams = await _asyncExecuter.ToListAsync(teamQ.Where(x => x.DepartmentId.HasValue && deptIds.Contains(x.DepartmentId.Value)));
            return teams.Select(x => x.Id).ToHashSet();
        }

        /// <summary>
        /// 递归获取指定部门及其所有下级部门的Id集合
        /// </summary>
        /// <param name="departmentId">部门Id</param>
        /// <returns>部门Id集合</returns>
        private async Task<HashSet<Guid>> GetDepartmentAndChildrenIdsAsync(Guid departmentId)
        {
            var result = new HashSet<Guid> { departmentId };
            var allDepts = await _departmentRepository.GetListAsync();
            CollectChildDepartmentIds(departmentId, allDepts, result);
            return result;
        }

        /// <summary>
        /// 递归收集下级部门Id
        /// </summary>
        /// <param name="parentId">父部门Id</param>
        /// <param name="allDepts">所有部门列表</param>
        /// <param name="result">结果集合</param>
        private void CollectChildDepartmentIds(Guid parentId, List<Department> allDepts, HashSet<Guid> result)
        {
            var children = allDepts.Where(x => x.ParentDepartmentId == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child.Id);
                CollectChildDepartmentIds(child.Id, allDepts, result);
            }
        }
    }
}

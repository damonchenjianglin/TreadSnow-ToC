using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Accounts;
using TreadSnow.DataPermissions;
using TreadSnow.Lookups;
using TreadSnow.Permissions;
using TreadSnow.Teams;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.Pets.Default)]
    public class PetAppService : ApplicationService, IPetAppService
    {
        /// <summary>
        /// 宠物仓储
        /// </summary>
        private readonly IRepository<Pet, Guid> _repository;

        /// <summary>
        /// 会员仓储
        /// </summary>
        private readonly IRepository<Account, Guid> _accountRepository;

        /// <summary>
        /// 异步查询执行器
        /// </summary>
        private readonly IAsyncQueryableExecuter _asyncExecuter;

        /// <summary>
        /// 数据权限过滤服务
        /// </summary>
        private readonly DataPermissionService _dataPermissionService;

        /// <summary>
        /// 用户仓储（用于查询负责人名称）
        /// </summary>
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        /// <summary>
        /// 团队仓储（用于查询负责团队名称）
        /// </summary>
        private readonly IRepository<Team, Guid> _teamRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">宠物仓储</param>
        /// <param name="accountRepository">会员仓储</param>
        /// <param name="asyncExecuter">异步查询执行器</param>
        /// <param name="dataPermissionService">数据权限过滤服务</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="teamRepository">团队仓储</param>
        public PetAppService(IRepository<Pet, Guid> repository, IRepository<Account, Guid> accountRepository, IAsyncQueryableExecuter asyncExecuter, DataPermissionService dataPermissionService, IRepository<IdentityUser, Guid> userRepository, IRepository<Team, Guid> teamRepository)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _asyncExecuter = asyncExecuter;
            _dataPermissionService = dataPermissionService;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// 获取单条宠物信息（关联查询会员名称）
        /// </summary>
        /// <param name="id">宠物Id</param>
        /// <returns>宠物DTO（含主人名称）</returns>
        public async Task<PetDto> GetAsync(Guid id)
        {
            var pet = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<Pet, PetDto>(pet);

            var account = await _accountRepository.FindAsync(pet.AccountId);
            dto.AccountName = account?.Name;
            var dtoList = new List<PetDto> { dto };
            await FillLookupNamesAsync(dtoList);
            await FillPermissionsAsync(dtoList);

            return dto;
        }

        /// <summary>
        /// 获取宠物分页列表（支持name模糊筛选 + 负责人筛选，关联查询会员名称）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果（含主人名称）</returns>
        public async Task<PagedResultDto<PetDto>> GetListAsync(GetPetListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                query = query.Where(x => x.Name.Contains(input.Name));
            }

            if (input.OwnerId.HasValue)
            {
                query = query.Where(x => x.OwnerId == input.OwnerId.Value);
            }

            if (input.StartCreationTime.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.StartCreationTime.Value);
            }

            if (input.EndCreationTime.HasValue)
            {
                query = query.Where(x => x.CreationTime < input.EndCreationTime.Value.AddDays(1));
            }

            if (input.StartLastModificationTime.HasValue)
            {
                query = query.Where(x => x.LastModificationTime >= input.StartLastModificationTime.Value);
            }

            if (input.EndLastModificationTime.HasValue)
            {
                query = query.Where(x => x.LastModificationTime < input.EndLastModificationTime.Value.AddDays(1));
            }

            query = await _dataPermissionService.ApplyReadFilterAsync(query, "pet", x => x.OwnerId, x => x.OwnerTeamId);

            var totalCount = await _asyncExecuter.CountAsync(query);

            query = query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount);
            var pets = await _asyncExecuter.ToListAsync(query);
            var dtos = ObjectMapper.Map<List<Pet>, List<PetDto>>(pets);

            var accountIds = pets.Select(p => p.AccountId).Distinct().ToList();
            var accountQueryable = await _accountRepository.GetQueryableAsync();
            var accounts = await _asyncExecuter.ToListAsync(accountQueryable.Where(a => accountIds.Contains(a.Id)));
            var accountDict = accounts.ToDictionary(a => a.Id, a => a.Name);

            foreach (var dto in dtos)
            {
                accountDict.TryGetValue(dto.AccountId, out var accountName);
                dto.AccountName = accountName;
            }

            await FillLookupNamesAsync(dtos);
            await FillPermissionsAsync(dtos);

            return new PagedResultDto<PetDto>(totalCount, dtos);
        }

        /// <summary>
        /// 获取所有宠物列表（不分页，用于导出，关联查询会员名称）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        public async Task<List<PetDto>> GetExportListAsync(string? name)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            query = await _dataPermissionService.ApplyReadFilterAsync(query, "pet", x => x.OwnerId, x => x.OwnerTeamId);

            query = query.OrderByDescending(x => x.CreationTime);
            var pets = await _asyncExecuter.ToListAsync(query);
            var dtos = ObjectMapper.Map<List<Pet>, List<PetDto>>(pets);

            var accountIds = pets.Select(p => p.AccountId).Distinct().ToList();
            var accountQueryable = await _accountRepository.GetQueryableAsync();
            var accounts = await _asyncExecuter.ToListAsync(accountQueryable.Where(a => accountIds.Contains(a.Id)));
            var accountDict = accounts.ToDictionary(a => a.Id, a => a.Name);

            foreach (var dto in dtos)
            {
                accountDict.TryGetValue(dto.AccountId, out var accountName);
                dto.AccountName = accountName;
            }

            await FillLookupNamesAsync(dtos);
            await FillPermissionsAsync(dtos);

            return dtos;
        }

        /// <summary>
        /// 获取会员下拉列表（用于宠物表单选择主人）
        /// </summary>
        /// <returns>会员Id和名称列表</returns>
        public async Task<ListResultDto<AccountLookupDto>> GetAccountLookupAsync()
        {
            var accounts = await _accountRepository.GetListAsync();
            return new ListResultDto<AccountLookupDto>(
                ObjectMapper.Map<List<Account>, List<AccountLookupDto>>(accounts)
            );
        }

        /// <summary>
        /// 获取用户下拉列表（用于选择负责人）
        /// </summary>
        /// <returns>用户Id和名称列表</returns>
        public async Task<ListResultDto<UserLookupDto>> GetOwnerLookupAsync()
        {
            var queryable = await _userRepository.GetQueryableAsync();
            var users = await _asyncExecuter.ToListAsync(queryable);
            var items = users.Select(u => new UserLookupDto { Id = u.Id, Name = u.UserName }).ToList();
            return new ListResultDto<UserLookupDto>(items);
        }

        /// <summary>
        /// 获取团队下拉列表（用于选择负责团队）
        /// </summary>
        /// <returns>团队Id和名称列表</returns>
        public async Task<ListResultDto<TeamLookupDto>> GetTeamLookupAsync()
        {
            var teams = await _teamRepository.GetListAsync();
            var items = teams.Select(t => new TeamLookupDto { Id = t.Id, Name = t.Name }).ToList();
            return new ListResultDto<TeamLookupDto>(items);
        }

        /// <summary>
        /// 创建宠物（OwnerId不传则默认当前用户）
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的宠物DTO</returns>
        [Authorize(TreadSnowPermissions.Pets.Create)]
        public async Task<PetDto> CreateAsync(CreatePetDto input)
        {
            var account = await _accountRepository.FindAsync(input.AccountId);
            if (account == null)
            {
                throw new UserFriendlyException("Account not found");
            }
            var pet = ObjectMapper.Map<CreatePetDto, Pet>(input);
            pet.TenantId = CurrentTenant.Id;
            pet.OwnerId = input.OwnerId ?? CurrentUser.Id;
            await _repository.InsertAsync(pet);
            return ObjectMapper.Map<Pet, PetDto>(pet);
        }

        /// <summary>
        /// 更新宠物
        /// </summary>
        /// <param name="id">宠物Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的宠物DTO</returns>
        [Authorize(TreadSnowPermissions.Pets.Edit)]
        public async Task<PetDto> UpdateAsync(Guid id, UpdatePetDto input)
        {
            var pet = await _repository.GetAsync(id);
            var hasPermission = await _dataPermissionService.CheckWritePermissionAsync("pet", pet.OwnerId, pet.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的编辑权限");
            ObjectMapper.Map(input, pet);
            await _repository.UpdateAsync(pet);
            return ObjectMapper.Map<Pet, PetDto>(pet);
        }

        /// <summary>
        /// 删除宠物（含删除权限校验）
        /// </summary>
        /// <param name="id">宠物Id</param>
        [Authorize(TreadSnowPermissions.Pets.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var pet = await _repository.GetAsync(id);
            var hasPermission = await _dataPermissionService.CheckDeletePermissionAsync("pet", pet.OwnerId, pet.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的删除权限");
            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// 批量填充每条记录的编辑/删除权限标识
        /// </summary>
        /// <param name="dtos">DTO列表</param>
        private async Task FillPermissionsAsync(List<PetDto> dtos)
        {
            var records = dtos.Select(d => (d.OwnerId, d.OwnerTeamId)).ToList();
            var permissions = await _dataPermissionService.BatchCheckPermissionsAsync("pet", records);
            for (var i = 0; i < dtos.Count; i++)
            {
                dtos[i].CanEdit = permissions[i].CanEdit;
                dtos[i].CanDelete = permissions[i].CanDelete;
            }
        }

        /// <summary>
        /// 批量填充负责人、负责团队、创建人、修改人名称
        /// </summary>
        /// <param name="dtos">DTO列表</param>
        private async Task FillLookupNamesAsync(List<PetDto> dtos)
        {
            var ownerIds = dtos.Where(d => d.OwnerId.HasValue).Select(d => d.OwnerId!.Value).Distinct().ToList();
            var creatorIds = dtos.Where(d => d.CreatorId.HasValue).Select(d => d.CreatorId!.Value).Distinct().ToList();
            var modifierIds = dtos.Where(d => d.LastModifierId.HasValue).Select(d => d.LastModifierId!.Value).Distinct().ToList();
            var allUserIds = ownerIds.Union(creatorIds).Union(modifierIds).Distinct().ToList();

            var teamIds = dtos.Where(d => d.OwnerTeamId.HasValue).Select(d => d.OwnerTeamId!.Value).Distinct().ToList();

            var userDict = new Dictionary<Guid, string>();
            var teamDict = new Dictionary<Guid, string>();

            if (allUserIds.Any())
            {
                var userQueryable = await _userRepository.GetQueryableAsync();
                var users = await _asyncExecuter.ToListAsync(userQueryable.Where(u => allUserIds.Contains(u.Id)));
                userDict = users.ToDictionary(u => u.Id, u => u.UserName);
            }

            if (teamIds.Any())
            {
                var teamQueryable = await _teamRepository.GetQueryableAsync();
                var teams = await _asyncExecuter.ToListAsync(teamQueryable.Where(t => teamIds.Contains(t.Id)));
                teamDict = teams.ToDictionary(t => t.Id, t => t.Name);
            }

            foreach (var dto in dtos)
            {
                if (dto.OwnerId.HasValue && userDict.TryGetValue(dto.OwnerId.Value, out var ownerName)) dto.OwnerName = ownerName;
                if (dto.OwnerTeamId.HasValue && teamDict.TryGetValue(dto.OwnerTeamId.Value, out var teamName)) dto.OwnerTeamName = teamName;
                if (dto.CreatorId.HasValue && userDict.TryGetValue(dto.CreatorId.Value, out var creatorName)) dto.CreatorName = creatorName;
                if (dto.LastModifierId.HasValue && userDict.TryGetValue(dto.LastModifierId.Value, out var modifierName)) dto.LastModifierName = modifierName;
            }
        }
    }
}

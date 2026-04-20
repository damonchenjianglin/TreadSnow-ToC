using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.DataPermissions;
using TreadSnow.Lookups;
using TreadSnow.Permissions;
using TreadSnow.Teams;
using TreadSnow.UploadFiles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace TreadSnow.UploadFiles
{
    /// <summary>
    /// 附件应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.UploadFiles.Default)]
    public class UploadFileAppService : ApplicationService, IUploadFileAppService
    {
        /// <summary>
        /// 附件仓储
        /// </summary>
        private readonly IRepository<UploadFile, Guid> _repository;

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
        /// <param name="repository">附件仓储</param>
        /// <param name="dataPermissionService">数据权限过滤服务</param>
        /// <param name="userRepository">用户仓储</param>
        /// <param name="teamRepository">团队仓储</param>
        public UploadFileAppService(IRepository<UploadFile, Guid> repository, DataPermissionService dataPermissionService, IRepository<IdentityUser, Guid> userRepository, IRepository<Team, Guid> teamRepository)
        {
            _repository = repository;
            _dataPermissionService = dataPermissionService;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// 获取单条
        /// </summary>
        /// <param name="id">附件Id</param>
        /// <returns>附件DTO</returns>
        public async Task<UploadFileDto> GetAsync(Guid id)
        {
            var uploadFile = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<UploadFile, UploadFileDto>(uploadFile);
            var dtoList = new List<UploadFileDto> { dto };
            await FillLookupNamesAsync(dtoList);
            await FillPermissionsAsync(dtoList);
            return dto;
        }

        /// <summary>
        /// 获取附件列表（EntityName和RecordId可选，不传则查所有，支持负责人筛选）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<UploadFileDto>> GetListAsync(GetUploadFileListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.EntityName))
            {
                query = query.Where(x => x.EntityName == input.EntityName);
            }
            if (!string.IsNullOrWhiteSpace(input.RecordId))
            {
                query = query.Where(x => x.RecordId == input.RecordId);
            }
            if (input.OwnerId.HasValue)
            {
                query = query.Where(x => x.OwnerId == input.OwnerId.Value);
            }

            query = await _dataPermissionService.ApplyReadFilterAsync(query, "uploadFile", x => x.OwnerId, x => x.OwnerTeamId);

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount);
            var uploadFiles = await AsyncExecuter.ToListAsync(query);
            var dtos = ObjectMapper.Map<List<UploadFile>, List<UploadFileDto>>(uploadFiles);
            await FillLookupNamesAsync(dtos);
            await FillPermissionsAsync(dtos);

            return new PagedResultDto<UploadFileDto>(totalCount, dtos);
        }

        /// <summary>
        /// 根据实体获取附件列表
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="recordId">记录Id</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<UploadFileDto>> GetListByEntityAsync(string entityName, string recordId)
        {
            var queryable = await _repository.GetQueryableAsync();
            var baseQuery = queryable
                .Where(f => f.EntityName == entityName && f.RecordId == recordId);

            baseQuery = await _dataPermissionService.ApplyReadFilterAsync(baseQuery, "uploadFile", x => x.OwnerId, x => x.OwnerTeamId);

            var query = baseQuery.OrderByDescending(f => f.CreationTime);
            var uploadFiles = await AsyncExecuter.ToListAsync(query);
            var totalCount = uploadFiles.Count;

            return new PagedResultDto<UploadFileDto>(
                totalCount,
                ObjectMapper.Map<List<UploadFile>, List<UploadFileDto>>(uploadFiles)
            );
        }

        /// <summary>
        /// 创建（OwnerId不传则默认当前用户）
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的附件DTO</returns>
        [Authorize(TreadSnowPermissions.UploadFiles.Create)]
        public async Task<UploadFileDto> CreateAsync(CreateUploadFileDto input)
        {
            var uploadFile = ObjectMapper.Map<CreateUploadFileDto, UploadFile>(input);
            uploadFile.TenantId = CurrentTenant.Id;
            uploadFile.OwnerId = input.OwnerId ?? CurrentUser.Id;
            await _repository.InsertAsync(uploadFile);
            return ObjectMapper.Map<UploadFile, UploadFileDto>(uploadFile);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id">附件Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的附件DTO</returns>
        [Authorize(TreadSnowPermissions.UploadFiles.Edit)]
        public async Task<UploadFileDto> UpdateAsync(Guid id, UpdateUploadFileDto input)
        {
            var uploadFile = await _repository.GetAsync(id);
            var hasPermission = await _dataPermissionService.CheckWritePermissionAsync("uploadFile", uploadFile.OwnerId, uploadFile.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的编辑权限");
            ObjectMapper.Map(input, uploadFile);
            await _repository.UpdateAsync(uploadFile);
            return ObjectMapper.Map<UploadFile, UploadFileDto>(uploadFile);
        }

        /// <summary>
        /// 删除附件（含删除权限校验）
        /// </summary>
        /// <param name="id">附件Id</param>
        [Authorize(TreadSnowPermissions.UploadFiles.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var uploadFile = await _repository.GetAsync(id);
            var hasPermission = await _dataPermissionService.CheckDeletePermissionAsync("uploadFile", uploadFile.OwnerId, uploadFile.OwnerTeamId);
            if (!hasPermission) throw new Volo.Abp.UserFriendlyException("您没有该记录的删除权限");
            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// 获取用户下拉列表（用于选择负责人）
        /// </summary>
        /// <returns>用户Id和名称列表</returns>
        public async Task<ListResultDto<UserLookupDto>> GetOwnerLookupAsync()
        {
            var queryable = await _userRepository.GetQueryableAsync();
            var users = await AsyncExecuter.ToListAsync(queryable);
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
        /// 批量填充每条记录的编辑/删除权限标识
        /// </summary>
        /// <param name="dtos">DTO列表</param>
        private async Task FillPermissionsAsync(List<UploadFileDto> dtos)
        {
            var records = dtos.Select(d => (d.OwnerId, d.OwnerTeamId)).ToList();
            var permissions = await _dataPermissionService.BatchCheckPermissionsAsync("uploadFile", records);
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
        private async Task FillLookupNamesAsync(List<UploadFileDto> dtos)
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
                var users = await AsyncExecuter.ToListAsync(userQueryable.Where(u => allUserIds.Contains(u.Id)));
                userDict = users.ToDictionary(u => u.Id, u => u.UserName);
            }

            if (teamIds.Any())
            {
                var teamQueryable = await _teamRepository.GetQueryableAsync();
                var teams = await AsyncExecuter.ToListAsync(teamQueryable.Where(t => teamIds.Contains(t.Id)));
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

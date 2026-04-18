using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Accounts;
using TreadSnow.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.Pets.Default)]
    public class PetAppService : ApplicationService, IPetAppService
    {
        private readonly IRepository<Pet, Guid> _repository;
        private readonly IRepository<Account, Guid> _accountRepository;
        private readonly IAsyncQueryableExecuter _asyncExecuter;

        public PetAppService(IRepository<Pet, Guid> repository, IRepository<Account, Guid> accountRepository, IAsyncQueryableExecuter asyncExecuter)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _asyncExecuter = asyncExecuter;
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

            return dto;
        }

        /// <summary>
        /// 获取宠物分页列表（支持name模糊筛选，关联查询会员名称）
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

            var totalCount = await _asyncExecuter.CountAsync(query);

            query = query.OrderBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
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

            query = query.OrderBy(x => x.Name);
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
        /// 创建宠物
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
            ObjectMapper.Map(input, pet);
            await _repository.UpdateAsync(pet);
            return ObjectMapper.Map<Pet, PetDto>(pet);
        }

        /// <summary>
        /// 删除宠物
        /// </summary>
        /// <param name="id">宠物Id</param>
        [Authorize(TreadSnowPermissions.Pets.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}

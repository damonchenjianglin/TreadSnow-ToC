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
using Volo.Abp.ObjectMapping;

namespace TreadSnow.Pets
{
    [Authorize(TreadSnowPermissions.Pets.Default)]
    public class PetAppService : ApplicationService, IPetAppService
    {
        private readonly IRepository<Pet, Guid> _repository;
        private readonly IRepository<Account, Guid> _accountRepository;
        private readonly IAsyncQueryableExecuter _asyncExecuter;

        public PetAppService(
            IRepository<Pet, Guid> repository,
            IRepository<Account, Guid> accountRepository,
            IAsyncQueryableExecuter asyncExecuter)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _asyncExecuter = asyncExecuter;
        }

        /// <summary>
        /// 获取单条宠物信息
        /// </summary>
        public async Task<PetDto> GetAsync(Guid id)
        {
            var pet = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<Pet, PetDto>(pet);

            return dto;
        }

        /// <summary>
        /// 获取宠物列表
        /// </summary>
        public async Task<PagedResultDto<PetDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            var query = queryable
                .OrderBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var pets = await _asyncExecuter.ToListAsync(query);
            var totalCount = await _asyncExecuter.CountAsync(queryable);

            return new PagedResultDto<PetDto>(
                totalCount,
                ObjectMapper.Map<List<Pet>, List<PetDto>>(pets)
            );
        }

        /// <summary>
        /// 创建宠物
        /// </summary>
        [Authorize(TreadSnowPermissions.Pets.Create)]
        public async Task<PetDto> CreateAsync(CreatePetDto input)
        {
            // 验证主人是否存在
            var account = await _accountRepository.GetAsync(input.AccountId);
            if (account == null)
            {
                throw new UserFriendlyException("Account not found");
            }
            var pet = ObjectMapper.Map<CreatePetDto, Pet>(input);
            await _repository.InsertAsync(pet);
            return ObjectMapper.Map<Pet, PetDto>(pet);
        }

        /// <summary>
        /// 更新宠物
        /// </summary>
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
        [Authorize(TreadSnowPermissions.Pets.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
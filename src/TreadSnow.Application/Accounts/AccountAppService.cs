using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 会员应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.Accounts.Default)]
    public class AccountAppService : ApplicationService, IAccountAppService
    {
        private readonly IRepository<Account, Guid> _repository;

        public AccountAppService(IRepository<Account, Guid> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取单条会员
        /// </summary>
        /// <param name="id">会员Id</param>
        /// <returns>会员DTO</returns>
        public async Task<AccountDto> GetAsync(Guid id)
        {
            var account = await _repository.GetAsync(id);
            return ObjectMapper.Map<Account, AccountDto>(account);
        }

        /// <summary>
        /// 获取会员分页列表（支持name模糊筛选）
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<AccountDto>> GetListAsync(GetAccountListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                query = query.Where(x => x.Name.Contains(input.Name));
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.OrderBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount);
            var accounts = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<AccountDto>(totalCount, ObjectMapper.Map<List<Account>, List<AccountDto>>(accounts));
        }

        /// <summary>
        /// 获取所有会员列表（不分页，用于导出）
        /// </summary>
        /// <param name="name">名称模糊筛选（可选）</param>
        /// <returns>全量列表</returns>
        public async Task<List<AccountDto>> GetExportListAsync(string? name)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            query = query.OrderBy(x => x.Name);
            var accounts = await AsyncExecuter.ToListAsync(query);

            return ObjectMapper.Map<List<Account>, List<AccountDto>>(accounts);
        }

        /// <summary>
        /// 创建会员
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的会员DTO</returns>
        [Authorize(TreadSnowPermissions.Accounts.Create)]
        public async Task<AccountDto> CreateAsync(CreateAccountDto input)
        {
            var account = ObjectMapper.Map<CreateAccountDto, Account>(input);
            await _repository.InsertAsync(account);
            return ObjectMapper.Map<Account, AccountDto>(account);
        }

        /// <summary>
        /// 更新会员
        /// </summary>
        /// <param name="id">会员Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的会员DTO</returns>
        [Authorize(TreadSnowPermissions.Accounts.Edit)]
        public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountDto input)
        {
            var account = await _repository.GetAsync(id);
            ObjectMapper.Map(input, account);
            await _repository.UpdateAsync(account);
            return ObjectMapper.Map<Account, AccountDto>(account);
        }

        /// <summary>
        /// 删除会员
        /// </summary>
        /// <param name="id">会员Id</param>
        [Authorize(TreadSnowPermissions.Accounts.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}

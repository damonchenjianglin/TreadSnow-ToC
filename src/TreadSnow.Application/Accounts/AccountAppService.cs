using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Permissions;
using TreadSnow.UploadFiles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TreadSnow.Accounts
{
    [Authorize(TreadSnowPermissions.Accounts.Default)]
    public class AccountAppService : ApplicationService, IAccountAppService
    {
        private readonly IRepository<Account, Guid> _repository;

        public AccountAppService(IRepository<Account, Guid> repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// 获取单条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AccountDto> GetAsync(Guid id)
        {
            var Account = await _repository.GetAsync(id);
            return ObjectMapper.Map<Account, AccountDto>(Account);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<AccountDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable
                .OrderBy(x => "Name")
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var Accounts = await AsyncExecuter.ToListAsync(query);
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            return new PagedResultDto<AccountDto>(
                totalCount,
                ObjectMapper.Map<List<Account>, List<AccountDto>>(Accounts)
            );
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(TreadSnowPermissions.Accounts.Create)]
        public async Task<AccountDto> CreateAsync(CreateAccountDto input)
        {
            var Account = ObjectMapper.Map<CreateAccountDto, Account>(input);
            await _repository.InsertAsync(Account);
            return ObjectMapper.Map<Account, AccountDto>(Account);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(TreadSnowPermissions.Accounts.Edit)]
        public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountDto input)
        {
            var Account = await _repository.GetAsync(id);
            ObjectMapper.Map(input, Account);
            await _repository.UpdateAsync(Account);
            return ObjectMapper.Map<Account, AccountDto>(Account);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [Authorize(TreadSnowPermissions.Accounts.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        ///// <summary>
        ///// 根据实体获取附件列表
        ///// </summary>
        //public async Task<List<UploadFileDto>> GetListByEntityAsync(string entityName, string recordId)
        //{
        //    var queryable = await Repository.GetQueryableAsync();
        //    var query = queryable
        //        .Where(f => f.EntityName == entityName && f.RecordId == recordId)
        //        .OrderBy(f => f.Name);

        //    var files = await _asyncExecuter.ToListAsync(query);
        //    var dtos = ObjectMapper.Map<List<UploadFile>, List<UploadFileDto>>(files);

        //    foreach (var dto in dtos)
        //    {
        //        dto.Url = $"/api/uploadfiles/{dto.Id}/download";
        //    }

        //    return dtos;
        //}
    }

}

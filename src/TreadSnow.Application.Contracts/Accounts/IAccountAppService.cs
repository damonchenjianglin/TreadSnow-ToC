using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Accounts
{
    public interface IAccountAppService : ICrudAppService<AccountDto, Guid, PagedAndSortedResultRequestDto, CreateAccountDto, UpdateAccountDto>
    {
    }
}
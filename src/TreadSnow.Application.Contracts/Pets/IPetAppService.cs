using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Pets
{
    public interface IPetAppService : ICrudAppService<PetDto, Guid, PagedAndSortedResultRequestDto, CreatePetDto, UpdatePetDto>
    {
    }
}
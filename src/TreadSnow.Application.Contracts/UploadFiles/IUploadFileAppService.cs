using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.UploadFiles
{
    public interface IUploadFileAppService : ICrudAppService<UploadFileDto, Guid, GetUploadFileListDto, CreateUploadFileDto, UpdateUploadFileDto>
    {
        Task<PagedResultDto<UploadFileDto>> GetListByEntityAsync(string entityName, string recordId);
    }
}
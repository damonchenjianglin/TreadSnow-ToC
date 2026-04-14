using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.UploadFiles;
using TreadSnow.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace TreadSnow.UploadFiles
{
    [Authorize(TreadSnowPermissions.UploadFiles.Default)]
    public class UploadFileAppService : ApplicationService, IUploadFileAppService
    {
        private readonly IRepository<UploadFile, Guid> _repository;

        public UploadFileAppService(IRepository<UploadFile, Guid> repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// 获取单条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UploadFileDto> GetAsync(Guid id)
        {
            var UploadFile = await _repository.GetAsync(id);
            return ObjectMapper.Map<UploadFile, UploadFileDto>(UploadFile);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<UploadFileDto>> GetListAsync(GetUploadFileListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.Where(x => x.EntityName == input.EntityName && x.RecordId == input.RecordId).OrderBy(x => "Name");

            var UploadFiles = await AsyncExecuter.ToListAsync(query);
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            return new PagedResultDto<UploadFileDto>(
                totalCount,
                ObjectMapper.Map<List<UploadFile>, List<UploadFileDto>>(UploadFiles)
            );
        }

        /// <summary>
        /// 根据实体获取附件列表
        /// </summary>
        public async Task<PagedResultDto<UploadFileDto>> GetListByEntityAsync(string entityName, string recordId)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable
                .Where(f => f.EntityName == entityName && f.RecordId == recordId)
                .OrderBy(f => f.Name);

            var UploadFiles = await AsyncExecuter.ToListAsync(query);
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            return new PagedResultDto<UploadFileDto>(
                totalCount,
                ObjectMapper.Map<List<UploadFile>, List<UploadFileDto>>(UploadFiles)
            );
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(TreadSnowPermissions.UploadFiles.Create)]
        public async Task<UploadFileDto> CreateAsync(CreateUploadFileDto input)
        {
            var UploadFile = ObjectMapper.Map<CreateUploadFileDto, UploadFile>(input);
            await _repository.InsertAsync(UploadFile);
            return ObjectMapper.Map<UploadFile, UploadFileDto>(UploadFile);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(TreadSnowPermissions.UploadFiles.Edit)]
        public async Task<UploadFileDto> UpdateAsync(Guid id, UpdateUploadFileDto input)
        {
            var UploadFile = await _repository.GetAsync(id);
            ObjectMapper.Map(input, UploadFile);
            await _repository.UpdateAsync(UploadFile);
            return ObjectMapper.Map<UploadFile, UploadFileDto>(UploadFile);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [Authorize(TreadSnowPermissions.UploadFiles.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

    }

}
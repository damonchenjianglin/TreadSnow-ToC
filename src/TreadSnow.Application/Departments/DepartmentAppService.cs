using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreadSnow.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 部门应用服务
    /// </summary>
    [Authorize(TreadSnowPermissions.Departments.Default)]
    public class DepartmentAppService : ApplicationService, IDepartmentAppService
    {
        /// <summary>
        /// 部门仓储
        /// </summary>
        private readonly IRepository<Department, Guid> _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">部门仓储</param>
        public DepartmentAppService(IRepository<Department, Guid> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取单条部门
        /// </summary>
        /// <param name="id">部门Id</param>
        /// <returns>部门DTO</returns>
        public async Task<DepartmentDto> GetAsync(Guid id)
        {
            var department = await _repository.GetAsync(id);
            var dto = ObjectMapper.Map<Department, DepartmentDto>(department);
            if (department.ParentDepartmentId.HasValue)
            {
                var parent = await _repository.FindAsync(department.ParentDepartmentId.Value);
                dto.ParentDepartmentName = parent?.Name;
            }
            return dto;
        }

        /// <summary>
        /// 获取部门分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<DepartmentDto>> GetListAsync(GetDepartmentListDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var query = queryable.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                query = query.Where(x => x.Name.Contains(input.Name));
            }

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.OrderBy(x => x.No).Skip(input.SkipCount).Take(input.MaxResultCount);
            var departments = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<DepartmentDto>(totalCount, ObjectMapper.Map<List<Department>, List<DepartmentDto>>(departments));
        }

        /// <summary>
        /// 获取部门树形结构（全量列表，前端构建树）
        /// </summary>
        /// <returns>部门列表</returns>
        public async Task<List<DepartmentDto>> GetTreeAsync()
        {
            var queryable = await _repository.GetQueryableAsync();
            var departments = await AsyncExecuter.ToListAsync(queryable.OrderBy(x => x.No));
            return ObjectMapper.Map<List<Department>, List<DepartmentDto>>(departments);
        }

        /// <summary>
        /// 创建部门
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的部门DTO</returns>
        [Authorize(TreadSnowPermissions.Departments.Create)]
        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto input)
        {
            var department = ObjectMapper.Map<CreateDepartmentDto, Department>(input);
            department.TenantId = CurrentTenant.Id;
            await _repository.InsertAsync(department);
            return ObjectMapper.Map<Department, DepartmentDto>(department);
        }

        /// <summary>
        /// 更新部门
        /// </summary>
        /// <param name="id">部门Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的部门DTO</returns>
        [Authorize(TreadSnowPermissions.Departments.Edit)]
        public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto input)
        {
            var department = await _repository.GetAsync(id);
            ObjectMapper.Map(input, department);
            await _repository.UpdateAsync(department);
            return ObjectMapper.Map<Department, DepartmentDto>(department);
        }

        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id">部门Id</param>
        [Authorize(TreadSnowPermissions.Departments.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 部门应用服务接口
    /// </summary>
    public interface IDepartmentAppService : IApplicationService
    {
        /// <summary>
        /// 获取单条部门
        /// </summary>
        /// <param name="id">部门Id</param>
        /// <returns>部门DTO</returns>
        Task<DepartmentDto> GetAsync(Guid id);

        /// <summary>
        /// 获取部门分页列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<DepartmentDto>> GetListAsync(GetDepartmentListDto input);

        /// <summary>
        /// 获取部门树形结构
        /// </summary>
        /// <returns>部门树形列表</returns>
        Task<List<DepartmentDto>> GetTreeAsync();

        /// <summary>
        /// 创建部门
        /// </summary>
        /// <param name="input">创建DTO</param>
        /// <returns>创建后的部门DTO</returns>
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto input);

        /// <summary>
        /// 更新部门
        /// </summary>
        /// <param name="id">部门Id</param>
        /// <param name="input">更新DTO</param>
        /// <returns>更新后的部门DTO</returns>
        Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto input);

        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id">部门Id</param>
        Task DeleteAsync(Guid id);
    }
}

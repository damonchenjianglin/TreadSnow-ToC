import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto, GetDepartmentListDto } from './models';

/** 部门API服务 */
@Injectable({ providedIn: 'root' })
export class DepartmentService {
  /** API基础路径 */
  private apiUrl = '/api/app/department';

  constructor(private rest: RestService) {}

  /**
   * 获取单条部门
   * @param id 部门Id
   * @returns 部门DTO
   */
  get(id: string): Observable<DepartmentDto> {
    return this.rest.request<void, DepartmentDto>({ method: 'GET', url: `${this.apiUrl}/${id}` });
  }

  /**
   * 获取部门分页列表
   * @param params 查询参数
   * @returns 分页结果
   */
  getList(params?: GetDepartmentListDto): Observable<PagedResultDto<DepartmentDto>> {
    return this.rest.request<void, PagedResultDto<DepartmentDto>>({ method: 'GET', url: this.apiUrl, params });
  }

  /**
   * 获取部门树形列表
   * @returns 部门列表
   */
  getTree(): Observable<DepartmentDto[]> {
    return this.rest.request<void, DepartmentDto[]>({ method: 'GET', url: `${this.apiUrl}/tree` });
  }

  /**
   * 创建部门
   * @param input 创建DTO
   * @returns 创建后的部门DTO
   */
  create(input: CreateDepartmentDto): Observable<DepartmentDto> {
    return this.rest.request<CreateDepartmentDto, DepartmentDto>({ method: 'POST', url: this.apiUrl, body: input });
  }

  /**
   * 更新部门
   * @param id 部门Id
   * @param input 更新DTO
   * @returns 更新后的部门DTO
   */
  update(id: string, input: UpdateDepartmentDto): Observable<DepartmentDto> {
    return this.rest.request<UpdateDepartmentDto, DepartmentDto>({ method: 'PUT', url: `${this.apiUrl}/${id}`, body: input });
  }

  /**
   * 删除部门
   * @param id 部门Id
   * @returns void
   */
  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'DELETE', url: `${this.apiUrl}/${id}` });
  }
}

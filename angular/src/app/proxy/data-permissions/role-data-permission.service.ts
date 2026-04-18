import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { DataPermissionConfigDto, RoleDataPermissionDto, UserEffectivePermissionDto } from './models';

/** 角色数据权限API服务 */
@Injectable({ providedIn: 'root' })
export class RoleDataPermissionService {
  /** API基础路径 */
  private apiUrl = '/api/app/role-data-permission';

  constructor(private rest: RestService) {}

  /**
   * 获取角色的数据权限配置
   * @param roleId 角色Id
   * @returns 角色数据权限配置
   */
  get(roleId: string): Observable<RoleDataPermissionDto> {
    return this.rest.request<void, RoleDataPermissionDto>({ method: 'GET', url: this.apiUrl, params: { roleId } });
  }

  /**
   * 更新角色的数据权限配置
   * @param roleId 角色Id
   * @param configs 权限配置列表
   * @returns void
   */
  update(roleId: string, configs: DataPermissionConfigDto[]): Observable<void> {
    return this.rest.request<DataPermissionConfigDto[], void>({ method: 'PUT', url: this.apiUrl, params: { roleId }, body: configs });
  }

  /**
   * 获取当前用户的有效数据权限
   * @returns 用户有效权限
   */
  getCurrentUserPermissions(): Observable<UserEffectivePermissionDto> {
    return this.rest.request<void, UserEffectivePermissionDto>({ method: 'GET', url: `${this.apiUrl}/current-user-permissions` });
  }
}

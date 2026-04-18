import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { TeamDto, CreateTeamDto, UpdateTeamDto, GetTeamListDto, TeamUserDto, TeamRoleDto } from './models';

/** 团队API服务 */
@Injectable({ providedIn: 'root' })
export class TeamService {
  /** API基础路径 */
  private apiUrl = '/api/app/team';

  constructor(private rest: RestService) {}

  /**
   * 获取单条团队
   * @param id 团队Id
   * @returns 团队DTO
   */
  get(id: string): Observable<TeamDto> {
    return this.rest.request<void, TeamDto>({ method: 'GET', url: `${this.apiUrl}/${id}` });
  }

  /**
   * 获取团队分页列表
   * @param params 查询参数
   * @returns 分页结果
   */
  getList(params?: GetTeamListDto): Observable<PagedResultDto<TeamDto>> {
    return this.rest.request<void, PagedResultDto<TeamDto>>({ method: 'GET', url: this.apiUrl, params });
  }

  /**
   * 创建团队
   * @param input 创建DTO
   * @returns 创建后的团队DTO
   */
  create(input: CreateTeamDto): Observable<TeamDto> {
    return this.rest.request<CreateTeamDto, TeamDto>({ method: 'POST', url: this.apiUrl, body: input });
  }

  /**
   * 更新团队
   * @param id 团队Id
   * @param input 更新DTO
   * @returns 更新后的团队DTO
   */
  update(id: string, input: UpdateTeamDto): Observable<TeamDto> {
    return this.rest.request<UpdateTeamDto, TeamDto>({ method: 'PUT', url: `${this.apiUrl}/${id}`, body: input });
  }

  /**
   * 删除团队
   * @param id 团队Id
   * @returns void
   */
  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'DELETE', url: `${this.apiUrl}/${id}` });
  }

  /**
   * 获取团队用户列表
   * @param teamId 团队Id
   * @returns 团队用户列表
   */
  getUsers(teamId: string): Observable<TeamUserDto[]> {
    return this.rest.request<void, TeamUserDto[]>({ method: 'GET', url: `${this.apiUrl}/${teamId}/users` });
  }

  /**
   * 添加团队用户
   * @param teamId 团队Id
   * @param userId 用户Id
   * @returns void
   */
  addUser(teamId: string, userId: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'POST', url: `${this.apiUrl}/${teamId}/users/${userId}` });
  }

  /**
   * 移除团队用户
   * @param teamId 团队Id
   * @param userId 用户Id
   * @returns void
   */
  removeUser(teamId: string, userId: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'DELETE', url: `${this.apiUrl}/${teamId}/users/${userId}` });
  }

  /**
   * 获取团队角色列表
   * @param teamId 团队Id
   * @returns 团队角色列表
   */
  getRoles(teamId: string): Observable<TeamRoleDto[]> {
    return this.rest.request<void, TeamRoleDto[]>({ method: 'GET', url: `${this.apiUrl}/${teamId}/roles` });
  }

  /**
   * 添加团队角色
   * @param teamId 团队Id
   * @param roleId 角色Id
   * @returns void
   */
  addRole(teamId: string, roleId: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'POST', url: `${this.apiUrl}/${teamId}/roles/${roleId}` });
  }

  /**
   * 移除团队角色
   * @param teamId 团队Id
   * @param roleId 角色Id
   * @returns void
   */
  removeRole(teamId: string, roleId: string): Observable<void> {
    return this.rest.request<void, void>({ method: 'DELETE', url: `${this.apiUrl}/${teamId}/roles/${roleId}` });
  }
}

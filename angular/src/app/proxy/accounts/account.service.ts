import type { AccountDto, CreateAccountDto, UpdateAccountDto, UserLookupDto, TeamLookupDto } from './models';
import { RestService } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

/**
 * 会员管理API服务
 * 提供会员的增删改查接口调用
 */
@Injectable({
  providedIn: 'root',
})
export class AccountService {
  /** API名称 */
  apiName = 'Default';

  /**
   * 创建会员
   * @param input 创建会员DTO
   * @returns 创建后的会员DTO
   */
  create = (input: CreateAccountDto) =>
    this.restService.request<any, AccountDto>({
      method: 'POST',
      url: '/api/app/account',
      body: input,
    }, { apiName: this.apiName });

  /**
   * 删除会员
   * @param id 会员ID
   */
  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/account/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取单个会员详情
   * @param id 会员ID
   * @returns 会员DTO
   */
  get = (id: string) =>
    this.restService.request<any, AccountDto>({
      method: 'GET',
      url: `/api/app/account/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取会员分页列表（支持name模糊筛选和负责人筛选）
   * @param input 分页排序参数（含可选name和ownerId）
   * @returns 会员分页结果
   */
  getList = (input: PagedAndSortedResultRequestDto & { name?: string; ownerId?: string }) =>
    this.restService.request<any, PagedResultDto<AccountDto>>({
      method: 'GET',
      url: '/api/app/account',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, name: input.name, ownerId: input.ownerId },
    }, { apiName: this.apiName });

  /**
   * 获取所有会员列表（不分页，用于导出）
   * @param name 名称模糊筛选（可选）
   * @returns 全量会员列表
   */
  getAllList = (name?: string) =>
    this.restService.request<any, AccountDto[]>({
      method: 'GET',
      url: '/api/app/account/export-list',
      params: { name },
    }, { apiName: this.apiName });

  /**
   * 更新会员
   * @param id 会员ID
   * @param input 更新会员DTO
   * @returns 更新后的会员DTO
   */
  update = (id: string, input: UpdateAccountDto) =>
    this.restService.request<any, AccountDto>({
      method: 'PUT',
      url: `/api/app/account/${id}`,
      body: input,
    }, { apiName: this.apiName });

  /**
   * 获取用户下拉列表（用于选择负责人）
   * @returns 用户Id和名称列表
   */
  getOwnerLookup = () =>
    this.restService.request<any, ListResultDto<UserLookupDto>>({
      method: 'GET',
      url: '/api/app/account/owner-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取团队下拉列表（用于选择负责团队）
   * @returns 团队Id和名称列表
   */
  getTeamLookup = () =>
    this.restService.request<any, ListResultDto<TeamLookupDto>>({
      method: 'GET',
      url: '/api/app/account/team-lookup',
    }, { apiName: this.apiName });

  constructor(private restService: RestService) {}
}

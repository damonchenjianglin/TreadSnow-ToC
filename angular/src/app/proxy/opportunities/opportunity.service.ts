import type { OpportunityDto, CreateOpportunityDto, UpdateOpportunityDto, AccountLookupDto, UserLookupDto, TeamLookupDto } from './models';
import { RestService } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

/**
 * 商机服务
 * 负责与后端商机API通信，提供增删改查接口
 */
@Injectable({
  providedIn: 'root',
})
export class OpportunityService {
  /** API名称 */
  apiName = 'Default';

  /**
   * 创建商机
   * @param input 创建商机DTO
   * @returns 创建成功的商机DTO
   */
  create = (input: CreateOpportunityDto) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: '/api/app/opportunity',
      body: input,
    }, { apiName: this.apiName });

  /**
   * 删除商机
   * @param id 商机ID
   */
  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/opportunity/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取单个商机
   * @param id 商机ID
   * @returns 商机DTO
   */
  get = (id: string) =>
    this.restService.request<any, OpportunityDto>({
      method: 'GET',
      url: `/api/app/opportunity/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取商机分页列表（支持name模糊筛选和负责人筛选）
   * @param input 分页排序请求参数（含可选name和ownerId）
   * @returns 商机分页结果
   */
  getList = (input: PagedAndSortedResultRequestDto & { name?: string; ownerId?: string; startCreationTime?: string; endCreationTime?: string; startLastModificationTime?: string; endLastModificationTime?: string }) =>
    this.restService.request<any, PagedResultDto<OpportunityDto>>({
      method: 'GET',
      url: '/api/app/opportunity',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, name: input.name, ownerId: input.ownerId, startCreationTime: input.startCreationTime, endCreationTime: input.endCreationTime, startLastModificationTime: input.startLastModificationTime, endLastModificationTime: input.endLastModificationTime },
    }, { apiName: this.apiName });

  /**
   * 获取所有商机列表（不分页，用于导出）
   * @param name 名称模糊筛选（可选）
   * @returns 全量商机列表
   */
  getAllList = (name?: string) =>
    this.restService.request<any, OpportunityDto[]>({
      method: 'GET',
      url: '/api/app/opportunity/export-list',
      params: { name },
    }, { apiName: this.apiName });

  /**
   * 更新商机
   * @param id 商机ID
   * @param input 更新商机DTO
   * @returns 更新后的商机DTO
   */
  update = (id: string, input: UpdateOpportunityDto) =>
    this.restService.request<any, OpportunityDto>({
      method: 'PUT',
      url: `/api/app/opportunity/${id}`,
      body: input,
    }, { apiName: this.apiName });

  /**
   * 获取客户下拉列表（用于选择客户）
   * @returns 客户Id和名称列表
   */
  getAccountLookup = () =>
    this.restService.request<any, ListResultDto<AccountLookupDto>>({
      method: 'GET',
      url: '/api/app/opportunity/account-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取用户下拉列表（用于选择负责人）
   * @returns 用户Id和名称列表
   */
  getOwnerLookup = () =>
    this.restService.request<any, ListResultDto<UserLookupDto>>({
      method: 'GET',
      url: '/api/app/opportunity/owner-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取团队下拉列表（用于选择负责团队）
   * @returns 团队Id和名称列表
   */
  getTeamLookup = () =>
    this.restService.request<any, ListResultDto<TeamLookupDto>>({
      method: 'GET',
      url: '/api/app/opportunity/team-lookup',
    }, { apiName: this.apiName });

  constructor(private restService: RestService) {}
}

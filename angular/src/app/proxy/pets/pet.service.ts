import type { PetDto, CreatePetDto, UpdatePetDto, AccountLookupDto, UserLookupDto, TeamLookupDto } from './models';
import { RestService } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

/**
 * 宠物服务
 * 负责与后端宠物API通信，提供增删改查接口
 */
@Injectable({
  providedIn: 'root',
})
export class PetService {
  /** API名称 */
  apiName = 'Default';

  /**
   * 创建宠物
   * @param input 创建宠物DTO
   * @returns 创建成功的宠物DTO
   */
  create = (input: CreatePetDto) =>
    this.restService.request<any, PetDto>({
      method: 'POST',
      url: '/api/app/pet',
      body: input,
    }, { apiName: this.apiName });

  /**
   * 删除宠物
   * @param id 宠物ID
   */
  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/pet/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取单个宠物
   * @param id 宠物ID
   * @returns 宠物DTO
   */
  get = (id: string) =>
    this.restService.request<any, PetDto>({
      method: 'GET',
      url: `/api/app/pet/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取宠物分页列表（支持name模糊筛选和负责人筛选）
   * @param input 分页排序请求参数（含可选name和ownerId）
   * @returns 宠物分页结果
   */
  getList = (input: PagedAndSortedResultRequestDto & { name?: string; ownerId?: string; startCreationTime?: string; endCreationTime?: string; startLastModificationTime?: string; endLastModificationTime?: string }) =>
    this.restService.request<any, PagedResultDto<PetDto>>({
      method: 'GET',
      url: '/api/app/pet',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, name: input.name, ownerId: input.ownerId, startCreationTime: input.startCreationTime, endCreationTime: input.endCreationTime, startLastModificationTime: input.startLastModificationTime, endLastModificationTime: input.endLastModificationTime },
    }, { apiName: this.apiName });

  /**
   * 获取所有宠物列表（不分页，用于导出）
   * @param name 名称模糊筛选（可选）
   * @returns 全量宠物列表
   */
  getAllList = (name?: string) =>
    this.restService.request<any, PetDto[]>({
      method: 'GET',
      url: '/api/app/pet/export-list',
      params: { name },
    }, { apiName: this.apiName });

  /**
   * 更新宠物
   * @param id 宠物ID
   * @param input 更新宠物DTO
   * @returns 更新后的宠物DTO
   */
  update = (id: string, input: UpdatePetDto) =>
    this.restService.request<any, PetDto>({
      method: 'PUT',
      url: `/api/app/pet/${id}`,
      body: input,
    }, { apiName: this.apiName });

  /**
   * 获取会员下拉列表（用于选择主人）
   * @returns 会员Id和名称列表
   */
  getAccountLookup = () =>
    this.restService.request<any, ListResultDto<AccountLookupDto>>({
      method: 'GET',
      url: '/api/app/pet/account-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取用户下拉列表（用于选择负责人）
   * @returns 用户Id和名称列表
   */
  getOwnerLookup = () =>
    this.restService.request<any, ListResultDto<UserLookupDto>>({
      method: 'GET',
      url: '/api/app/pet/owner-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取团队下拉列表（用于选择负责团队）
   * @returns 团队Id和名称列表
   */
  getTeamLookup = () =>
    this.restService.request<any, ListResultDto<TeamLookupDto>>({
      method: 'GET',
      url: '/api/app/pet/team-lookup',
    }, { apiName: this.apiName });

  constructor(private restService: RestService) {}
}

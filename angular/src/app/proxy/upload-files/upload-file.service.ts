import type { UploadFileDto, CreateUploadFileDto, UpdateUploadFileDto, UserLookupDto, TeamLookupDto } from './models';
import { RestService } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

/**
 * 附件服务
 * 提供附件的增删改查API调用
 */
@Injectable({
  providedIn: 'root',
})
export class UploadFileService {
  /** API名称 */
  apiName = 'Default';

  /**
   * 创建附件
   * @param input 新建附件DTO
   * @returns 创建后的附件DTO
   */
  create = (input: CreateUploadFileDto) =>
    this.restService.request<any, UploadFileDto>({
      method: 'POST',
      url: '/api/app/upload-file',
      body: input,
    }, { apiName: this.apiName });

  /**
   * 删除附件
   * @param id 附件ID
   */
  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/upload-file/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取单个附件
   * @param id 附件ID
   * @returns 附件DTO
   */
  get = (id: string) =>
    this.restService.request<any, UploadFileDto>({
      method: 'GET',
      url: `/api/app/upload-file/${id}`,
    }, { apiName: this.apiName });

  /**
   * 获取附件分页列表（支持entityName、recordId、ownerId筛选）
   * @param input 分页排序请求参数
   * @returns 分页结果
   */
  getList = (input: PagedAndSortedResultRequestDto & { entityName?: string; recordId?: string; ownerId?: string }) =>
    this.restService.request<any, PagedResultDto<UploadFileDto>>({
      method: 'GET',
      url: '/api/app/upload-file',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, entityName: input.entityName, recordId: input.recordId, ownerId: input.ownerId },
    }, { apiName: this.apiName });

  /**
   * 更新附件
   * @param id 附件ID
   * @param input 更新附件DTO
   * @returns 更新后的附件DTO
   */
  update = (id: string, input: UpdateUploadFileDto) =>
    this.restService.request<any, UploadFileDto>({
      method: 'PUT',
      url: `/api/app/upload-file/${id}`,
      body: input,
    }, { apiName: this.apiName });

  /**
   * 获取用户下拉列表（用于选择负责人）
   * @returns 用户Id和名称列表
   */
  getOwnerLookup = () =>
    this.restService.request<any, ListResultDto<UserLookupDto>>({
      method: 'GET',
      url: '/api/app/upload-file/owner-lookup',
    }, { apiName: this.apiName });

  /**
   * 获取团队下拉列表（用于选择负责团队）
   * @returns 团队Id和名称列表
   */
  getTeamLookup = () =>
    this.restService.request<any, ListResultDto<TeamLookupDto>>({
      method: 'GET',
      url: '/api/app/upload-file/team-lookup',
    }, { apiName: this.apiName });

  constructor(private restService: RestService) {}
}

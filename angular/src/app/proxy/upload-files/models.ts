import type { EntityDto } from '@abp/ng.core';

/**
 * 附件DTO
 * 用于附件列表展示和详情查看
 */
export interface UploadFileDto extends EntityDto<string> {
  /** 租户ID */
  tenantId?: string;
  /** 实体名称 */
  entityName?: string;
  /** 记录ID */
  recordId?: string;
  /** 文件名称 */
  name?: string;
  /** 文件类型 */
  type?: string;
  /** 文件路径 */
  path?: string;
  /** 文件访问URL（只读，后端计算） */
  url?: string;
}

/**
 * 新建附件DTO
 * 用于创建新附件时提交的数据
 */
export interface CreateUploadFileDto {
  /** 租户ID */
  tenantId?: string;
  /** 实体名称 */
  entityName: string;
  /** 记录ID */
  recordId: string;
  /** 文件名称 */
  name: string;
  /** 文件类型 */
  type: string;
  /** 文件路径 */
  path: string;
}

/**
 * 更新附件DTO
 * 用于编辑附件时提交的数据
 */
export interface UpdateUploadFileDto {
  /** 租户ID */
  tenantId?: string;
  /** 实体名称 */
  entityName: string;
  /** 记录ID */
  recordId: string;
  /** 文件名称 */
  name: string;
  /** 文件类型 */
  type: string;
  /** 文件路径 */
  path: string;
}

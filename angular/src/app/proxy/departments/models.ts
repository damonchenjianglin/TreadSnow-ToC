import { EntityDto } from '@abp/ng.core';

/** 部门DTO */
export interface DepartmentDto extends EntityDto<string> {
  /** 编号 */
  no: number;
  /** 名称 */
  name?: string;
  /** 上级部门Id */
  parentDepartmentId?: string;
  /** 上级部门名称 */
  parentDepartmentName?: string;
}

/** 创建部门DTO */
export interface CreateDepartmentDto {
  /** 名称 */
  name: string;
  /** 上级部门Id */
  parentDepartmentId?: string;
}

/** 更新部门DTO */
export interface UpdateDepartmentDto {
  /** 名称 */
  name: string;
  /** 上级部门Id */
  parentDepartmentId?: string;
}

/** 部门查询DTO */
export interface GetDepartmentListDto {
  /** 名称筛选 */
  name?: string;
  /** 跳过条数 */
  skipCount?: number;
  /** 每页条数 */
  maxResultCount?: number;
  /** 排序 */
  sorting?: string;
}

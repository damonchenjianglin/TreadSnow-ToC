import { EntityDto } from '@abp/ng.core';

/** 团队DTO */
export interface TeamDto extends EntityDto<string> {
  /** 编号 */
  no: number;
  /** 名称 */
  name?: string;
  /** 所属部门Id */
  departmentId?: string;
  /** 所属部门名称 */
  departmentName?: string;
}

/** 创建团队DTO */
export interface CreateTeamDto {
  /** 名称 */
  name: string;
  /** 所属部门Id */
  departmentId?: string;
}

/** 更新团队DTO */
export interface UpdateTeamDto {
  /** 名称 */
  name: string;
  /** 所属部门Id */
  departmentId?: string;
}

/** 团队查询DTO */
export interface GetTeamListDto {
  /** 名称筛选 */
  name?: string;
  /** 跳过条数 */
  skipCount?: number;
  /** 每页条数 */
  maxResultCount?: number;
  /** 排序 */
  sorting?: string;
}

/** 团队用户DTO */
export interface TeamUserDto {
  /** 用户Id */
  userId: string;
  /** 用户名 */
  userName?: string;
}

/** 团队角色DTO */
export interface TeamRoleDto {
  /** 角色Id */
  roleId: string;
  /** 角色名称 */
  roleName?: string;
}

import type { EntityDto } from '@abp/ng.core';

/** 宠物DTO，用于列表展示和详情查看 */
export interface PetDto extends EntityDto<string> {
  /** 租户ID（多租户隔离） */
  tenantId?: string;
  /** 编号（自增，只读） */
  no: number;
  /** 宠物名称 */
  name?: string;
  /** 所属会员ID */
  accountId?: string;
  /** 主人名称（关联查询） */
  accountName?: string;
}

/** 会员下拉选项DTO（用于宠物表单选择主人） */
export interface AccountLookupDto extends EntityDto<string> {
  /** 会员名称 */
  name?: string;
}

/** 创建宠物DTO */
export interface CreatePetDto {
  /** 宠物名称（必填） */
  name: string;
  /** 所属会员ID（必填） */
  accountId: string;
}

/** 更新宠物DTO */
export interface UpdatePetDto {
  /** 宠物名称（必填） */
  name: string;
  /** 所属会员ID（必填） */
  accountId: string;
}

/** 单个实体的数据权限配置 */
export interface DataPermissionConfigDto {
  /** 实体名称 */
  entityName: string;
  /** 读权限等级（0-4） */
  readLevel: number;
  /** 写权限等级（0-4） */
  writeLevel: number;
  /** 删除权限等级（0-4） */
  deleteLevel: number;
}

/** 角色数据权限配置 */
export interface RoleDataPermissionDto {
  /** 角色Id */
  roleId: string;
  /** 权限配置列表 */
  configs: DataPermissionConfigDto[];
}

/** 用户有效数据权限 */
export interface UserEffectivePermissionDto {
  /** 权限配置列表（多角色合并取最大值） */
  configs: DataPermissionConfigDto[];
}

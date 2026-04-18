import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { RestService } from '@abp/ng.core';

/** 权限组 */
interface PermissionGroup {
  /** 权限组名称 */
  name: string;
  /** 权限组显示名称 */
  displayName: string;
  /** 权限列表 */
  permissions: PermissionItem[];
}

/** 权限项 */
interface PermissionItem {
  /** 权限名称 */
  name: string;
  /** 权限显示名称 */
  displayName: string;
  /** 父权限名称 */
  parentName: string | null;
  /** 是否已授予 */
  isGranted: boolean;
  /** 缩进层级（根据parentName计算） */
  level: number;
}

/** ABP权限管理API响应 */
interface PermissionResponse {
  /** 实体显示名称 */
  entityDisplayName: string;
  /** 权限组列表 */
  groups: PermissionGroup[];
}

/**
 * 菜单权限组件（NG-ZORRO实现，内联在tab中）
 * 替代ABP自带的权限管理弹窗，直接显示权限树
 */
@Component({
  standalone: false,
  selector: 'app-menu-permission',
  templateUrl: './menu-permission.component.html',
  styleUrls: ['./menu-permission.component.scss'],
})
export class MenuPermissionComponent implements OnChanges {
  /** 角色名称（ABP权限的providerKey） */
  @Input() roleName = '';

  /** 权限组列表 */
  groups: PermissionGroup[] = [];

  /** 当前选中的权限组索引 */
  selectedGroupIndex = 0;

  /** 加载中状态 */
  loading = false;

  /** 保存中状态 */
  saving = false;

  /** 全选状态 */
  allGranted = false;

  /** API基础路径 */
  private apiUrl = '/api/permission-management/permissions';

  constructor(private rest: RestService) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['roleName'] && this.roleName) {
      this.loadPermissions();
    }
  }

  /** 加载权限数据 */
  loadPermissions() {
    this.loading = true;
    this.rest.request<void, PermissionResponse>({
      method: 'GET',
      url: this.apiUrl,
      params: { providerName: 'R', providerKey: this.roleName },
    }).subscribe({
      next: (res) => {
        this.groups = res.groups.map((g) => ({
          ...g,
          permissions: g.permissions.map((p) => ({
            ...p,
            level: this.calcLevel(p, g.permissions),
          })),
        }));
        this.updateAllGranted();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  /**
   * 计算权限项的缩进层级
   * @param item 当前权限项
   * @param all 所有权限项
   * @returns 层级数
   */
  private calcLevel(item: PermissionItem, all: PermissionItem[]): number {
    let level = 0;
    let current = item;
    while (current.parentName) {
      level++;
      const parent = all.find((p) => p.name === current.parentName);
      if (!parent) break;
      current = parent;
    }
    return level;
  }

  /** 切换权限组 */
  selectGroup(index: number) {
    this.selectedGroupIndex = index;
  }

  /**
   * 切换单个权限的授予状态
   * @param perm 权限项
   */
  togglePermission(perm: PermissionItem) {
    const group = this.groups[this.selectedGroupIndex];
    if (!group) return;
    perm.isGranted = !perm.isGranted;
    if (perm.isGranted) {
      this.grantParents(perm, group.permissions);
    } else {
      this.revokeChildren(perm, group.permissions);
    }
    this.updateAllGranted();
  }

  /** 授予时自动勾选父权限 */
  private grantParents(perm: PermissionItem, all: PermissionItem[]) {
    if (perm.parentName) {
      const parent = all.find((p) => p.name === perm.parentName);
      if (parent && !parent.isGranted) {
        parent.isGranted = true;
        this.grantParents(parent, all);
      }
    }
  }

  /** 取消时自动取消子权限 */
  private revokeChildren(perm: PermissionItem, all: PermissionItem[]) {
    all.filter((p) => p.parentName === perm.name).forEach((child) => {
      child.isGranted = false;
      this.revokeChildren(child, all);
    });
  }

  /** 切换当前组全选 */
  toggleGroupAll(checked: boolean) {
    const group = this.groups[this.selectedGroupIndex];
    if (!group) return;
    group.permissions.forEach((p) => (p.isGranted = checked));
    this.updateAllGranted();
  }

  /** 切换全局全选 */
  toggleAll(checked: boolean) {
    this.groups.forEach((g) => g.permissions.forEach((p) => (p.isGranted = checked)));
    this.allGranted = checked;
  }

  /** 更新全选状态 */
  private updateAllGranted() {
    this.allGranted = this.groups.every((g) => g.permissions.every((p) => p.isGranted));
  }

  /** 当前组是否全选 */
  get currentGroupAllGranted(): boolean {
    const group = this.groups[this.selectedGroupIndex];
    return group ? group.permissions.every((p) => p.isGranted) : false;
  }

  /** 当前组是否部分选中 */
  get currentGroupIndeterminate(): boolean {
    const group = this.groups[this.selectedGroupIndex];
    if (!group) return false;
    const granted = group.permissions.filter((p) => p.isGranted).length;
    return granted > 0 && granted < group.permissions.length;
  }

  /** 保存权限配置 */
  save() {
    this.saving = true;
    const permissions: { name: string; isGranted: boolean }[] = [];
    this.groups.forEach((g) =>
      g.permissions.forEach((p) => permissions.push({ name: p.name, isGranted: p.isGranted }))
    );
    this.rest.request<any, void>({
      method: 'PUT',
      url: this.apiUrl,
      params: { providerName: 'R', providerKey: this.roleName },
      body: { permissions },
    }).subscribe({
      next: () => {
        this.saving = false;
      },
      error: () => {
        this.saving = false;
      },
    });
  }
}

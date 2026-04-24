import { ListService, PagedResultDto } from '@abp/ng.core';
import { IdentityRoleService } from '@abp/ng.identity/proxy';
import { IdentityRoleDto } from '@abp/ng.identity/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { DataPermissionModalComponent } from './data-permission-modal/data-permission-modal.component';
import { MenuPermissionComponent } from './menu-permission/menu-permission.component';
import { fetchAllPaged, exportToXlsx } from '../shared/export-xlsx';

/** 角色列表组件 */
@Component({
  standalone: false,
  selector: 'app-role',
  templateUrl: './role.component.html',
  providers: [ListService],
})
export class RoleComponent implements OnInit {
  /** 数据权限子组件引用 */
  @ViewChild(DataPermissionModalComponent) dataPermissionModal!: DataPermissionModalComponent;

  /** 菜单权限子组件引用 */
  @ViewChild(MenuPermissionComponent) menuPermissionModal!: MenuPermissionComponent;

  /** 角色分页数据 */
  roles = { items: [], totalCount: 0 } as PagedResultDto<IdentityRoleDto>;

  /** 当前选中的角色 */
  selectedRole = {} as IdentityRoleDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 新增/编辑弹窗是否可见 */
  isModalOpen = false;

  /** 数据加载状态 */
  loading = false;

  /** 当前页码 */
  currentPage = 1;

  /** 名称列筛选关键词 */
  nameFilter = '';

  /** 名称列筛选弹出框是否可见 */
  nameFilterVisible = false;

  /** 权限管理弹窗是否可见 */
  permissionModalVisible = false;

  /** 权限管理弹窗对应的角色名称 */
  permissionRoleName = '';

  /** 权限管理tabs当前选中索引（0=菜单权限, 1=数据权限） */
  permissionTabIndex = 0;

  /** ABP菜单权限的providerKey（角色名称） */
  permissionProviderKey = '';

  /** 数据权限对应的角色Id */
  dataPermissionRoleId = '';

  /** 菜单权限保存中状态 */
  menuPermissionSaving = false;

  /** 数据权限保存中状态 */
  dataPermissionSaving = false;

  /** 按名称排序函数 */
  sortByName = (a: IdentityRoleDto, b: IdentityRoleDto) => (a.name ?? '').localeCompare(b.name ?? '');

  constructor(public readonly list: ListService, private roleService: IdentityRoleService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 数据查询流，传入filter模糊筛选 */
    const streamCreator = (query: any) => this.roleService.getList({ ...query, filter: this.nameFilter || undefined });
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.roles = response;
      this.loading = false;
    });
  }

  /** 名称列筛选确定 */
  onNameFilterSearch() {
    this.nameFilterVisible = false;
    this.list.get();
  }

  /** 名称列筛选重置 */
  onNameFilterReset() {
    this.nameFilter = '';
    this.nameFilterVisible = false;
    this.list.get();
  }

  /** 打开新建角色弹窗 */
  createRole() {
    this.selectedRole = {} as IdentityRoleDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /** 打开编辑角色弹窗 */
  editRole(id: string) {
    this.roleService.get(id).subscribe((role) => {
      this.selectedRole = role;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /** 构建表单 */
  buildForm() {
    this.form = this.fb.group({
      name: [{ value: this.selectedRole.name || '', disabled: !!(this.selectedRole.id && this.selectedRole.isStatic) }, [Validators.required, Validators.maxLength(256)]],
      isDefault: [this.selectedRole.isDefault || false],
      isPublic: [this.selectedRole.isPublic || false],
    });
  }

  /** 保存角色 */
  save() {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    if (this.selectedRole.id) {
      this.roleService.update(this.selectedRole.id, { ...raw, concurrencyStamp: this.selectedRole.concurrencyStamp }).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    } else {
      this.roleService.create(raw).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    }
  }

  /** 删除角色（静态角色不可删） */
  deleteRole(role: IdentityRoleDto) {
    if (role.isStatic) return;
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.roleService.delete(role.id!).subscribe(() => this.list.get());
      }
    });
  }

  /** 打开权限管理弹窗（含菜单权限和数据权限tabs） */
  openPermissionModal(role: IdentityRoleDto) {
    this.permissionProviderKey = role.name!;
    this.dataPermissionRoleId = role.id!;
    this.permissionRoleName = role.name!;
    this.permissionTabIndex = 0;
    this.permissionModalVisible = true;
  }

  /** 关闭权限管理弹窗 */
  closePermissionModal() {
    this.permissionModalVisible = false;
  }

  /** 保存菜单权限 */
  saveMenuPermission() {
    this.menuPermissionSaving = true;
    this.menuPermissionModal.save();
    setTimeout(() => { this.menuPermissionSaving = false; }, 1500);
  }

  /** 保存数据权限 */
  saveDataPermission() {
    this.dataPermissionSaving = true;
    this.dataPermissionModal.save();
  }

  /** 数据权限保存成功回调 */
  onDataPermissionSaved() {
    this.dataPermissionSaving = false;
  }

  /** 数据权限保存失败回调 */
  onDataPermissionSaveFailed() {
    this.dataPermissionSaving = false;
  }

  /** 关闭新增/编辑弹窗 */
  closeModal() {
    this.isModalOpen = false;
  }

  /** 分页页码变更 */
  onPageIndexChange(page: number) {
    this.currentPage = page;
    this.loading = true;
    this.list.page = page - 1;
  }

  /** 分页每页条数变更 */
  onPageSizeChange(size: number) {
    this.loading = true;
    this.list.maxResultCount = size;
  }

  /** 导出当前条件数据（分页批量拉取） */
  async exportCurrent() {
    this.loading = true;
    try {
      const data = await fetchAllPaged<IdentityRoleDto>((skip, take) => this.roleService.getList({ filter: this.nameFilter || undefined, skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '角色名称': d.name ?? '', '默认': d.isDefault ? '是' : '否', '公开': d.isPublic ? '是' : '否', '静态': d.isStatic ? '是' : '否' }));
      exportToXlsx(rows, '角色_当前条件');
    } finally {
      this.loading = false;
    }
  }

  /** 导出所有数据（分页批量拉取） */
  async exportAll() {
    this.loading = true;
    try {
      const data = await fetchAllPaged<IdentityRoleDto>((skip, take) => this.roleService.getList({ skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '角色名称': d.name ?? '', '默认': d.isDefault ? '是' : '否', '公开': d.isPublic ? '是' : '否', '静态': d.isStatic ? '是' : '否' }));
      exportToXlsx(rows, '角色_全部');
    } finally {
      this.loading = false;
    }
  }
}

import { ListService, PagedResultDto } from '@abp/ng.core';
import { IdentityRoleService } from '@abp/ng.identity/proxy';
import { IdentityRoleDto } from '@abp/ng.identity/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';

/** 角色-菜单权限列表组件 */
@Component({
  standalone: false,
  selector: 'app-role',
  templateUrl: './role.component.html',
  providers: [ListService],
})
export class RoleComponent implements OnInit {
  /** 角色分页数据 */
  roles = { items: [], totalCount: 0 } as PagedResultDto<IdentityRoleDto>;

  /** 当前选中的角色 */
  selectedRole = {} as IdentityRoleDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 弹窗是否可见 */
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
  permissionVisible = false;

  /** 权限管理的providerKey（角色名称） */
  permissionProviderKey = '';

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

  /** 打开权限管理弹窗 */
  openPermissions(role: IdentityRoleDto) {
    this.permissionProviderKey = role.name!;
    this.permissionVisible = true;
  }

  /** 关闭弹窗 */
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

  /** 导出当前条件数据 */
  exportCurrent() {
    this.roleService.getList({ filter: this.nameFilter || undefined, maxResultCount: 10000 } as any).subscribe((res) => {
      this.downloadXlsx(res.items ?? [], '角色_当前条件');
    });
  }

  /** 导出所有数据 */
  exportAll() {
    this.roleService.getAllList().subscribe((res) => {
      this.downloadXlsx(res.items ?? [], '角色_全部');
    });
  }

  /** 下载xlsx文件 */
  private downloadXlsx(data: IdentityRoleDto[], filename: string) {
    const rows = data.map((d) => ({ '角色名称': d.name ?? '', '默认': d.isDefault ? '是' : '否', '公开': d.isPublic ? '是' : '否', '静态': d.isStatic ? '是' : '否' }));
    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, '角色');
    XLSX.writeFile(wb, `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}

import { ConfigStateService, ListService, PagedResultDto } from '@abp/ng.core';
import { IdentityUserService, IdentityRoleService } from '@abp/ng.identity/proxy';
import { IdentityUserDto, IdentityRoleDto } from '@abp/ng.identity/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { fetchAllPaged, exportToXlsx } from '../shared/export-xlsx';
import { DepartmentService, DepartmentDto } from '../proxy/departments';

/** 用户管理列表组件 */
@Component({
  standalone: false,
  selector: 'app-user',
  templateUrl: './user.component.html',
  providers: [ListService],
})
export class UserComponent implements OnInit {
  /** 用户分页数据 */
  users = { items: [], totalCount: 0 } as PagedResultDto<IdentityUserDto>;

  /** 当前选中的用户 */
  selectedUser = {} as IdentityUserDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 弹窗是否可见 */
  isModalOpen = false;

  /** 是否为新建模式 */
  isNew = false;

  /** 数据加载状态 */
  loading = false;

  /** 当前页码 */
  currentPage = 1;

  /** 当前tab索引：0=用户信息，1=角色分配 */
  selectedTabIndex = 0;

  /** 用户名列筛选关键词 */
  userNameFilter = '';

  /** 用户名列筛选弹出框是否可见 */
  userNameFilterVisible = false;

  /** 所有可分配的角色 */
  assignableRoles: IdentityRoleDto[] = [];

  /** 角色勾选状态映射 */
  roleCheckedMap: { [roleName: string]: boolean } = {};

  /** 当前登录用户ID */
  currentUserId = '';

  /** 部门下拉列表数据 */
  departments: DepartmentDto[] = [];

  /** 部门筛选项列表 */
  departmentFilters: { text: string; value: string }[] = [];

  /** 部门筛选值 */
  departmentFilter: string | null = null;

  /** 部门筛选函数（前端过滤） */
  departmentFilterFn = (selectedValues: string[], item: IdentityUserDto) => selectedValues.includes((item as any).extraProperties?.DepartmentId ?? '');

  /** 按用户名排序函数 */
  sortByUserName = (a: IdentityUserDto, b: IdentityUserDto) => (a.userName ?? '').localeCompare(b.userName ?? '');

  /** 按邮箱排序函数 */
  sortByEmail = (a: IdentityUserDto, b: IdentityUserDto) => (a.email ?? '').localeCompare(b.email ?? '');

  /** 按手机号排序函数 */
  sortByPhone = (a: IdentityUserDto, b: IdentityUserDto) => (a.phoneNumber ?? '').localeCompare(b.phoneNumber ?? '');

  constructor(public readonly list: ListService, private userService: IdentityUserService, private roleService: IdentityRoleService, private departmentService: DepartmentService, private fb: FormBuilder, private confirmation: ConfirmationService, private configState: ConfigStateService) {}

  ngOnInit() {
    /* 获取当前登录用户ID */
    this.currentUserId = this.configState.getDeep('currentUser.id') || '';

    /* 数据查询流 */
    const streamCreator = (query: any) => this.userService.getList({ ...query, filter: this.userNameFilter || undefined });
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.users = response;
      this.loading = false;
    });

    /* 加载可分配角色列表 */
    this.userService.getAssignableRoles().subscribe((res) => {
      this.assignableRoles = res.items ?? [];
    });

    /* 加载部门列表 */
    this.departmentService.getList({ maxResultCount: 1000 }).subscribe((res) => {
      this.departments = res.items ?? [];
      this.departmentFilters = this.departments.map((d) => ({ text: d.name ?? '', value: d.id ?? '' }));
    });
  }

  /** 用户名列筛选确定 */
  onUserNameFilterSearch() {
    this.userNameFilterVisible = false;
    this.list.get();
  }

  /** 用户名列筛选重置 */
  onUserNameFilterReset() {
    this.userNameFilter = '';
    this.userNameFilterVisible = false;
    this.list.get();
  }

  /** 打开新建用户弹窗 */
  createUser() {
    this.selectedUser = {} as IdentityUserDto;
    this.isNew = true;
    this.selectedTabIndex = 0;
    this.roleCheckedMap = {};
    this.assignableRoles.forEach((r) => (this.roleCheckedMap[r.name!] = false));
    this.buildForm();
    this.isModalOpen = true;
  }

  /** 打开编辑用户弹窗 */
  editUser(id: string) {
    forkJoin([this.userService.get(id), this.userService.getRoles(id)]).subscribe(([user, rolesResult]) => {
      this.selectedUser = user;
      this.isNew = false;
      this.selectedTabIndex = 0;
      const userRoles = rolesResult.items ?? [];
      this.roleCheckedMap = {};
      this.assignableRoles.forEach((r) => {
        this.roleCheckedMap[r.name!] = userRoles.some((ur) => ur.name === r.name);
      });
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /** 构建表单：新建密码必填，编辑密码可选 */
  buildForm() {
    this.form = this.fb.group({
      userName: [this.selectedUser.userName || '', [Validators.required, Validators.maxLength(256)]],
      password: ['', this.isNew ? [Validators.required, Validators.maxLength(128)] : [Validators.maxLength(128)]],
      name: [this.selectedUser.name || '', Validators.maxLength(64)],
      surname: [this.selectedUser.surname || '', Validators.maxLength(64)],
      email: [this.selectedUser.email || '', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phoneNumber: [this.selectedUser.phoneNumber || '', Validators.maxLength(16)],
      openId: [(this.selectedUser as any).extraProperties?.OpenId || '', Validators.maxLength(128)],
      departmentId: [(this.selectedUser as any).extraProperties?.DepartmentId || null],
      isActive: [this.selectedUser.id ? this.selectedUser.isActive : true],
      lockoutEnabled: [this.selectedUser.id ? this.selectedUser.lockoutEnabled : true],
    });
  }

  /** 保存用户 */
  save() {
    if (this.form.invalid) return;
    const val = this.form.value;
    const roleNames = this.assignableRoles.filter((r) => this.roleCheckedMap[r.name!]).map((r) => r.name!);

    if (this.selectedUser.id) {
      const dto: any = {
        userName: val.userName,
        name: val.name,
        surname: val.surname,
        email: val.email,
        phoneNumber: val.phoneNumber,
        isActive: val.isActive,
        lockoutEnabled: val.lockoutEnabled,
        roleNames,
        concurrencyStamp: this.selectedUser.concurrencyStamp,
        extraProperties: { OpenId: val.openId, DepartmentId: val.departmentId || null },
      };
      if (val.password) dto.password = val.password;
      this.userService.update(this.selectedUser.id, dto).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    } else {
      const dto: any = {
        userName: val.userName,
        password: val.password,
        name: val.name,
        surname: val.surname,
        email: val.email,
        phoneNumber: val.phoneNumber,
        isActive: val.isActive,
        lockoutEnabled: val.lockoutEnabled,
        roleNames,
        extraProperties: { OpenId: val.openId, DepartmentId: val.departmentId || null },
      };
      this.userService.create(dto).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    }
  }

  /** 删除用户（不能删自己） */
  deleteUser(user: IdentityUserDto) {
    if (user.id === this.currentUserId) return;
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.userService.delete(user.id!).subscribe(() => this.list.get());
      }
    });
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

  /** 导出当前条件数据（分页批量拉取） */
  async exportCurrent() {
    this.loading = true;
    try {
      const data = await fetchAllPaged<IdentityUserDto>((skip, take) => this.userService.getList({ filter: this.userNameFilter || undefined, skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '用户名': d.userName ?? '', '部门': this.getDepartmentName((d as any).extraProperties?.DepartmentId), '邮箱': d.email ?? '', '手机号码': d.phoneNumber ?? '', 'OpenId': (d as any).extraProperties?.OpenId ?? '', '启用': d.isActive ? '是' : '否' }));
      exportToXlsx(rows, '用户_当前条件');
    } finally {
      this.loading = false;
    }
  }

  /** 导出所有数据（分页批量拉取） */
  async exportAll() {
    this.loading = true;
    try {
      const data = await fetchAllPaged<IdentityUserDto>((skip, take) => this.userService.getList({ skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '用户名': d.userName ?? '', '部门': this.getDepartmentName((d as any).extraProperties?.DepartmentId), '邮箱': d.email ?? '', '手机号码': d.phoneNumber ?? '', 'OpenId': (d as any).extraProperties?.OpenId ?? '', '启用': d.isActive ? '是' : '否' }));
      exportToXlsx(rows, '用户_全部');
    } finally {
      this.loading = false;
    }
  }

  /**
   * 根据部门ID获取部门名称
   * @param departmentId 部门ID
   * @returns 部门名称
   */
  getDepartmentName(departmentId: string | null | undefined): string {
    if (!departmentId) return '';
    const dept = this.departments.find((d) => d.id === departmentId);
    return dept?.name ?? '';
  }
}

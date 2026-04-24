import { ListService, PagedResultDto } from '@abp/ng.core';
import { TenantService } from '@abp/ng.tenant-management/proxy';
import { TenantDto } from '@abp/ng.tenant-management/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { fetchAllPaged, exportToXlsx } from '../shared/export-xlsx';

/** 租户管理列表组件 */
@Component({
  standalone: false,
  selector: 'app-tenant',
  templateUrl: './tenant.component.html',
  providers: [ListService],
})
export class TenantComponent implements OnInit {
  /** 租户分页数据 */
  tenants = { items: [], totalCount: 0 } as PagedResultDto<TenantDto>;

  /** 当前选中的租户 */
  selectedTenant = {} as TenantDto;

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

  /** 名称列筛选关键词 */
  nameFilter = '';

  /** 名称列筛选弹出框是否可见 */
  nameFilterVisible = false;

  /** 功能管理弹窗是否可见 */
  featureVisible = false;

  /** 功能管理的providerKey（租户ID） */
  featureProviderKey = '';

  /** 按名称排序函数 */
  sortByName = (a: TenantDto, b: TenantDto) => (a.name ?? '').localeCompare(b.name ?? '');

  constructor(public readonly list: ListService, private tenantService: TenantService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 数据查询流 */
    const streamCreator = (query: any) => this.tenantService.getList({ ...query, filter: this.nameFilter || undefined });
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.tenants = response;
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

  /** 打开新建租户弹窗 */
  createTenant() {
    this.selectedTenant = {} as TenantDto;
    this.isNew = true;
    this.buildForm();
    this.isModalOpen = true;
  }

  /** 打开编辑租户弹窗 */
  editTenant(id: string) {
    this.tenantService.get(id).subscribe((tenant) => {
      this.selectedTenant = tenant;
      this.isNew = false;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /** 构建表单：新建包含管理员邮箱和密码，编辑只有名称 */
  buildForm() {
    if (this.isNew) {
      this.form = this.fb.group({
        name: ['', [Validators.required, Validators.maxLength(256)]],
        adminEmailAddress: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
        adminPassword: ['', [Validators.required, Validators.maxLength(128)]],
      });
    } else {
      this.form = this.fb.group({
        name: [this.selectedTenant.name || '', [Validators.required, Validators.maxLength(256)]],
      });
    }
  }

  /** 保存租户 */
  save() {
    if (this.form.invalid) return;
    if (this.selectedTenant.id) {
      this.tenantService.update(this.selectedTenant.id, { name: this.form.value.name, concurrencyStamp: this.selectedTenant.concurrencyStamp } as any).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    } else {
      this.tenantService.create(this.form.value).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    }
  }

  /** 删除租户 */
  deleteTenant(tenant: TenantDto) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.tenantService.delete(tenant.id!).subscribe(() => this.list.get());
      }
    });
  }

  /** 打开功能管理弹窗 */
  openFeatures(tenant: TenantDto) {
    this.featureProviderKey = tenant.id!;
    this.featureVisible = true;
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
      const data = await fetchAllPaged<TenantDto>((skip, take) => this.tenantService.getList({ filter: this.nameFilter || undefined, skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '租户名称': d.name ?? '' }));
      exportToXlsx(rows, '租户_当前条件');
    } finally {
      this.loading = false;
    }
  }

  /** 导出所有数据（分页批量拉取） */
  async exportAll() {
    this.loading = true;
    try {
      const data = await fetchAllPaged<TenantDto>((skip, take) => this.tenantService.getList({ skipCount: skip, maxResultCount: take } as any));
      const rows = data.map((d) => ({ '租户名称': d.name ?? '' }));
      exportToXlsx(rows, '租户_全部');
    } finally {
      this.loading = false;
    }
  }
}

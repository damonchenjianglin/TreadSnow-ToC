import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { DepartmentService, DepartmentDto } from '../proxy/departments';

/**
 * 部门管理组件
 * 负责部门的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-department',
  templateUrl: './department.component.html',
  providers: [ListService],
})
export class DepartmentComponent implements OnInit {
  /** 部门分页数据 */
  departments = { items: [], totalCount: 0 } as PagedResultDto<DepartmentDto>;

  /** 所有部门（用于上级部门下拉） */
  allDepartments: DepartmentDto[] = [];

  /** 当前选中的部门（编辑时使用） */
  selectedDepartment = {} as DepartmentDto;

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

  /** 按编号排序函数 */
  sortByNo = (a: DepartmentDto, b: DepartmentDto) => a.no - b.no;

  /** 按名称排序函数 */
  sortByName = (a: DepartmentDto, b: DepartmentDto) => (a.name ?? '').localeCompare(b.name ?? '');

  constructor(public readonly list: ListService, private departmentService: DepartmentService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    const streamCreator = (query: any) => this.departmentService.getList({ ...query, name: this.nameFilter || undefined });
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.departments = response;
      this.loading = false;
    });
    this.loadAllDepartments();
  }

  /** 加载所有部门（用于上级部门下拉选择） */
  private loadAllDepartments() {
    this.departmentService.getTree().subscribe((res) => {
      this.allDepartments = res;
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

  /** 打开新建部门弹窗 */
  createDepartment() {
    this.selectedDepartment = {} as DepartmentDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑部门弹窗
   * @param id 部门ID
   */
  editDepartment(id: string) {
    this.departmentService.get(id).subscribe((dept) => {
      this.selectedDepartment = dept;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除部门（弹出确认框）
   * @param id 部门ID
   */
  deleteDepartment(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.departmentService.delete(id).subscribe(() => {
          this.list.get();
          this.loadAllDepartments();
        });
      }
    });
  }

  /** 构建表单 */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedDepartment.name || '', [Validators.required, Validators.maxLength(64)]],
      parentDepartmentId: [this.selectedDepartment.parentDepartmentId || null],
    });
  }

  /** 保存部门 */
  save() {
    if (this.form.invalid) return;
    const request = this.selectedDepartment.id
      ? this.departmentService.update(this.selectedDepartment.id, this.form.value)
      : this.departmentService.create(this.form.value);
    request.subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
      this.loadAllDepartments();
    });
  }

  /** 关闭弹窗 */
  closeModal() {
    this.isModalOpen = false;
  }

  /**
   * 分页页码变更
   * @param page 新页码
   */
  onPageIndexChange(page: number) {
    this.currentPage = page;
    this.loading = true;
    this.list.page = page - 1;
  }

  /**
   * 分页每页条数变更
   * @param size 每页条数
   */
  onPageSizeChange(size: number) {
    this.loading = true;
    this.list.maxResultCount = size;
  }
}

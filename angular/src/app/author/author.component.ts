import { ListService, PagedResultDto, RestService } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { AuthorService, AuthorDto, CreateAuthorDto, UpdateAuthorDto } from '../proxy/authors';

/**
 * 作者列表组件
 * 负责作者的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-author',
  templateUrl: './author.component.html',
  styleUrls: ['./author.component.scss'],
  providers: [ListService],
})
export class AuthorComponent implements OnInit {
  /** 作者分页数据 */
  author = { items: [], totalCount: 0 } as PagedResultDto<AuthorDto>;

  /** 当前选中的作者（编辑时使用） */
  selectedAuthor = {} as AuthorDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 弹窗是否可见 */
  isModalOpen = false;

  /** 数据加载状态 */
  loading = false;

  /** 搜索关键词 */
  searchText = '';

  /** 当前页码 */
  currentPage = 1;

  /** 按名称排序函数 */
  sortByName = (a: AuthorDto, b: AuthorDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按出生日期排序函数 */
  sortByBirthDate = (a: AuthorDto, b: AuthorDto) => new Date(a.birthDate ?? '').getTime() - new Date(b.birthDate ?? '').getTime();

  constructor(public readonly list: ListService, private authorService: AuthorService, private restService: RestService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 定义数据查询流，支持按名称筛选 */
    const authorStreamCreator = (query: any) =>
      this.restService.request<any, PagedResultDto<AuthorDto>>({
        method: 'GET',
        url: '/api/app/author',
        params: { sorting: query.sorting, skipCount: query.skipCount, maxResultCount: query.maxResultCount, filter: this.searchText || undefined },
      });

    this.list.hookToQuery(authorStreamCreator).subscribe((response) => {
      this.author = response;
      this.loading = false;
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedAuthor.name || '', Validators.required],
      birthDate: [this.selectedAuthor.birthDate ? new Date(this.selectedAuthor.birthDate) : null, Validators.required],
      shortBio: [this.selectedAuthor.shortBio || ''],
    });
  }

  /** 打开新建作者弹窗 */
  createAuthor() {
    this.selectedAuthor = {} as CreateAuthorDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑作者弹窗
   * @param id 作者ID
   */
  editAuthor(id: string) {
    this.authorService.get(id).subscribe((author) => {
      this.selectedAuthor = author as UpdateAuthorDto;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除作者（弹出确认框）
   * @param id 作者ID
   */
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.authorService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 保存作者（新建或更新） */
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedAuthor.id
      ? this.authorService.update(this.selectedAuthor.id, this.form.value)
      : this.authorService.create(this.form.value);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
    });
  }

  /** 关闭弹窗 */
  closeModal() {
    this.isModalOpen = false;
  }

  /**
   * 分页页码变更回调
   * @param page 新页码
   */
  onPageIndexChange(page: number) {
    this.currentPage = page;
    this.loading = true;
    this.list.page = page - 1;
  }

  /**
   * 分页每页条数变更回调
   * @param size 每页条数
   */
  onPageSizeChange(size: number) {
    this.loading = true;
    this.list.maxResultCount = size;
  }

  /**
   * 按名称搜索回调，触发列表重新查询
   * @param keyword 搜索关键词
   */
  onSearch(keyword: string) {
    this.searchText = keyword;
    this.list.get();
  }

  /** 导出当前条件数据 */
  exportCurrent() {
    // TODO: 对接后端导出接口，传入当前搜索条件
    console.warn('导出当前条件数据，搜索关键词:', this.searchText);
  }

  /** 导出所有数据 */
  exportAll() {
    // TODO: 对接后端导出接口，不传筛选条件
    console.warn('导出所有数据');
  }
}

import { ListService, PagedResultDto, RestService } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { BookService, BookDto, bookTypeOptions } from '../proxy/books';

/**
 * 图书列表组件
 * 负责图书的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-book',
  templateUrl: './book.component.html',
  styleUrls: ['./book.component.scss'],
  providers: [ListService],
})
export class BookComponent implements OnInit {
  /** 图书分页数据 */
  book = { items: [], totalCount: 0 } as PagedResultDto<BookDto>;

  /** 当前选中的图书（编辑时使用） */
  selectedBook = {} as BookDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 图书类型选项列表 */
  bookTypes = bookTypeOptions;

  /** 弹窗是否可见 */
  isModalOpen = false;

  /** 作者下拉列表数据 */
  authors: { id?: string; name?: string }[] = [];

  /** 作者名称筛选项列表（用于表格漏斗筛选） */
  authorFilters: { text: string; value: string }[] = [];

  /** 作者名称筛选函数 */
  authorFilterFn = (selectedValues: string[], item: BookDto) => selectedValues.includes(item.authorName ?? '');

  /** 数据加载状态 */
  loading = false;

  /** 搜索关键词 */
  searchText = '';

  /** 当前页码 */
  currentPage = 1;

  /** 按名称排序函数 */
  sortByName = (a: BookDto, b: BookDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按作者名称排序函数 */
  sortByAuthor = (a: BookDto, b: BookDto) => (a.authorName ?? '').localeCompare(b.authorName ?? '');

  /** 按类型排序函数 */
  sortByType = (a: BookDto, b: BookDto) => a.type - b.type;

  /** 按出版日期排序函数 */
  sortByDate = (a: BookDto, b: BookDto) => new Date(a.publishDate ?? '').getTime() - new Date(b.publishDate ?? '').getTime();

  /** 按价格排序函数 */
  sortByPrice = (a: BookDto, b: BookDto) => a.price - b.price;

  constructor(public readonly list: ListService, private bookService: BookService, private restService: RestService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 定义数据查询流，支持按名称筛选 */
    const bookStreamCreator = (query: any) =>
      this.restService.request<any, PagedResultDto<BookDto>>({
        method: 'GET',
        url: '/api/app/book',
        params: { sorting: query.sorting, skipCount: query.skipCount, maxResultCount: query.maxResultCount, name: this.searchText || undefined },
      });

    this.list.hookToQuery(bookStreamCreator).subscribe((response) => {
      this.book = response;
      this.loading = false;
    });

    // 加载作者列表
    this.loadAuthors();
  }

  /** 加载作者下拉列表数据 */
  loadAuthors() {
    this.bookService.getAuthorLookup().subscribe((response) => {
      this.authors = response.items ?? [];
      this.authorFilters = (response.items ?? []).map((a) => ({ text: a.name ?? '', value: a.name ?? '' }));
    });
  }

  /** 打开新建图书弹窗 */
  createBook() {
    this.selectedBook = {} as BookDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑图书弹窗
   * @param id 图书ID
   */
  editBook(id: string) {
    this.bookService.get(id).subscribe((book) => {
      this.selectedBook = book;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除图书（弹出确认框）
   * @param id 图书ID
   */
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.bookService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedBook.name || '', Validators.required],
      type: [this.selectedBook.type ?? null, Validators.required],
      publishDate: [this.selectedBook.publishDate ? new Date(this.selectedBook.publishDate) : null, Validators.required],
      price: [this.selectedBook.price || null, Validators.required],
      authorId: [this.selectedBook.authorId || null, Validators.required],
    });
  }

  /** 保存图书（新建或更新） */
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedBook.id
      ? this.bookService.update(this.selectedBook.id, this.form.value)
      : this.bookService.create(this.form.value);

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

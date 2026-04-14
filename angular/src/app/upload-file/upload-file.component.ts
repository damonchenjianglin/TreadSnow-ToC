import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { UploadFileService, UploadFileDto } from '../proxy/upload-files';

/**
 * 附件列表组件
 * 负责附件的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-upload-file',
  templateUrl: './upload-file.component.html',
  styleUrls: ['./upload-file.component.scss'],
  providers: [ListService],
})
export class UploadFileComponent implements OnInit {
  /** 附件分页数据 */
  uploadFile = { items: [], totalCount: 0 } as PagedResultDto<UploadFileDto>;

  /** 当前选中的附件（编辑时使用） */
  selectedUploadFile = {} as UploadFileDto;

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
  sortByName = (a: UploadFileDto, b: UploadFileDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按实体名称排序函数 */
  sortByEntityName = (a: UploadFileDto, b: UploadFileDto) => (a.entityName ?? '').localeCompare(b.entityName ?? '');

  /** 按记录ID排序函数 */
  sortByRecordId = (a: UploadFileDto, b: UploadFileDto) => (a.recordId ?? '').localeCompare(b.recordId ?? '');

  /** 按文件类型排序函数 */
  sortByType = (a: UploadFileDto, b: UploadFileDto) => (a.type ?? '').localeCompare(b.type ?? '');

  /** 按文件路径排序函数 */
  sortByPath = (a: UploadFileDto, b: UploadFileDto) => (a.path ?? '').localeCompare(b.path ?? '');

  constructor(public readonly list: ListService, private uploadFileService: UploadFileService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 定义数据查询流，列表页查所有附件，不传entityName和recordId */
    const uploadFileStreamCreator = (query: any) =>
      this.uploadFileService.getList(query);

    this.list.hookToQuery(uploadFileStreamCreator).subscribe((response) => {
      this.uploadFile = response;
      this.loading = false;
    });
  }

  /** 打开新建附件弹窗 */
  createUploadFile() {
    this.selectedUploadFile = {} as UploadFileDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑附件弹窗
   * @param id 附件ID
   */
  editUploadFile(id: string) {
    this.uploadFileService.get(id).subscribe((file) => {
      this.selectedUploadFile = file;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除附件（弹出确认框）
   * @param id 附件ID
   */
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.uploadFileService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      entityName: [this.selectedUploadFile.entityName || '', Validators.required],
      recordId: [this.selectedUploadFile.recordId || '', Validators.required],
      name: [this.selectedUploadFile.name || '', Validators.required],
      type: [this.selectedUploadFile.type || '', Validators.required],
      path: [this.selectedUploadFile.path || '', Validators.required],
    });
  }

  /** 保存附件（新建或更新） */
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedUploadFile.id
      ? this.uploadFileService.update(this.selectedUploadFile.id, this.form.value)
      : this.uploadFileService.create(this.form.value);

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

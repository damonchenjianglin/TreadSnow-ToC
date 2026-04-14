import { ListService, PagedResultDto } from '@abp/ng.core';
import * as XLSX from 'xlsx';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { PetService, PetDto } from '../proxy/pets';

/**
 * 宠物列表组件
 * 负责宠物的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-pet',
  templateUrl: './pet.component.html',
  styleUrls: ['./pet.component.scss'],
  providers: [ListService],
})
export class PetComponent implements OnInit {
  /** 宠物分页数据 */
  pet = { items: [], totalCount: 0 } as PagedResultDto<PetDto>;

  /** 当前选中的宠物（编辑时使用） */
  selectedPet = {} as PetDto;

  /** 表单对象 */
  form!: FormGroup;

  /** 弹窗是否可见 */
  isModalOpen = false;

  /** 数据加载状态 */
  loading = false;

  /** 当前页码 */
  currentPage = 1;

  /** 会员下拉列表数据（用于选择主人） */
  accounts: { id?: string; name?: string }[] = [];

  /** 主人筛选项列表（用于表格漏斗筛选） */
  accountFilters: { text: string; value: string }[] = [];

  /** 主人筛选函数 */
  accountFilterFn = (selectedValues: string[], item: PetDto) => selectedValues.includes(item.accountName ?? '');

  /** 名称列筛选关键词 */
  nameFilter = '';

  /** 名称列筛选弹出框是否可见 */
  nameFilterVisible = false;

  /** 按编号排序函数 */
  sortByNo = (a: PetDto, b: PetDto) => a.no - b.no;

  /** 按名称排序函数 */
  sortByName = (a: PetDto, b: PetDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按主人名称排序函数 */
  sortByAccountName = (a: PetDto, b: PetDto) => (a.accountName ?? '').localeCompare(b.accountName ?? '');

  constructor(public readonly list: ListService, private petService: PetService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    /** 定义数据查询流，传入name模糊筛选 */
    const petStreamCreator = (query: any) =>
      this.petService.getList({ ...query, name: this.nameFilter || undefined });

    this.list.hookToQuery(petStreamCreator).subscribe((response) => {
      this.pet = response;
      this.loading = false;
    });

    // 加载会员下拉列表
    this.loadAccounts();
  }

  /** 加载会员下拉列表数据 */
  loadAccounts() {
    this.petService.getAccountLookup().subscribe((response) => {
      this.accounts = response.items ?? [];
      this.accountFilters = (response.items ?? []).map((a) => ({ text: a.name ?? '', value: a.name ?? '' }));
    });
  }

  /** 名称列筛选确定（关闭弹出框并刷新列表） */
  onNameFilterSearch() {
    this.nameFilterVisible = false;
    this.list.get();
  }

  /** 名称列筛选重置（清空关键词并刷新列表） */
  onNameFilterReset() {
    this.nameFilter = '';
    this.nameFilterVisible = false;
    this.list.get();
  }

  /** 打开新建宠物弹窗 */
  createPet() {
    this.selectedPet = {} as PetDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑宠物弹窗
   * @param id 宠物ID
   */
  editPet(id: string) {
    this.petService.get(id).subscribe((pet) => {
      this.selectedPet = pet;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除宠物（弹出确认框）
   * @param id 宠物ID
   */
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.petService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedPet.name || '', Validators.required],
      accountId: [this.selectedPet.accountId || null, Validators.required],
    });
  }

  /** 保存宠物（新建或更新） */
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedPet.id
      ? this.petService.update(this.selectedPet.id, this.form.value)
      : this.petService.create(this.form.value);

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
   * 导出当前条件数据（调后端不分页接口，传当前筛选条件，导出全部匹配数据）
   */
  exportCurrent() {
    this.petService.getAllList(this.nameFilter || undefined).subscribe((data) => {
      this.downloadXlsx(data, '宠物_当前条件');
    });
  }

  /**
   * 导出所有数据（不传任何筛选条件）
   */
  exportAll() {
    this.petService.getAllList().subscribe((data) => {
      this.downloadXlsx(data, '宠物_全部');
    });
  }

  /**
   * 将数据导出为xlsx文件并下载
   * @param data 导出数据
   * @param filename 文件名前缀
   */
  private downloadXlsx(data: PetDto[], filename: string) {
    const rows = data.map((d) => ({ '编号': d.no, '名称': d.name ?? '', '主人': d.accountName ?? '' }));
    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, '宠物');
    XLSX.writeFile(wb, `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}

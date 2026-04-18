import { ConfigStateService, ListService, PagedResultDto } from '@abp/ng.core';
import * as XLSX from 'xlsx';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { AccountService, AccountDto, UserLookupDto, TeamLookupDto } from '../proxy/accounts';

/**
 * 会员列表组件
 * 负责会员的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.scss'],
  providers: [ListService],
})
export class AccountComponent implements OnInit {
  /** 会员分页数据 */
  account = { items: [], totalCount: 0 } as PagedResultDto<AccountDto>;

  /** 当前选中的会员（编辑时使用） */
  selectedAccount = {} as AccountDto;

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

  /** 负责人筛选值 */
  ownerFilter: string | null = null;

  /** 当前登录用户ID */
  currentUserId = '';

  /** 用户下拉列表数据（用于选择负责人） */
  owners: UserLookupDto[] = [];

  /** 团队下拉列表数据（用于选择负责团队） */
  teams: TeamLookupDto[] = [];

  /** 负责人筛选项列表（用于表格漏斗筛选） */
  ownerFilters: { text: string; value: string }[] = [];

  /** 按编号排序函数 */
  sortByNo = (a: AccountDto, b: AccountDto) => a.no - b.no;

  /** 按名称排序函数 */
  sortByName = (a: AccountDto, b: AccountDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按手机号码排序函数 */
  sortByPhone = (a: AccountDto, b: AccountDto) => (a.phone ?? '').localeCompare(b.phone ?? '');

  /** 按邮箱排序函数 */
  sortByEmail = (a: AccountDto, b: AccountDto) => (a.email ?? '').localeCompare(b.email ?? '');

  /** 按OpenId排序函数 */
  sortByOpenId = (a: AccountDto, b: AccountDto) => (a.openId ?? '').localeCompare(b.openId ?? '');

  /** 按描述排序函数 */
  sortByDescription = (a: AccountDto, b: AccountDto) => (a.description ?? '').localeCompare(b.description ?? '');

  /** 按负责人排序函数 */
  sortByOwnerName = (a: AccountDto, b: AccountDto) => (a.ownerName ?? '').localeCompare(b.ownerName ?? '');

  /** 按负责团队排序函数 */
  sortByOwnerTeamName = (a: AccountDto, b: AccountDto) => (a.ownerTeamName ?? '').localeCompare(b.ownerTeamName ?? '');

  /** 按创建时间排序函数 */
  sortByCreationTime = (a: AccountDto, b: AccountDto) => (a.creationTime ?? '').localeCompare(b.creationTime ?? '');

  constructor(public readonly list: ListService, private accountService: AccountService, private fb: FormBuilder, private confirmation: ConfirmationService, private configState: ConfigStateService) {}

  ngOnInit() {
    this.currentUserId = this.configState.getDeep('currentUser.id') || '';

    /** 定义数据查询流，传入name模糊筛选和负责人筛选 */
    const accountStreamCreator = (query: any) =>
      this.accountService.getList({ ...query, name: this.nameFilter || undefined, ownerId: this.ownerFilter || undefined });

    this.list.hookToQuery(accountStreamCreator).subscribe((response) => {
      this.account = response;
      this.loading = false;
    });

    this.loadLookups();
  }

  /** 加载负责人和团队下拉数据 */
  loadLookups() {
    this.accountService.getOwnerLookup().subscribe((res) => {
      this.owners = res.items ?? [];
      this.ownerFilters = this.owners.map((o) => ({ text: o.name ?? '', value: o.id ?? '' }));
    });
    this.accountService.getTeamLookup().subscribe((res) => {
      this.teams = res.items ?? [];
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

  /**
   * 负责人列筛选变更回调
   * @param selectedValues 选中的筛选值
   */
  onOwnerFilterChange(selectedValues: string[]) {
    this.ownerFilter = selectedValues.length > 0 ? selectedValues[0] : null;
    this.list.get();
  }

  /** 打开新建会员弹窗 */
  createAccount() {
    this.selectedAccount = {} as AccountDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑会员弹窗
   * @param id 会员ID
   */
  editAccount(id: string) {
    this.accountService.get(id).subscribe((account) => {
      this.selectedAccount = account;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除会员（弹出确认框）
   * @param id 会员ID
   */
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.accountService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedAccount.name || '', Validators.required],
      phone: [this.selectedAccount.phone || '', Validators.required],
      email: [this.selectedAccount.email || '', Validators.required],
      openId: [this.selectedAccount.openId || '', Validators.required],
      description: [this.selectedAccount.description || ''],
      ownerId: [this.selectedAccount.ownerId || this.currentUserId || null],
      ownerTeamId: [this.selectedAccount.ownerTeamId || null],
    });
  }

  /** 保存会员（新建或更新） */
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedAccount.id
      ? this.accountService.update(this.selectedAccount.id, this.form.value)
      : this.accountService.create(this.form.value);

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
    this.accountService.getAllList(this.nameFilter || undefined).subscribe((data) => {
      this.downloadXlsx(data, '会员_当前条件');
    });
  }

  /**
   * 导出所有数据（不传任何筛选条件）
   */
  exportAll() {
    this.accountService.getAllList().subscribe((data) => {
      this.downloadXlsx(data, '会员_全部');
    });
  }

  /**
   * 将数据导出为xlsx文件并下载
   * @param data 导出数据
   * @param filename 文件名前缀
   */
  private downloadXlsx(data: AccountDto[], filename: string) {
    const rows = data.map((d) => ({ '编号': d.no, '名称': d.name ?? '', '手机号码': d.phone ?? '', '邮箱': d.email ?? '', 'OpenId': d.openId ?? '', '描述': d.description ?? '', '负责人': d.ownerName ?? '', '负责团队': d.ownerTeamName ?? '' }));
    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, '会员');
    XLSX.writeFile(wb, `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}

import { ConfigStateService, ListService, PagedResultDto } from '@abp/ng.core';
import { exportToXlsx } from '../shared/export-xlsx';
import { Component, OnInit, AfterViewInit, ElementRef, Renderer2 } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { PetService, PetDto, UserLookupDto, TeamLookupDto } from '../proxy/pets';
import { NzMessageService } from 'ng-zorro-antd/message';

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
export class PetComponent implements OnInit, AfterViewInit {
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

  /** 当前登录用户ID */
  currentUserId = '';

  /** 会员下拉列表数据（用于选择主人） */
  accounts: { id?: string; name?: string }[] = [];

  /** 用户下拉列表数据（用于选择负责人） */
  owners: UserLookupDto[] = [];

  /** 团队下拉列表数据（用于选择负责团队） */
  teams: TeamLookupDto[] = [];

  /** 主人筛选项列表（用于表格漏斗筛选） */
  accountFilters: { text: string; value: string }[] = [];

  /** 主人筛选函数 */
  accountFilterFn = (selectedValues: string[], item: PetDto) => selectedValues.includes(item.accountName ?? '');

  /** 负责人筛选值 */
  ownerFilter: string | null = null;

  /** 负责人筛选项列表 */
  ownerFilters: { text: string; value: string }[] = [];

  /** 创建人筛选项列表 */
  creatorFilters: { text: string; value: string }[] = [];

  /** 创建人筛选函数（前端过滤） */
  creatorFilterFn = (selectedValues: string[], item: PetDto) => selectedValues.includes(item.creatorName ?? '');

  /** 修改人筛选项列表 */
  lastModifierFilters: { text: string; value: string }[] = [];

  /** 修改人筛选函数（前端过滤） */
  lastModifierFilterFn = (selectedValues: string[], item: PetDto) => selectedValues.includes(item.lastModifierName ?? '');

  /** 创建时间范围筛选值 */
  creationTimeRange: Date[] | null = null;

  /** 创建时间筛选弹出框是否可见 */
  creationTimeFilterVisible = false;

  /** 修改时间范围筛选值 */
  lastModificationTimeRange: Date[] | null = null;

  /** 修改时间筛选弹出框是否可见 */
  lastModificationTimeFilterVisible = false;

  /** 名称列筛选关键词 */
  nameFilter = '';

  /** 名称列筛选弹出框是否可见 */
  nameFilterVisible = false;

  /** 初始化表头列宽拖动手柄 */
  ngAfterViewInit(): void {
    this.setupColumnResize();
  }

  /** 为表头每个th（最后一列除外）添加拖动手柄 */
  private setupColumnResize(): void {
    setTimeout(() => {
      const tableEl = this.el.nativeElement.querySelector('nz-table');
      if (!tableEl) return;
      const ths = tableEl.querySelectorAll('.ant-table-thead th');
      const cols = tableEl.querySelectorAll('colgroup col');
      ths.forEach((th: HTMLElement, index: number) => {
        if (index >= ths.length - 1) return;
        const col = cols[index] as HTMLElement;
        if (!col) return;
        const handle = this.renderer.createElement('span');
        this.renderer.addClass(handle, 'col-resize-handle');
        this.renderer.appendChild(th, handle);
        this.renderer.listen(handle, 'mousedown', (e: MouseEvent) => {
          e.preventDefault();
          e.stopPropagation();
          const startX = e.clientX;
          const startWidth = th.offsetWidth;
          const onMouseMove = (ev: MouseEvent) => {
            const newWidth = Math.max(50, startWidth + ev.clientX - startX) + 'px';
            this.renderer.setStyle(col, 'width', newWidth);
            this.renderer.setStyle(col, 'min-width', newWidth);
          };
          const onMouseUp = () => {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
          };
          document.addEventListener('mousemove', onMouseMove);
          document.addEventListener('mouseup', onMouseUp);
        });
      });
    });
  }

  /** 按编号排序函数 */
  sortByNo = (a: PetDto, b: PetDto) => a.no - b.no;

  /** 按名称排序函数 */
  sortByName = (a: PetDto, b: PetDto) => (a.name ?? '').localeCompare(b.name ?? '');

  /** 按主人名称排序函数 */
  sortByAccountName = (a: PetDto, b: PetDto) => (a.accountName ?? '').localeCompare(b.accountName ?? '');

  /** 按负责人排序函数 */
  sortByOwnerName = (a: PetDto, b: PetDto) => (a.ownerName ?? '').localeCompare(b.ownerName ?? '');

  /** 按负责团队排序函数 */
  sortByOwnerTeamName = (a: PetDto, b: PetDto) => (a.ownerTeamName ?? '').localeCompare(b.ownerTeamName ?? '');

  /** 按创建时间排序函数 */
  sortByCreationTime = (a: PetDto, b: PetDto) => (a.creationTime ?? '').localeCompare(b.creationTime ?? '');

  /** 按创建人排序函数 */
  sortByCreatorName = (a: PetDto, b: PetDto) => (a.creatorName ?? '').localeCompare(b.creatorName ?? '');

  /** 按修改人排序函数 */
  sortByLastModifierName = (a: PetDto, b: PetDto) => (a.lastModifierName ?? '').localeCompare(b.lastModifierName ?? '');

  /** 按修改时间排序函数 */
  sortByLastModificationTime = (a: PetDto, b: PetDto) => (a.lastModificationTime ?? '').localeCompare(b.lastModificationTime ?? '');

  constructor(public readonly list: ListService, private petService: PetService, private fb: FormBuilder, private confirmation: ConfirmationService, private configState: ConfigStateService, private message: NzMessageService, private el: ElementRef, private renderer: Renderer2) {}

  ngOnInit() {
    this.currentUserId = this.configState.getDeep('currentUser.id') || '';

    /** 定义数据查询流，传入name模糊筛选和负责人筛选 */
    const petStreamCreator = (query: any) =>
      this.petService.getList({ ...query, name: this.nameFilter || undefined, ownerId: this.ownerFilter || undefined, startCreationTime: this.creationTimeRange?.[0]?.toISOString() || undefined, endCreationTime: this.creationTimeRange?.[1]?.toISOString() || undefined, startLastModificationTime: this.lastModificationTimeRange?.[0]?.toISOString() || undefined, endLastModificationTime: this.lastModificationTimeRange?.[1]?.toISOString() || undefined });

    this.list.hookToQuery(petStreamCreator).subscribe((response) => {
      this.pet = response;
      this.loading = false;
    });

    this.loadLookups();
  }

  /** 加载所有下拉列表数据 */
  loadLookups() {
    this.petService.getAccountLookup().subscribe((response) => {
      this.accounts = response.items ?? [];
      this.accountFilters = (response.items ?? []).map((a) => ({ text: a.name ?? '', value: a.name ?? '' }));
    });
    this.petService.getOwnerLookup().subscribe((res) => {
      this.owners = res.items ?? [];
      this.ownerFilters = this.owners.map((o) => ({ text: o.name ?? '', value: o.id ?? '' }));
      this.creatorFilters = this.owners.map((o) => ({ text: o.name ?? '', value: o.name ?? '' }));
      this.lastModifierFilters = this.owners.map((o) => ({ text: o.name ?? '', value: o.name ?? '' }));
    });
    this.petService.getTeamLookup().subscribe((res) => {
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

  /** 创建时间筛选确定 */
  onCreationTimeFilterSearch() {
    this.creationTimeFilterVisible = false;
    this.list.get();
  }

  /** 创建时间筛选重置 */
  onCreationTimeFilterReset() {
    this.creationTimeRange = null;
    this.creationTimeFilterVisible = false;
    this.list.get();
  }

  /** 修改时间筛选确定 */
  onLastModificationTimeFilterSearch() {
    this.lastModificationTimeFilterVisible = false;
    this.list.get();
  }

  /** 修改时间筛选重置 */
  onLastModificationTimeFilterReset() {
    this.lastModificationTimeRange = null;
    this.lastModificationTimeFilterVisible = false;
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
        this.petService.delete(id).subscribe({
          next: () => this.list.get(),
          error: (err: any) => {
            if (err?.status === 403 || err?.error?.error?.code === 'Volo.Authorization') {
              this.message.warning('您没有该记录的删除权限');
            }
          },
        });
      }
    });
  }

  /** 构建表单（新建或编辑时初始化表单字段） */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedPet.name || '', Validators.required],
      accountId: [this.selectedPet.accountId || null, Validators.required],
      ownerId: [this.selectedPet.ownerId || this.currentUserId || null],
      ownerTeamId: [this.selectedPet.ownerTeamId || null],
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

    request.subscribe({
      next: () => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
      },
      error: (err: any) => {
        if (err?.status === 403 || err?.error?.error?.code === 'Volo.Authorization') {
          this.message.warning('您没有该记录的编辑权限');
        }
      },
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

  /** 导出当前条件数据（调后端不分页接口，传当前筛选条件，导出全部匹配数据） */
  exportCurrent() {
    this.petService.getAllList(this.nameFilter || undefined).subscribe((data) => {
      const rows = data.map((d) => ({ '编号': d.no, '名称': d.name ?? '', '主人': d.accountName ?? '', '负责人': d.ownerName ?? '', '负责团队': d.ownerTeamName ?? '' }));
      exportToXlsx(rows, '宠物_当前条件');
    });
  }

  /** 导出所有数据（不传任何筛选条件） */
  exportAll() {
    this.petService.getAllList().subscribe((data) => {
      const rows = data.map((d) => ({ '编号': d.no, '名称': d.name ?? '', '主人': d.accountName ?? '', '负责人': d.ownerName ?? '', '负责团队': d.ownerTeamName ?? '' }));
      exportToXlsx(rows, '宠物_全部');
    });
  }
}

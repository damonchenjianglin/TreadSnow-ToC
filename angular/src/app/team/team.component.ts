import { ListService, PagedResultDto } from '@abp/ng.core';
import { IdentityRoleService, IdentityRoleDto } from '@abp/ng.identity/proxy';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { forkJoin } from 'rxjs';
import { TeamService, TeamDto, TeamRoleDto } from '../proxy/teams';
import { DepartmentService, DepartmentDto } from '../proxy/departments';

/**
 * 团队管理组件
 * 负责团队的增删改查及列表展示
 */
@Component({
  standalone: false,
  selector: 'app-team',
  templateUrl: './team.component.html',
  providers: [ListService],
})
export class TeamComponent implements OnInit {
  /** 团队分页数据 */
  teams = { items: [], totalCount: 0 } as PagedResultDto<TeamDto>;

  /** 所有部门（用于部门下拉） */
  allDepartments: DepartmentDto[] = [];

  /** 当前选中的团队 */
  selectedTeam = {} as TeamDto;

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

  /** 所有可分配的角色 */
  assignableRoles: IdentityRoleDto[] = [];

  /** 当前团队已有的角色ID集合 */
  teamRoleIds: Set<string> = new Set();

  /** 角色勾选状态映射 */
  roleCheckedMap: { [roleName: string]: boolean } = {};

  /** 按编号排序函数 */
  sortByNo = (a: TeamDto, b: TeamDto) => a.no - b.no;

  /** 按名称排序函数 */
  sortByName = (a: TeamDto, b: TeamDto) => (a.name ?? '').localeCompare(b.name ?? '');

  constructor(public readonly list: ListService, private teamService: TeamService, private departmentService: DepartmentService, private roleService: IdentityRoleService, private fb: FormBuilder, private confirmation: ConfirmationService) {}

  ngOnInit() {
    const streamCreator = (query: any) => this.teamService.getList({ ...query, name: this.nameFilter || undefined });
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.teams = response;
      this.loading = false;
    });
    this.loadDepartments();
    this.loadRoles();
  }

  /** 加载部门列表 */
  private loadDepartments() {
    this.departmentService.getTree().subscribe((res) => {
      this.allDepartments = res;
    });
  }

  /** 加载所有可分配角色 */
  private loadRoles() {
    this.roleService.getAllList().subscribe((res) => {
      this.assignableRoles = res.items ?? [];
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

  /** 打开新建团队弹窗 */
  createTeam() {
    this.selectedTeam = {} as TeamDto;
    this.teamRoleIds = new Set();
    this.roleCheckedMap = {};
    this.assignableRoles.forEach((r) => (this.roleCheckedMap[r.name!] = false));
    this.buildForm();
    this.isModalOpen = true;
  }

  /**
   * 打开编辑团队弹窗，同时加载团队角色
   * @param id 团队ID
   */
  editTeam(id: string) {
    forkJoin([this.teamService.get(id), this.teamService.getRoles(id)]).subscribe(([team, roles]) => {
      this.selectedTeam = team;
      this.teamRoleIds = new Set(roles.map((r) => r.roleId));
      this.roleCheckedMap = {};
      this.assignableRoles.forEach((r) => {
        this.roleCheckedMap[r.name!] = roles.some((tr) => tr.roleName === r.name);
      });
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  /**
   * 删除团队
   * @param id 团队ID
   */
  deleteTeam(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.teamService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  /** 构建表单 */
  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedTeam.name || '', [Validators.required, Validators.maxLength(64)]],
      departmentId: [this.selectedTeam.departmentId || null],
    });
  }

  /** 保存团队（含角色变更） */
  save() {
    if (this.form.invalid) return;
    const request = this.selectedTeam.id
      ? this.teamService.update(this.selectedTeam.id, this.form.value)
      : this.teamService.create(this.form.value);
    request.subscribe((team) => {
      const teamId = this.selectedTeam.id || team.id;
      this.saveRoles(teamId);
    });
  }

  /**
   * 保存团队角色变更（对比新旧差异，增删角色）
   * @param teamId 团队ID
   */
  private saveRoles(teamId: string) {
    const selectedRoleIds = new Set(
      this.assignableRoles.filter((r) => this.roleCheckedMap[r.name!]).map((r) => r.id!)
    );
    const toAdd = [...selectedRoleIds].filter((id) => !this.teamRoleIds.has(id));
    const toRemove = [...this.teamRoleIds].filter((id) => !selectedRoleIds.has(id));
    const ops = [
      ...toAdd.map((roleId) => this.teamService.addRole(teamId, roleId)),
      ...toRemove.map((roleId) => this.teamService.removeRole(teamId, roleId)),
    ];
    if (ops.length > 0) {
      forkJoin(ops).subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
      });
    } else {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
    }
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

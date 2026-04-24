import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { DataPermissionConfigDto, RoleDataPermissionService } from '../../proxy/data-permissions';

/** 权限等级配置（颜色和标签） */
const LEVEL_CONFIG = [
  { level: 0, color: '#d9d9d9', label: '无权限' },
  { level: 1, color: '#52c41a', label: '个人' },
  { level: 2, color: '#1890ff', label: '部门' },
  { level: 3, color: '#fa8c16', label: '部门及下级' },
  { level: 4, color: '#f5222d', label: '组织' },
];

/** 受数据权限控制的实体列表 */
const ENTITY_LIST = [
  { entityName: 'account', labelKey: '::Menu:Accounts' },
  { entityName: 'pet', labelKey: '::Menu:Pets' },
  { entityName: 'uploadFile', labelKey: '::Menu:UploadFiles' },
  { entityName: 'opportunity', labelKey: '::Menu:Opportunities' },
];

/**
 * 数据权限配置组件（嵌入tabs中使用）
 * 用于在角色管理中配置各实体的读/写/删除权限等级
 */
@Component({
  standalone: false,
  selector: 'app-data-permission-modal',
  templateUrl: './data-permission-modal.component.html',
  styleUrls: ['./data-permission-modal.component.scss'],
})
export class DataPermissionModalComponent implements OnChanges {
  /** 角色Id */
  @Input() roleId = '';

  /** 保存中状态 */
  @Input() saving = false;

  /** 保存完成事件 */
  @Output() saved = new EventEmitter<void>();

  /** 保存失败事件 */
  @Output() saveFailed = new EventEmitter<void>();

  /** 权限等级配置常量 */
  levelConfig = LEVEL_CONFIG;

  /** 实体列表 */
  entityList = ENTITY_LIST;

  /** 当前配置数据 */
  configs: DataPermissionConfigDto[] = [];

  /** 数据加载中 */
  loading = false;

  constructor(private service: RoleDataPermissionService) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['roleId'] && this.roleId) {
      this.loadData();
    }
  }

  /** 加载角色的数据权限配置 */
  loadData() {
    if (!this.roleId) return;
    this.loading = true;
    this.service.get(this.roleId).subscribe({
      next: (res) => {
        this.configs = this.entityList.map((entity) => {
          const existing = res.configs?.find((c) => c.entityName === entity.entityName);
          return {
            entityName: entity.entityName,
            readLevel: existing?.readLevel ?? 0,
            writeLevel: existing?.writeLevel ?? 0,
            deleteLevel: existing?.deleteLevel ?? 0,
          };
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  /**
   * 点击权限等级按钮，循环切换0→1→2→3→4→0
   * @param config 当前配置项
   * @param field 字段名（readLevel/writeLevel/deleteLevel）
   */
  cycleLevel(config: DataPermissionConfigDto, field: 'readLevel' | 'writeLevel' | 'deleteLevel') {
    config[field] = (config[field] + 1) % 5;
  }

  /**
   * 根据权限等级获取颜色
   * @param level 权限等级
   * @returns 颜色值
   */
  getLevelColor(level: number): string {
    return LEVEL_CONFIG[level]?.color ?? '#d9d9d9';
  }

  /** 保存配置（由父组件调用） */
  save() {
    this.service.update(this.roleId, this.configs).subscribe({
      next: () => this.saved.emit(),
      error: () => this.saveFailed.emit(),
    });
  }
}

import { Observable, lastValueFrom } from 'rxjs';
import { PagedResultDto } from '@abp/ng.core';
import * as XLSX from 'xlsx';

/** 分页批量拉取每页条数（不超过服务端MaxMaxResultCount） */
const EXPORT_PAGE_SIZE = 1000;

/** 每个Sheet最大行数（Excel上限1048576，留余量） */
const MAX_ROWS_PER_SHEET = 1000000;

/**
 * 分页批量拉取全部数据
 * @param fetchFn 分页查询函数，接收skipCount和maxResultCount，返回分页结果Observable
 * @param pageSize 每页条数，默认1000
 * @returns 全部数据数组
 */
export async function fetchAllPaged<T>(fetchFn: (skipCount: number, maxResultCount: number) => Observable<PagedResultDto<T>>, pageSize: number = EXPORT_PAGE_SIZE): Promise<T[]> {
  const allItems: T[] = [];
  let skipCount = 0;
  let totalCount = 0;
  do {
    const res = await lastValueFrom(fetchFn(skipCount, pageSize));
    allItems.push(...(res.items ?? []));
    totalCount = res.totalCount;
    skipCount += pageSize;
  } while (skipCount < totalCount);
  return allItems;
}

/**
 * 将行数据导出为xlsx文件并下载，超过100万行自动分Sheet
 * @param rows 行数据数组（每行为键值对象）
 * @param filename 文件名前缀（自动追加日期后缀）
 */
export function exportToXlsx(rows: Record<string, any>[], filename: string): void {
  const wb = XLSX.utils.book_new();
  const sheetCount = Math.ceil(rows.length / MAX_ROWS_PER_SHEET) || 1;
  for (let i = 0; i < sheetCount; i++) {
    const chunk = rows.slice(i * MAX_ROWS_PER_SHEET, (i + 1) * MAX_ROWS_PER_SHEET);
    const ws = XLSX.utils.json_to_sheet(chunk);
    XLSX.utils.book_append_sheet(wb, ws, `Sheet${i + 1}`);
  }
  XLSX.writeFile(wb, `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`);
}

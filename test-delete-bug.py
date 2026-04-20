#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""复现销售经理删除权限bug"""
import json, requests, urllib3
urllib3.disable_warnings()

API = 'https://localhost:44312'

def get_token(u, p):
    return requests.post(f'{API}/connect/token', data={'grant_type':'password','client_id':'TreadSnow_App','username':u,'password':p,'scope':'offline_access TreadSnow'}, verify=False).json()['access_token']

admin_token = get_token('admin', '1q2w3E*')
h = lambda t: {'Authorization': f'Bearer {t}', 'Content-Type': 'application/json'}

with open('test-ids.json', 'r', encoding='utf-8') as f:
    ids = json.load(f)

rid = ids['roles']['销售经理']

# 查看当前配置
r = requests.get(f'{API}/api/app/role-data-permission?roleId={rid}', headers=h(admin_token), verify=False).json()
print('当前销售经理配置:', json.dumps(r.get('configs',[]), ensure_ascii=False))

# 设置删除权限为Personal(1)
configs = [
    {'entityName':'account','readLevel':3,'writeLevel':3,'deleteLevel':1},
    {'entityName':'pet','readLevel':3,'writeLevel':3,'deleteLevel':1},
    {'entityName':'uploadFile','readLevel':3,'writeLevel':3,'deleteLevel':1},
]
requests.put(f'{API}/api/app/role-data-permission?roleId={rid}', headers=h(admin_token), json=configs, verify=False)

# 验证
r2 = requests.get(f'{API}/api/app/role-data-permission?roleId={rid}', headers=h(admin_token), verify=False).json()
print('更新后:', json.dumps(r2.get('configs',[]), ensure_ascii=False))

# 用zhang登录
zhang_token = get_token('sales_mgr_zhang', 'xiaoxiaoxi11')

# 获取zhang看得到的会员
accounts = requests.get(f'{API}/api/app/account?skipCount=0&maxResultCount=100', headers=h(zhang_token), verify=False).json()
print(f'\nzhang可见会员({accounts["totalCount"]}):')
for a in accounts['items']:
    print(f'  {a["name"]:25s} 负责人={str(a.get("ownerName","无")):20s} 团队={str(a.get("ownerTeamName","无"))}')

# 测试1: 删除wang的会员（不同团队，应该被拒绝）
wang_accts = [a for a in accounts['items'] if 'sales_staff_wang' in a['name']]
if wang_accts:
    aid = wang_accts[0]['id']
    r = requests.delete(f'{API}/api/app/account/{aid}', headers=h(zhang_token), verify=False)
    print(f'\n测试1: zhang删除wang的会员 → status={r.status_code}')
    if r.status_code in [200, 204]:
        print('  >>> BUG! Personal权限不应该能删除不同团队的数据')
    else:
        print('  >>> 正确拒绝')

# 测试2: 删除li的会员（同团队销售A队，Personal=自己+团队）
li_accts = [a for a in accounts['items'] if a['name'] == '客户_sales_staff_li']
if li_accts:
    aid = li_accts[0]['id']
    r = requests.delete(f'{API}/api/app/account/{aid}', headers=h(zhang_token), verify=False)
    print(f'\n测试2: zhang删除li的会员(同团队销售A队) → status={r.status_code}')
    if r.status_code in [200, 204]:
        print('  >>> 成功(按设计: Personal包含团队数据)')
    else:
        print('  >>> 被拒绝')

# 测试3: 获取zhang的effective permissions确认实际权限
r3 = requests.get(f'{API}/api/app/role-data-permission/current-user-permissions', headers=h(zhang_token), verify=False).json()
print(f'\nzhang的实际有效权限: {json.dumps(r3.get("configs",[]), ensure_ascii=False)}')

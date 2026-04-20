#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修复之前失败的团队分配和数据权限配置"""
import json
import requests
import urllib3
urllib3.disable_warnings()

API = "https://localhost:44312"

def get_token(username, password):
    r = requests.post(f"{API}/connect/token", data={
        "grant_type": "password",
        "client_id": "TreadSnow_App",
        "username": username,
        "password": password,
        "scope": "offline_access TreadSnow"
    }, verify=False)
    return r.json().get("access_token")

def api(token, method, path, data=None):
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json; charset=utf-8"}
    url = f"{API}{path}"
    if method == "GET":
        r = requests.get(url, headers=headers, verify=False, timeout=15)
    elif method == "POST":
        r = requests.post(url, headers=headers, json=data, verify=False, timeout=15)
    elif method == "PUT":
        r = requests.put(url, headers=headers, json=data, verify=False, timeout=15)
    elif method == "DELETE":
        r = requests.delete(url, headers=headers, verify=False, timeout=15)
    else:
        raise ValueError(f"Unknown method: {method}")
    if r.status_code >= 400:
        print(f"  ERROR {r.status_code}: {r.text[:300]}")
        return None
    if r.text:
        return r.json()
    return {}

# 加载已创建的ID
with open("test-ids.json", "r", encoding="utf-8") as f:
    ids = json.load(f)

admin_token = get_token("admin", "1q2w3E*")
print("Admin token OK")

roles = ids["roles"]
users = ids["users"]
teams = ids["teams"]

# 修复数据权限配置
print("\n=== 配置数据权限 ===")
dp_configs = {
    "销售经理": [
        {"entityName": "account", "readLevel": 3, "writeLevel": 3, "deleteLevel": 2},
        {"entityName": "pet", "readLevel": 3, "writeLevel": 3, "deleteLevel": 2},
        {"entityName": "uploadFile", "readLevel": 3, "writeLevel": 3, "deleteLevel": 2},
    ],
    "销售员工": [
        {"entityName": "account", "readLevel": 1, "writeLevel": 1, "deleteLevel": 1},
        {"entityName": "pet", "readLevel": 1, "writeLevel": 1, "deleteLevel": 1},
        {"entityName": "uploadFile", "readLevel": 1, "writeLevel": 1, "deleteLevel": 1},
    ],
    "技术经理": [
        {"entityName": "account", "readLevel": 4, "writeLevel": 4, "deleteLevel": 4},
        {"entityName": "pet", "readLevel": 4, "writeLevel": 4, "deleteLevel": 4},
        {"entityName": "uploadFile", "readLevel": 4, "writeLevel": 4, "deleteLevel": 4},
    ],
    "技术员工": [
        {"entityName": "account", "readLevel": 2, "writeLevel": 2, "deleteLevel": 1},
        {"entityName": "pet", "readLevel": 2, "writeLevel": 2, "deleteLevel": 1},
        {"entityName": "uploadFile", "readLevel": 2, "writeLevel": 2, "deleteLevel": 1},
    ],
}
for rn, configs in dp_configs.items():
    rid = roles[rn]
    r = api(admin_token, "PUT", f"/api/app/role-data-permission?roleId={rid}", configs)
    if r is not None:
        print(f"  OK: {rn} Read={configs[0]['readLevel']}, Write={configs[0]['writeLevel']}, Delete={configs[0]['deleteLevel']}")
    else:
        print(f"  FAILED: {rn}")

# 修复团队用户分配
print("\n=== 分配用户到团队 ===")
team_user_mapping = {
    "销售A队": ["sales_staff_li", "sales_mgr_zhang"],
    "销售B队": ["sales_staff_wang"],
    "技术支持组": ["tech_staff_zhao", "tech_mgr_chen"],
}
for tname, usernames in team_user_mapping.items():
    tid = teams[tname]
    for un in usernames:
        uid = users[un]
        r = api(admin_token, "POST", f"/api/app/team/user?teamId={tid}&userId={uid}")
        if r is not None:
            print(f"  OK: {un} -> {tname}")
        else:
            print(f"  FAILED: {un} -> {tname}")

# 修复团队角色分配
print("\n=== 为团队分配角色 ===")
r = api(admin_token, "POST", f"/api/app/team/role?teamId={teams['销售A队']}&roleId={roles['销售员工']}")
print(f"  {'OK' if r is not None else 'FAILED'}: 销售A队 -> 销售员工角色")
r = api(admin_token, "POST", f"/api/app/team/role?teamId={teams['技术支持组']}&roleId={roles['技术员工']}")
print(f"  {'OK' if r is not None else 'FAILED'}: 技术支持组 -> 技术员工角色")

# 验证
print("\n=== 验证数据权限配置 ===")
for rn, rid in roles.items():
    r = api(admin_token, "GET", f"/api/app/role-data-permission?roleId={rid}")
    if r:
        print(f"  {rn}: {json.dumps(r.get('configs', []), ensure_ascii=False)[:200]}")

print("\n=== 验证团队成员 ===")
for tname, tid in teams.items():
    r = api(admin_token, "GET", f"/api/app/team/users/{tid}")
    if r:
        names = [u.get('userName', '') for u in r]
        print(f"  {tname}: {names}")

print("\nDone!")

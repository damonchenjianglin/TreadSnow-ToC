#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""测试数据创建脚本 - 数据权限测试"""
import json
import requests
import urllib3
urllib3.disable_warnings()

API = "https://localhost:44312"

def get_token(username, password):
    """获取OAuth token"""
    r = requests.post(f"{API}/connect/token", data={
        "grant_type": "password",
        "client_id": "TreadSnow_App",
        "username": username,
        "password": password,
        "scope": "offline_access TreadSnow"
    }, verify=False)
    return r.json().get("access_token")

def api(token, method, path, data=None):
    """通用API调用"""
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
        print(f"  ERROR {r.status_code}: {r.text[:200]}")
        return None
    if r.text:
        return r.json()
    return {}

# 获取admin token
print("=== 获取admin token ===")
admin_token = get_token("admin", "1q2w3E*")
if not admin_token:
    print("FAILED to get admin token!")
    exit(1)
print("OK")

# 先删除之前创建的测试部门HQ
print("\n=== 清理已有测试数据 ===")
depts = api(admin_token, "GET", "/api/app/department?skipCount=0&maxResultCount=100")
if depts and depts.get("items"):
    for d in depts["items"]:
        if d["name"] == "HQ":
            api(admin_token, "DELETE", f"/api/app/department/{d['id']}")
            print(f"  删除HQ部门: {d['id']}")

# 创建部门结构
print("\n=== 创建部门结构 ===")
# 总公司
r = api(admin_token, "POST", "/api/app/department", {"name": "总公司"})
COMPANY_ID = r["id"]
print(f"  总公司: {COMPANY_ID}")

# 销售部
r = api(admin_token, "POST", "/api/app/department", {"name": "销售部", "parentDepartmentId": COMPANY_ID})
SALES_ID = r["id"]
print(f"  销售部: {SALES_ID}")

# 销售一组
r = api(admin_token, "POST", "/api/app/department", {"name": "销售一组", "parentDepartmentId": SALES_ID})
SALES1_ID = r["id"]
print(f"  销售一组: {SALES1_ID}")

# 销售二组
r = api(admin_token, "POST", "/api/app/department", {"name": "销售二组", "parentDepartmentId": SALES_ID})
SALES2_ID = r["id"]
print(f"  销售二组: {SALES2_ID}")

# 技术部
r = api(admin_token, "POST", "/api/app/department", {"name": "技术部", "parentDepartmentId": COMPANY_ID})
TECH_ID = r["id"]
print(f"  技术部: {TECH_ID}")

# 创建角色
print("\n=== 创建角色 ===")
roles = {}
role_names = ["销售经理", "销售员工", "技术经理", "技术员工"]
for rn in role_names:
    r = api(admin_token, "POST", "/api/identity/roles", {"name": rn, "isDefault": False, "isPublic": True})
    if r:
        roles[rn] = r["id"]
        print(f"  {rn}: {r['id']}")
    else:
        # 可能已存在，查询
        all_roles = api(admin_token, "GET", "/api/identity/roles?skipCount=0&maxResultCount=100")
        for ar in all_roles.get("items", []):
            if ar["name"] == rn:
                roles[rn] = ar["id"]
                print(f"  {rn} (已存在): {ar['id']}")
                break

# 为每个角色授予会员/宠物/附件/部门/团队的完整CRUD权限
print("\n=== 为角色授予功能权限 ===")
permission_names = [
    "TreadSnow.Accounts", "TreadSnow.Accounts.Create", "TreadSnow.Accounts.Edit", "TreadSnow.Accounts.Delete",
    "TreadSnow.Pets", "TreadSnow.Pets.Create", "TreadSnow.Pets.Edit", "TreadSnow.Pets.Delete",
    "TreadSnow.UploadFiles", "TreadSnow.UploadFiles.Create", "TreadSnow.UploadFiles.Edit", "TreadSnow.UploadFiles.Delete",
    "TreadSnow.Departments", "TreadSnow.Teams",
    "TreadSnow.DataPermissions",
]
for rn, rid in roles.items():
    perms = {"permissions": [{"name": p, "isGranted": True} for p in permission_names]}
    r = api(admin_token, "PUT", f"/api/permission-management/permissions?providerName=R&providerKey={rn}", perms)
    print(f"  {rn} 功能权限已设置")

# 配置数据权限
# 销售经理: 部门及下级(3), 写(3), 删(2)
# 销售员工: 个人(1), 写(1), 删(1)
# 技术经理: 组织(4), 写(4), 删(4) - 可看所有
# 技术员工: 部门(2), 写(2), 删(1)
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
    print(f"  {rn} 数据权限已设置: Read={configs[0]['readLevel']}, Write={configs[0]['writeLevel']}, Delete={configs[0]['deleteLevel']}")

# 创建测试用户
print("\n=== 创建测试用户 ===")
PASSWORD = "xiaoxiaoxi11"
users = {}
user_configs = [
    # (用户名, 邮箱, 角色, 部门ID, 部门名称)
    ("sales_mgr_zhang", "zhang@test.com", "销售经理", SALES_ID, "销售部"),
    ("sales_staff_li", "li@test.com", "销售员工", SALES1_ID, "销售一组"),
    ("sales_staff_wang", "wang@test.com", "销售员工", SALES2_ID, "销售二组"),
    ("tech_mgr_chen", "chen@test.com", "技术经理", TECH_ID, "技术部"),
    ("tech_staff_zhao", "zhao@test.com", "技术员工", TECH_ID, "技术部"),
]

for username, email, role_name, dept_id, dept_name in user_configs:
    # 创建用户
    user_data = {
        "userName": username,
        "name": username,
        "surname": "test",
        "email": email,
        "phoneNumber": None,
        "isActive": True,
        "lockoutEnabled": False,
        "roleNames": [role_name],
        "password": PASSWORD,
        "extraProperties": {"DepartmentId": dept_id}
    }
    r = api(admin_token, "POST", "/api/identity/users", user_data)
    if r:
        users[username] = r["id"]
        print(f"  {username} ({dept_name}, {role_name}): {r['id']}")
    else:
        # 可能已存在
        all_users = api(admin_token, "GET", f"/api/identity/users?filter={username}&skipCount=0&maxResultCount=10")
        if all_users and all_users.get("items"):
            for u in all_users["items"]:
                if u["userName"] == username:
                    users[username] = u["id"]
                    print(f"  {username} (已存在): {u['id']}")
                    break

# 创建团队
print("\n=== 创建团队 ===")
teams = {}
team_configs = [
    ("销售A队", SALES1_ID),
    ("销售B队", SALES2_ID),
    ("技术支持组", TECH_ID),
]
for tname, dept_id in team_configs:
    r = api(admin_token, "POST", "/api/app/team", {"name": tname, "departmentId": dept_id})
    if r:
        teams[tname] = r["id"]
        print(f"  {tname}: {r['id']}")

# 分配用户到团队
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
        api(admin_token, "POST", f"/api/app/team/user?teamId={tid}&userId={uid}")
        print(f"  {un} -> {tname}")

# 为团队分配角色（团队角色继承测试）
print("\n=== 为团队分配角色 ===")
# 销售A队分配"销售员工"角色 - 这样团队成员自动继承
api(admin_token, "POST", f"/api/app/team/role?teamId={teams['销售A队']}&roleId={roles['销售员工']}")
print(f"  销售A队 -> 销售员工角色")
api(admin_token, "POST", f"/api/app/team/role?teamId={teams['技术支持组']}&roleId={roles['技术员工']}")
print(f"  技术支持组 -> 技术员工角色")

# 输出汇总
print("\n" + "="*60)
print("=== 测试环境设置完成 ===")
print("="*60)
print(f"\n部门结构:")
print(f"  总公司 ({COMPANY_ID})")
print(f"  ├── 销售部 ({SALES_ID})")
print(f"  │   ├── 销售一组 ({SALES1_ID})")
print(f"  │   └── 销售二组 ({SALES2_ID})")
print(f"  └── 技术部 ({TECH_ID})")
print(f"\n角色及数据权限:")
for rn, rid in roles.items():
    cfg = dp_configs[rn][0]
    print(f"  {rn} ({rid}): Read={cfg['readLevel']}, Write={cfg['writeLevel']}, Delete={cfg['deleteLevel']}")
print(f"\n用户:")
for un, uid in users.items():
    config = next(c for c in user_configs if c[0] == un)
    print(f"  {un} ({uid}): {config[4]}, 角色={config[2]}")
print(f"\n团队:")
for tn, tid in teams.items():
    print(f"  {tn} ({tid}): 成员={team_user_mapping[tn]}")
print(f"\n所有测试用户密码: {PASSWORD}")

# 保存ID映射以供后续测试使用
result = {
    "departments": {"总公司": COMPANY_ID, "销售部": SALES_ID, "销售一组": SALES1_ID, "销售二组": SALES2_ID, "技术部": TECH_ID},
    "roles": roles,
    "users": users,
    "teams": teams,
}
with open("test-ids.json", "w", encoding="utf-8") as f:
    json.dump(result, f, ensure_ascii=False, indent=2)
print(f"\nID映射已保存到 test-ids.json")

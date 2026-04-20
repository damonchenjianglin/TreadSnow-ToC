#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""创建测试数据 + 数据权限验证测试"""
import json
import requests
import urllib3
import uuid
urllib3.disable_warnings()

API = "https://localhost:44312"
PASSWORD = "xiaoxiaoxi11"
PASS = 0
FAIL = 0

def get_token(username, password):
    r = requests.post(f"{API}/connect/token", data={
        "grant_type": "password",
        "client_id": "TreadSnow_App",
        "username": username,
        "password": password,
        "scope": "offline_access TreadSnow"
    }, verify=False, timeout=10)
    data = r.json()
    if "access_token" not in data:
        print(f"  LOGIN FAILED for {username}: {data.get('error_description', data)}")
        return None
    return data["access_token"]

def api(token, method, path, data=None):
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json; charset=utf-8"}
    url = f"{API}{path}"
    try:
        if method == "GET":
            r = requests.get(url, headers=headers, verify=False, timeout=30)
        elif method == "POST":
            r = requests.post(url, headers=headers, json=data, verify=False, timeout=30)
        elif method == "PUT":
            r = requests.put(url, headers=headers, json=data, verify=False, timeout=30)
        elif method == "DELETE":
            r = requests.delete(url, headers=headers, verify=False, timeout=30)
        else:
            raise ValueError(f"Unknown method: {method}")
    except Exception as e:
        return {"_error": str(e), "_status": 0}
    result = {}
    if r.text:
        try:
            result = r.json()
        except:
            result = {"_raw": r.text[:200]}
    result["_status"] = r.status_code
    return result

def check(desc, condition):
    global PASS, FAIL
    if condition:
        PASS += 1
        print(f"  [PASS] {desc}")
    else:
        FAIL += 1
        print(f"  [FAIL] {desc}")

# 加载已创建的ID
with open("test-ids.json", "r", encoding="utf-8") as f:
    ids = json.load(f)

users = ids["users"]
teams = ids["teams"]

# 获取所有用户token
print("=== 获取用户Token ===")
tokens = {}
admin_token = get_token("admin", "1q2w3E*")
tokens["admin"] = admin_token
for username in users:
    t = get_token(username, PASSWORD)
    tokens[username] = t
    status = "OK" if t else "FAILED"
    print(f"  {username}: {status}")

# ============================================================
# 创建测试数据：每个用户创建会员和宠物
# ============================================================
print("\n=== 创建会员测试数据 ===")
account_ids = {}
for username in users:
    t = tokens[username]
    if not t:
        continue
    r = api(t, "POST", "/api/app/account", {
        "name": f"客户_{username}",
        "phone": "13800138000",
        "email": f"{username}_client@test.com",
        "openId": str(uuid.uuid4())[:16],
    })
    if r.get("id"):
        account_ids[username] = r["id"]
        print(f"  {username} -> 会员ID: {r['id']}")
    else:
        print(f"  {username} 创建失败: status={r.get('_status')}, {str(r)[:200]}")

# admin也创建一个
r = api(admin_token, "POST", "/api/app/account", {
    "name": "客户_admin",
    "phone": "13900139000",
    "email": "admin_client@test.com",
    "openId": str(uuid.uuid4())[:16],
})
if r.get("id"):
    account_ids["admin"] = r["id"]
    print(f"  admin -> 会员ID: {r['id']}")

# 创建一个指定到销售A队的会员（由li负责、归属销售A队）
r = api(admin_token, "POST", "/api/app/account", {
    "name": "客户_团队A共享",
    "phone": "13700137000",
    "email": "teama@test.com",
    "openId": str(uuid.uuid4())[:16],
    "ownerTeamId": teams["销售A队"],
    "ownerId": users["sales_staff_li"],
})
if r.get("id"):
    account_ids["team_a_shared"] = r["id"]
    print(f"  团队A共享会员: {r['id']}")

print(f"\n  共创建 {len(account_ids)} 个会员")

# 创建宠物
print("\n=== 创建宠物测试数据 ===")
pet_ids = {}
for username in users:
    t = tokens[username]
    if not t or username not in account_ids:
        continue
    r = api(t, "POST", "/api/app/pet", {
        "name": f"宠物_{username}",
        "accountId": account_ids[username],
    })
    if r.get("id"):
        pet_ids[username] = r["id"]
        print(f"  {username} -> 宠物ID: {r['id']}")
    else:
        print(f"  {username} 创建宠物失败: status={r.get('_status')}, {str(r)[:200]}")

# 创建附件
print("\n=== 创建附件测试数据 ===")
file_ids = {}
for username in users:
    t = tokens[username]
    if not t:
        continue
    r = api(t, "POST", "/api/app/upload-file", {
        "entityName": "account",
        "recordId": account_ids.get(username, "test"),
        "name": f"文件_{username}.pdf",
        "type": "application/pdf",
        "path": f"/uploads/{username}.pdf",
    })
    if r.get("id"):
        file_ids[username] = r["id"]
        print(f"  {username} -> 附件ID: {r['id']}")
    else:
        print(f"  {username} 创建附件失败: status={r.get('_status')}, {str(r)[:200]}")

# ============================================================
# 数据权限验证测试
# ============================================================
print("\n" + "=" * 60)
print("=== 数据权限验证测试 ===")
print("=" * 60)

# ----- 测试1: 销售员工 Personal(1) -----
print("\n--- 测试1: sales_staff_li (Personal权限=1, 销售一组, 销售A队) ---")
print("  预期: 只能看自己负责的 + 自己团队(销售A队)负责的")
t = tokens["sales_staff_li"]
r = api(t, "GET", "/api/app/account?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  可见会员({count}): {visible}")
    check("能看到自己创建的会员(客户_sales_staff_li)", any("sales_staff_li" in v for v in visible))
    check("能看到团队A共享会员", any("团队A" in v for v in visible))
    check("不能看到tech_mgr_chen的会员", not any("tech_mgr_chen" in v for v in visible))
    check("不能看到sales_staff_wang的会员(不同团队)", not any("sales_staff_wang" in v for v in visible))
    check("不能看到admin的会员", not any("admin" in v for v in visible))
else:
    print(f"  请求失败: {r}")

# sales_mgr_zhang也在销售A队，Personal权限应看到团队A的数据
print("\n  sales_mgr_zhang同在销售A队的视角(但他权限是DeptAndChildren=3):")
t = tokens["sales_mgr_zhang"]
r_mgr = api(t, "GET", "/api/app/account?skipCount=0&maxResultCount=100")
if r_mgr and "items" in r_mgr:
    visible_mgr = [item["name"] for item in r_mgr["items"]]
    print(f"  可见会员({r_mgr['totalCount']}): {visible_mgr}")

# ----- 测试2: 销售经理 DeptAndChildren(3) -----
print("\n--- 测试2: sales_mgr_zhang (DeptAndChildren=3, 销售部) ---")
print("  预期: 能看到销售部 + 销售一组 + 销售二组的所有数据")
t = tokens["sales_mgr_zhang"]
r = api(t, "GET", "/api/app/account?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  可见会员({count}): {visible}")
    check("能看到自己的会员", any("sales_mgr_zhang" in v for v in visible))
    check("能看到销售一组li的会员", any("sales_staff_li" in v for v in visible))
    check("能看到销售二组wang的会员", any("sales_staff_wang" in v for v in visible))
    check("能看到团队A共享会员", any("团队A" in v for v in visible))
    check("不能看到技术部chen的会员", not any("tech_mgr_chen" in v for v in visible))
    check("不能看到admin的会员(无部门)", not any("客户_admin" in v for v in visible))

# ----- 测试3: 技术经理 Organization(4) -----
print("\n--- 测试3: tech_mgr_chen (Organization=4, 技术部) ---")
print("  预期: 能看到所有数据")
t = tokens["tech_mgr_chen"]
r = api(t, "GET", "/api/app/account?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  可见会员({count}): {visible}")
    check("能看到所有新建会员(>=7)", count >= 7)
    check("能看到销售部的会员", any("sales_mgr_zhang" in v for v in visible))
    check("能看到技术部的会员", any("tech_staff_zhao" in v for v in visible))
    check("能看到admin的会员", any("客户_admin" in v for v in visible))

# ----- 测试4: 技术员工 Department(2) -----
print("\n--- 测试4: tech_staff_zhao (Department=2, 技术部) ---")
print("  预期: 能看到技术部所有人的数据")
t = tokens["tech_staff_zhao"]
r = api(t, "GET", "/api/app/account?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  可见会员({count}): {visible}")
    check("能看到自己的会员", any("tech_staff_zhao" in v for v in visible))
    check("能看到同部门chen的会员", any("tech_mgr_chen" in v for v in visible))
    check("不能看到销售部的会员", not any("sales_mgr_zhang" in v for v in visible))
    check("不能看到admin的会员", not any("客户_admin" in v for v in visible))

# ----- 测试5: 宠物列表权限 -----
print("\n--- 测试5: 宠物列表权限测试 ---")
t = tokens["sales_staff_li"]
r = api(t, "GET", "/api/app/pet?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  sales_staff_li 可见宠物({count}): {visible}")
    check("[宠物]Personal: 能看到自己的", any("sales_staff_li" in v for v in visible))
    check("[宠物]Personal: 看不到技术部的", not any("tech_mgr_chen" in v for v in visible))

t = tokens["tech_mgr_chen"]
r = api(t, "GET", "/api/app/pet?skipCount=0&maxResultCount=100")
if r and "items" in r:
    count = r["totalCount"]
    print(f"  tech_mgr_chen 可见宠物({count})")
    check("[宠物]Organization: 能看到所有(>=5)", count >= 5)

# ----- 测试6: 附件列表权限 -----
print("\n--- 测试6: 附件列表权限测试 ---")
t = tokens["sales_staff_li"]
r = api(t, "GET", "/api/app/upload-file?skipCount=0&maxResultCount=100")
if r and "items" in r:
    visible = [item["name"] for item in r["items"]]
    count = r["totalCount"]
    print(f"  sales_staff_li 可见附件({count}): {visible}")
    check("[附件]Personal: 能看到自己的", any("sales_staff_li" in v for v in visible))
    check("[附件]Personal: 看不到技术部的", not any("tech_mgr_chen" in v for v in visible))

t = tokens["tech_mgr_chen"]
r = api(t, "GET", "/api/app/upload-file?skipCount=0&maxResultCount=100")
if r and "items" in r:
    count = r["totalCount"]
    print(f"  tech_mgr_chen 可见附件({count})")
    check("[附件]Organization: 能看到所有(>=5)", count >= 5)

# ----- 测试7: 编辑权限 -----
print("\n--- 测试7: 编辑权限测试 ---")

# Personal(1)不能编辑他人会员
if "sales_staff_wang" in account_ids:
    t = tokens["sales_staff_li"]
    orig = api(tokens["admin"], "GET", f"/api/app/account/{account_ids['sales_staff_wang']}")
    if orig and orig.get("name"):
        r = api(t, "PUT", f"/api/app/account/{account_ids['sales_staff_wang']}", {
            "name": orig["name"], "phone": orig.get("phone", ""), "email": orig.get("email", ""),
            "openId": orig.get("openId", "test"),
        })
        check("[编辑]Personal不能编辑他人(不同团队)会员", r.get("_status") in [403, 500])

# DeptAndChildren(3)能编辑下级部门会员
if "sales_staff_li" in account_ids:
    t = tokens["sales_mgr_zhang"]
    orig = api(t, "GET", f"/api/app/account/{account_ids['sales_staff_li']}")
    if orig and orig.get("name"):
        r = api(t, "PUT", f"/api/app/account/{account_ids['sales_staff_li']}", {
            "name": orig["name"], "phone": orig.get("phone", ""),
            "email": orig.get("email", ""), "openId": orig.get("openId", "test"),
        })
        check("[编辑]DeptAndChildren能编辑下级部门会员", r.get("_status") == 200 or r.get("id") is not None)

# Department(2)不能编辑其他部门
if "sales_staff_li" in account_ids:
    t = tokens["tech_staff_zhao"]
    orig = api(tokens["admin"], "GET", f"/api/app/account/{account_ids['sales_staff_li']}")
    if orig and orig.get("name"):
        r = api(t, "PUT", f"/api/app/account/{account_ids['sales_staff_li']}", {
            "name": orig["name"], "phone": orig.get("phone", ""),
            "email": orig.get("email", ""), "openId": orig.get("openId", "test"),
        })
        check("[编辑]Department不能编辑其他部门会员", r.get("_status") in [403, 500])

# ----- 测试8: 删除权限 -----
print("\n--- 测试8: 删除权限测试 ---")

# Personal(deleteLevel=1)不能删除同部门他人附件
if "tech_mgr_chen" in file_ids:
    t = tokens["tech_staff_zhao"]
    r = api(t, "DELETE", f"/api/app/upload-file/{file_ids['tech_mgr_chen']}")
    check("[删除]deleteLevel=1不能删除同部门他人附件", r.get("_status") in [403, 500])

# Organization(deleteLevel=4)可以删除任何人的附件
# 先创建一个临时附件用于测试删除
temp_file = api(tokens["sales_staff_wang"], "POST", "/api/app/upload-file", {
    "entityName": "test", "recordId": "temp", "name": "临时删除测试.pdf",
    "type": "application/pdf", "path": "/uploads/temp.pdf",
})
if temp_file and temp_file.get("id"):
    t = tokens["tech_mgr_chen"]
    r = api(t, "DELETE", f"/api/app/upload-file/{temp_file['id']}")
    check("[删除]Organization能删除任何人的附件", r.get("_status") in [200, 204])

# Personal可以删除自己的附件
if "sales_staff_li" in file_ids:
    t = tokens["sales_staff_li"]
    r = api(t, "DELETE", f"/api/app/upload-file/{file_ids['sales_staff_li']}")
    check("[删除]Personal能删除自己的附件", r.get("_status") in [200, 204])
    # 重建
    r2 = api(t, "POST", "/api/app/upload-file", {
        "entityName": "account", "recordId": account_ids.get("sales_staff_li", "test"),
        "name": "文件_sales_staff_li_v2.pdf", "type": "application/pdf",
        "path": "/uploads/sales_staff_li_v2.pdf",
    })
    if r2 and r2.get("id"):
        file_ids["sales_staff_li"] = r2["id"]

# ============================================================
# 汇总
# ============================================================
print("\n" + "=" * 60)
total = PASS + FAIL
print(f"=== 测试结果: PASS={PASS}/{total}, FAIL={FAIL}/{total} ===")
print("=" * 60)
if FAIL > 0:
    print(f"\n有 {FAIL} 个测试失败，需要检查修复！")
else:
    print("\n所有测试通过！数据权限系统工作正常。")

# 保存
test_data = {"accounts": account_ids, "pets": pet_ids, "files": file_ids}
with open("test-data-ids.json", "w", encoding="utf-8") as f:
    json.dump(test_data, f, ensure_ascii=False, indent=2)
print(f"\n测试数据ID已保存到 test-data-ids.json")

namespace TreadSnow;

/// <summary>
/// 数据访问权限等级
/// </summary>
public enum DataAccessLevel
{
    /// <summary>
    /// 无权限
    /// </summary>
    None = 0,

    /// <summary>
    /// 个人权限（仅自己负责的数据和所在团队负责的数据）
    /// </summary>
    Personal = 1,

    /// <summary>
    /// 部门权限（本部门所有用户和团队的数据）
    /// </summary>
    Department = 2,

    /// <summary>
    /// 部门及下级部门权限（本部门及所有下级部门的数据）
    /// </summary>
    DepartmentAndChildren = 3,

    /// <summary>
    /// 组织权限（租户内所有数据）
    /// </summary>
    Organization = 4
}

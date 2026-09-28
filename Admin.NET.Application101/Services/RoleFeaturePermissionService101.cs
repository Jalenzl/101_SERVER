using Admin.NET.Core.Service;
using Admin.NET.Core101.Entity;
using System.Text.Json;

namespace Admin.NET.Application101.Services;

public sealed record RoleFeatureRule101(string FeatureKey, bool Enabled, bool Editable,
    string DepartmentScope, string[] VisibleFields);
public sealed record MyFeaturePermissions101(bool Unrestricted, RoleFeatureRule101[] Rules);

public sealed class RoleFeaturePermissionService101(
    SqlSugarRepository<RoleFeaturePermission101> repository,
    SqlSugarRepository<SysRole> roles,
    SqlSugarRepository<SysOrg> organizations,
    SysUserRoleService userRoles,
    UserManager user) : ITransient
{
    private static readonly HashSet<string> Scopes = new(StringComparer.Ordinal)
        { "unlimited", "all", "below", "superior", "self" };

    public async Task<RoleFeatureRule101[]> GetRoleAsync(long roleId)
    {
        await RequireManagedRoleAsync(roleId);
        return (await repository.AsQueryable().Where(item => item.RoleId == roleId).ToListAsync())
            .Select(Map).ToArray();
    }

    public async Task SaveRoleAsync(long roleId, RoleFeatureRule101[] rules)
    {
        await RequireManagedRoleAsync(roleId);
        if (rules is null || rules.Length > 200 ||
            rules.Select(item => item.FeatureKey).Distinct(StringComparer.Ordinal).Count() != rules.Length ||
            rules.Any(item => item.FeatureKey is null || item.FeatureKey.Length is < 1 or > 80 ||
                !item.FeatureKey.All(char.IsAsciiLetterOrDigit) || !Scopes.Contains(item.DepartmentScope) ||
                item.VisibleFields is null || item.VisibleFields.Length > 150 ||
                item.VisibleFields.Any(field => field is null || field.Length is < 1 or > 80 ||
                    !field.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))))
            throw Oops.Oh("特性权限配置无效。").StatusCode(400);

        await repository.DeleteAsync(item => item.RoleId == roleId);
        if (rules.Length == 0) return;
        await repository.InsertRangeAsync(rules.Select(item => new RoleFeaturePermission101
        {
            RoleId = roleId, FeatureKey = item.FeatureKey, Enabled = item.Enabled,
            Editable = item.Editable && item.Enabled, DepartmentScope = item.DepartmentScope,
            VisibleFieldsJson = JsonSerializer.Serialize(item.VisibleFields.Distinct().ToArray())
        }).ToList());
    }

    public async Task<MyFeaturePermissions101> GetMineAsync()
    {
        if (user.SuperAdmin) return new(true, []);
        var roleIds = await userRoles.GetUserRoleIdList(user.UserId);
        if (roleIds.Count == 0) return new(false, []);
        var rows = await repository.AsQueryable().Where(item => roleIds.Contains(item.RoleId)).ToListAsync();
        var configuredIds = rows.Select(item => item.RoleId).ToHashSet();
        return new(roleIds.Any(id => !configuredIds.Contains(id)), rows.Select(Map).ToArray());
    }

    public async Task<RoleFeatureRule101[]> GetEnabledRulesAsync(string featureKey)
    {
        var mine = await GetMineAsync();
        if (mine.Unrestricted) return [new(featureKey, true, true, "unlimited", [])];
        return mine.Rules.Where(item => item.FeatureKey == featureKey && item.Enabled).ToArray();
    }

    public async Task RequireEditableAsync(string featureKey)
    {
        if (!(await GetEnabledRulesAsync(featureKey)).Any(item => item.Editable))
            throw Oops.Oh("没有此数据表的编辑权限。").StatusCode(403);
    }

    public async Task<List<long>?> GetAllowedOrgIdsAsync(string featureKey)
    {
        var rules = await GetEnabledRulesAsync(featureKey);
        if (rules.Length == 0) throw Oops.Oh("没有此数据表的访问权限。").StatusCode(403);
        if (rules.Any(item => item.DepartmentScope == "unlimited")) return null;
        var orgs = await organizations.AsQueryable().ToListAsync();
        var byId = orgs.ToDictionary(item => item.Id);
        var allowed = new HashSet<long>();
        foreach (var rule in rules)
        {
            if (user.OrgId == 0) continue;
            allowed.Add(user.OrgId);
            if (rule.DepartmentScope is "all" or "below")
            {
                var frontier = new Queue<long>();
                var visited = new HashSet<long> { user.OrgId };
                frontier.Enqueue(user.OrgId);
                while (frontier.Count > 0)
                {
                    var parent = frontier.Dequeue();
                    foreach (var child in orgs.Where(item => item.Pid == parent && visited.Add(item.Id)))
                    {
                        allowed.Add(child.Id);
                        frontier.Enqueue(child.Id);
                    }
                }
            }
            if (rule.DepartmentScope is "all" or "superior")
            {
                var current = user.OrgId;
                var visited = new HashSet<long> { current };
                while (byId.TryGetValue(current, out var org) && org.Pid != 0 && visited.Add(org.Pid))
                {
                    allowed.Add(org.Pid);
                    current = org.Pid;
                }
            }
        }
        return allowed.ToList();
    }

    public async Task<List<string>?> GetAllowedDepartmentNamesAsync(string featureKey)
    {
        var ids = await GetAllowedOrgIdsAsync(featureKey);
        if (ids is null) return null;
        var orgs = await organizations.AsQueryable().ToListAsync();
        var idSet = ids.ToHashSet();
        return orgs.GroupBy(item => item.Name)
            .Where(group => group.All(item => idSet.Contains(item.Id)))
            .Select(group => group.Key).ToList();
    }

    public async Task RequireDepartmentAsync(string featureKey, string department)
    {
        var allowed = await GetAllowedDepartmentNamesAsync(featureKey);
        if (allowed is not null && !allowed.Contains(department))
            throw Oops.Oh("没有此部门的数据权限。").StatusCode(403);
    }

    private async Task RequireManagedRoleAsync(long roleId)
    {
        var role = await roles.GetFirstAsync(item => item.Id == roleId)
            ?? throw Oops.Oh("角色不存在。").StatusCode(404);
        if (user.SuperAdmin || role.CreateUserId == user.UserId) return;
        var ownedIds = await userRoles.GetUserRoleIdList(user.UserId);
        if (!ownedIds.Contains(roleId)) throw Oops.Oh("无权管理此角色。").StatusCode(403);
    }

    private static RoleFeatureRule101 Map(RoleFeaturePermission101 item) => new(
        item.FeatureKey, item.Enabled, item.Editable, item.DepartmentScope,
        JsonSerializer.Deserialize<string[]>(item.VisibleFieldsJson) ?? []);
}

namespace Admin.NET.Application101.Authorization;

public static class ApiPermissionAuthorization101
{
    public static bool IsAllowed(string routeName, string? endpointPermission,
        IReadOnlyCollection<string> ownedPermissions, IReadOnlyCollection<string> allPermissions)
    {
        var owned = ownedPermissions.Any(value =>
            routeName.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (endpointPermission is not null)
            return owned && allPermissions.Any(value =>
                routeName.Equals(value, StringComparison.OrdinalIgnoreCase));
        return owned || allPermissions.All(value =>
            !routeName.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}

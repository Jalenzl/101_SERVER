namespace Admin.NET.Application101.Services;

public sealed class AdminNetCurrentUser101(UserManager userManager) : ICurrentUser101, IScoped
{
    public long UserId => userManager.UserId;
    public string RealName => userManager.RealName;
    public bool IsAdministrator => userManager.SuperAdmin || userManager.SysAdmin;
}

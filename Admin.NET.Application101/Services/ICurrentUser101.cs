namespace Admin.NET.Application101.Services;

public interface ICurrentUser101
{
    long UserId { get; }
    string RealName { get; }
    bool IsAdministrator { get; }
}

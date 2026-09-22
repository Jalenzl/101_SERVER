using Admin.NET.Application101.Dtos.Operations;
using Admin.NET.Application101.Services;
using Admin.NET.Core101.Domain;
using Xunit;

namespace Admin.NET.Application101.Tests.Services;

public sealed class OperationService101Tests
{
    [Fact]
    public void SignatureInput_ContainsOnlyRole_AndApprovedRolesAreFixed()
    {
        Assert.Equal(new[] { "操作岗", "检查岗", "检验员会签", "委托单位会签", "产保会签" },
            SignatureRoles.Operation.OrderBy(item => Array.IndexOf(
                new[] { "操作岗", "检查岗", "检验员会签", "委托单位会签", "产保会签" }, item)));
        Assert.Equal(new[] { "Role" }, typeof(CreateSignatureInput).GetProperties().Select(item => item.Name));
        Assert.DoesNotContain(typeof(CreateSignatureInput).GetProperties(), item =>
            item.Name.Contains("Signer", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CurrentUserContract_DoesNotAcceptRequestIdentity()
    {
        Assert.Equal(new[] { "IsAdministrator", "RealName", "UserId" },
            typeof(ICurrentUser101).GetProperties().Select(item => item.Name).OrderBy(item => item));
    }
}

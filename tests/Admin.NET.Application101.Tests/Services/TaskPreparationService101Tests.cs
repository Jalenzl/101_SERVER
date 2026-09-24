using Admin.NET.Application101.Validation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Admin.NET.Application101.Tests.Services;

public sealed class TaskPreparationService101Tests
{
    [Fact]
    public void TransferValidation_RejectsUnknownTablesFieldsAndNestedValues()
    {
        var valid = JObject.Parse("""{"parameter":"F1","unit":"N"}""");
        TransferFieldAllowList101.Validate("upper-thrust", valid);

        var unknownField = JObject.Parse("""{"password":"secret"}""");
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("upper-thrust", unknownField));

        var nested = JObject.Parse("""{"parameter":{"nested":true}}""");
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("upper-thrust", nested));
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("other-table", valid));
    }
}

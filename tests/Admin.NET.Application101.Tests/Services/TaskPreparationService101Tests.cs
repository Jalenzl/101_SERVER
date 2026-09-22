using System.Text.Json;
using Admin.NET.Application101.Validation;
using Xunit;

namespace Admin.NET.Application101.Tests.Services;

public sealed class TaskPreparationService101Tests
{
    [Fact]
    public void TransferValidation_RejectsUnknownTablesFieldsAndNestedValues()
    {
        using var valid = JsonDocument.Parse("""{"parameter":"F1","unit":"N"}""");
        TransferFieldAllowList101.Validate("upper-thrust", valid.RootElement);

        using var unknownField = JsonDocument.Parse("""{"password":"secret"}""");
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("upper-thrust", unknownField.RootElement));

        using var nested = JsonDocument.Parse("""{"parameter":{"nested":true}}""");
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("upper-thrust", nested.RootElement));
        Assert.ThrowsAny<Exception>(() => TransferFieldAllowList101.Validate("other-table", valid.RootElement));
    }
}

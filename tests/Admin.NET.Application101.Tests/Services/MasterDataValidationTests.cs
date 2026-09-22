using System.ComponentModel.DataAnnotations;
using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Devices;
using Admin.NET.Application101.Dtos.Personnel;
using Admin.NET.Application101.Dtos.Tasks;
using Admin.NET.Application101.Dtos.Workflows;
using Xunit;

namespace Admin.NET.Application101.Tests.Services;

public sealed class MasterDataValidationTests
{
    [Fact]
    public void InvalidMasterDataInputs_AreRejected()
    {
        AssertInvalid(new CreateTaskInput { Name = " ", IgnitionCount = -1 });
        AssertInvalid(new CreateDeviceInput { Code = " " });
        AssertInvalid(new CreatePersonInput { Name = " " });
        AssertInvalid(new CreateWorkflowInput { Department = "", Process = "", Step = "" });
        AssertInvalid(new PageQuery { PageSize = 201 });
    }

    private static void AssertInvalid(object value)
    {
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(value, new ValidationContext(value), results, true));
        Assert.NotEmpty(results);
    }
}

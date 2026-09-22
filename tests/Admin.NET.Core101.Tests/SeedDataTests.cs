using Admin.NET.Core101.Seed;

namespace Admin.NET.Core101.Tests;

public sealed class SeedDataTests
{
    [Fact]
    public void SeedIds_AreNonEmptyUniqueAndStable()
    {
        var first = SeedIds101.All.ToArray();
        var second = SeedIds101.All.ToArray();

        Assert.NotEmpty(first);
        Assert.DoesNotContain(Guid.Empty, first);
        Assert.Equal(first.Length, first.Distinct().Count());
        Assert.Equal(first, second);
    }

    [Fact]
    public void Seeds_ContainRequiredFrontendFixtures()
    {
        Assert.Contains(TaskSeed101.Tasks, item => item.Id == SeedIds101.TaskGg1101);
        Assert.Contains(MasterDataSeed101.Devices, item => item.Id == SeedIds101.Device1);
        Assert.Contains(MasterDataSeed101.People, item => item.Id == SeedIds101.Person1);
        Assert.Contains(MasterDataSeed101.Documents, item => item.Id == SeedIds101.Document1);
        Assert.Contains(MasterDataSeed101.WorkflowNodes, item => item.Id == SeedIds101.WorkflowModel0001);
    }

    [Fact]
    public void MasterSeeds_PreserveFrontendRowCounts()
    {
        Assert.Equal(10, MasterDataSeed101.People.Count);
        Assert.Equal(8, MasterDataSeed101.Devices.Count);
        Assert.Equal(7, MasterDataSeed101.Documents.Count);
        Assert.Equal(334, MasterDataSeed101.WorkflowNodes.Count);
        Assert.Equal(5, TaskSeed101.Tasks.Count);
    }

    [Fact]
    public void InvalidFrontendDates_AreStoredAsNull()
    {
        Assert.Null(MasterDataSeed101.People.Single(item => item.Id == SeedIds101.Person2).InspectorValidUntil);
        Assert.Null(MasterDataSeed101.Devices.Single(item => item.Id == SeedIds101.Device1).NextMaintenance);
    }
}

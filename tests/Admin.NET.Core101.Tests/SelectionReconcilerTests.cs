using Admin.NET.Core101.Domain;

namespace Admin.NET.Core101.Tests;

public sealed class SelectionReconcilerTests
{
    [Fact]
    public void Compare_CollapsesDuplicates_AndReturnsDeterministicDelta()
    {
        var delta = SelectionReconciler.Compare(new[] { "b", "a", "remove" }, new[] { "a", "b", "b", "new" });
        Assert.Equal(new[] { "new" }, delta.Add);
        Assert.Equal(new[] { "remove" }, delta.Remove);
    }

    [Fact]
    public void Compare_UnchangedSet_HasNoWork()
    {
        var delta = SelectionReconciler.Compare(new[] { 2, 1 }, new[] { 1, 2, 2 });
        Assert.Empty(delta.Add);
        Assert.Empty(delta.Remove);
    }
}

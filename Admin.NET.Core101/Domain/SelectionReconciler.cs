namespace Admin.NET.Core101.Domain;

public sealed record SelectionDelta<TId>(IReadOnlyList<TId> Add, IReadOnlyList<TId> Remove);

public static class SelectionReconciler
{
    public static SelectionDelta<TId> Compare<TId>(IEnumerable<TId> existing, IEnumerable<TId> requested)
        where TId : notnull
    {
        var existingSet = existing.ToHashSet();
        var requestedSet = requested.ToHashSet();
        return new SelectionDelta<TId>(
            requestedSet.Except(existingSet).OrderBy(item => item.ToString(), StringComparer.Ordinal).ToArray(),
            existingSet.Except(requestedSet).OrderBy(item => item.ToString(), StringComparer.Ordinal).ToArray());
    }
}

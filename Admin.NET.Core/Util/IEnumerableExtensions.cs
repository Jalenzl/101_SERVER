namespace Admin.NET.Core;
public static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> objs, Action<T> action)
    {
        foreach (T obj in objs)
        {
            action(obj);
        }
    }
}

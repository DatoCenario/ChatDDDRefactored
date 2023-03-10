namespace Chatiks.Tools;

public static class CommonExtensions
{
    public static ICollection<T> EmptyIfNull<T>(this ICollection<T> collection)
    {
        return collection ?? new List<T>();
    }
}
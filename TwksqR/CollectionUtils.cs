namespace Twksqr.Collections.Generic;

public static class Utils
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return collection.Any() || (collection == null);
    }
}
using System.Linq.Expressions;

namespace SindiOps.API.Helpers;

public static class ListSort
{
    public static string? Key(string? sort, params string[] allowed)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return null;

        foreach (var allowedKey in allowed)
        {
            if (string.Equals(allowedKey, sort.Trim(), StringComparison.OrdinalIgnoreCase))
                return allowedKey;
        }

        return null;
    }

    public static bool Descending(string? dir) =>
        string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);

    public static IOrderedQueryable<T> By<T, TKey>(
        IQueryable<T> query,
        Expression<Func<T, TKey>> key,
        bool descending) =>
        descending ? query.OrderByDescending(key) : query.OrderBy(key);
}

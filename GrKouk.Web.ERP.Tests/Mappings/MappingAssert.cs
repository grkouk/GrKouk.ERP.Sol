using System.Reflection;

namespace GrKouk.Web.ERP.Tests.Mappings;

/// <summary>
/// Guards hand-written mappings against "forgotten" properties: fills every simple
/// property of the source with a distinct non-default value, runs the mapping, and
/// asserts that each destination property with the same name received that value.
/// </summary>
public static class MappingAssert
{
    public static TDest AllMatchingPropertiesCopied<TSrc, TDest>(
        Func<TSrc, TDest> map, params string[] ignore) where TSrc : new()
    {
        var source = CreateFilled<TSrc>();
        var dest = map(source);
        Assert.NotNull(dest);

        var destProps = typeof(TDest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name);

        var failures = new List<string>();
        var checkedCount = 0;
        foreach (var sp in SimpleProps(typeof(TSrc)))
        {
            if (ignore.Contains(sp.Name) || !destProps.TryGetValue(sp.Name, out var dp))
                continue;

            checkedCount++;
            var expected = sp.GetValue(source);
            var actual = dp.GetValue(dest);
            if (!Equals(Normalize(expected), Normalize(actual)))
                failures.Add($"{sp.Name}: expected '{expected}', got '{actual}'");
        }

        Assert.True(checkedCount > 0,
            $"No matching properties between {typeof(TSrc).Name} and {typeof(TDest).Name}.");
        Assert.True(failures.Count == 0,
            $"{typeof(TSrc).Name} -> {typeof(TDest).Name} did not copy:\n  " + string.Join("\n  ", failures));
        return dest;
    }

    /// <summary>Creates an instance whose simple properties all hold distinct non-default values.</summary>
    public static T CreateFilled<T>() where T : new()
    {
        var obj = new T();
        var seed = 1;
        foreach (var p in SimpleProps(typeof(T)).Where(p => p.CanWrite))
            p.SetValue(obj, SampleValue(p.PropertyType, p.Name, seed++));
        return obj;
    }

    private static IEnumerable<PropertyInfo> SimpleProps(Type t) =>
        t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsSimple(p.PropertyType));

    private static bool IsSimple(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
               || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid)
               || t == typeof(TimeSpan) || t == typeof(DateOnly) || t == typeof(TimeOnly);
    }

    private static object SampleValue(Type type, string name, int seed)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t == typeof(string)) return $"{name}_{seed}";
        if (t == typeof(bool)) return true;
        if (t == typeof(decimal)) return seed + 0.25m;
        if (t == typeof(double)) return seed + 0.25d;
        if (t == typeof(float)) return seed + 0.25f;
        if (t == typeof(DateTime)) return new DateTime(2020, 1, 1).AddDays(seed);
        if (t == typeof(DateTimeOffset)) return new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(seed);
        if (t == typeof(DateOnly)) return new DateOnly(2020, 1, 1).AddDays(seed);
        if (t == typeof(TimeOnly)) return new TimeOnly(0, 0).AddMinutes(seed);
        if (t == typeof(TimeSpan)) return TimeSpan.FromMinutes(seed);
        if (t == typeof(Guid)) return new Guid(seed, 0, 0, new byte[8]);
        if (t.IsEnum)
        {
            // Pick a non-default member so a missed copy is detectable.
            var values = Enum.GetValues(t);
            return values.GetValue(values.Length - 1)!;
        }
        return Convert.ChangeType(seed, t); // int, long, short, byte, ...
    }

    // Lets e.g. an int-backed enum compare equal to an int on the other side.
    private static object? Normalize(object? v) =>
        v is Enum e ? Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType())) : v;
}

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// The primary-key CLR types <c>DbContextBuilder&lt;T&gt;.EnsureUniqueRandomPrimaryKeys</c> renumbers,
/// with the largest key each can hold. Kept outside the generic builder so one table is shared by
/// every closed <c>DbContextBuilder&lt;T&gt;</c>.
/// </summary>
internal static class IntegralKeyTypes
{
    internal static readonly IReadOnlyDictionary<Type, long> MaxValues = new Dictionary<Type, long>
    {
        [typeof(int)] = int.MaxValue,
        [typeof(long)] = long.MaxValue,
        [typeof(short)] = short.MaxValue,
        [typeof(byte)] = byte.MaxValue,
    };
}

using Dapper;

namespace Scribbly.Infrastructure.Repositories;

internal static class CredentialHashSql
{
    public static DbString AsParameter(string hash)
    {
        if (hash.Length != 64)
        {
            throw new ArgumentException("Credential hash must be SHA-256 hex.", nameof(hash));
        }
        return new DbString { Value = hash, IsAnsi = true, IsFixedLength = true, Length = 64 };
    }
}

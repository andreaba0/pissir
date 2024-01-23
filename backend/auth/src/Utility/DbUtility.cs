using System.Data;
using System.Data.Common;

namespace Utility;

public static class DbUtility {
    public static DbParameter CreateParameter(DbConnection connection, DbType type, object? value)
    {
        DbParameter parameter = DbProviderFactories.GetFactory(connection).CreateParameter();
        parameter.DbType = type;
        parameter.Value = value;
        return parameter;
    }
}
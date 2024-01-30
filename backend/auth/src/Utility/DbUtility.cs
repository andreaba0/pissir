using System.Data;
using System.Data.Common;

namespace Utility;

public static class DbUtility
{
    public static DbParameter CreateParameter(DbConnection connection, DbType type, object? value, bool shouldThrow = true)
    {
        if (value == null&&shouldThrow) throw new DbUtilityException(DbUtilityException.ErrorCode.VALUE_PARAMETER_NULL, "Value parameter null");
        DbProviderFactory? factory = DbProviderFactories.GetFactory(connection);
        if (factory == null) throw new DbUtilityException(DbUtilityException.ErrorCode.DB_PROVIDER_FACTORY_NOT_FOUND, "Db provider factory not found");
        DbParameter? parameter = factory.CreateParameter();
        if (parameter == null) throw new DbUtilityException(DbUtilityException.ErrorCode.DB_PROVIDER_NOT_FOUND, "Db provider not found");
        parameter.DbType = type;
        parameter.Value = value;
        return parameter;
    }
}

public class DbUtilityException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        UNKNOW_ERROR = 1,
        DB_PROVIDER_FACTORY_NOT_FOUND = 2,
        DB_PROVIDER_NOT_FOUND = 3,
        VALUE_PARAMETER_NULL = 4,
    }
    public ErrorCode Code { get; set; }
    public DbUtilityException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }
    public DbUtilityException(ErrorCode code, string message, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}
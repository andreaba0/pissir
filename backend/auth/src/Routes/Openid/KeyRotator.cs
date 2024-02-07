using System;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using Utility;
using System.Net.Http.Headers;
using Module.Middleware;

namespace Routes;

public class KeyRotator
{
    public static Task PostMethod_Rotate(
        IHeaderDictionary headers,
        DbDataSource dataSource
    )
    {
        try
        {
            string authorization = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            Console.WriteLine(authorization);
            string token = Authentication.ParseBearerToken(authorization, Authentication.SchemeList.INTERNAL);
            if(token != "prova") throw new Exception("Invalid token");
            RSA rsa = RSA.Create();
            RSAParameters rsaParameters = rsa.ExportParameters(true);
            Guid guid = Guid.NewGuid();

            using DbConnection connection = dataSource.CreateConnection();
            connection.Open();
            using DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO rsa (
                    id,
                    d,
                    dp,
                    dq,
                    exponent,
                    inverse_q,
                    modulus,
                    p,
                    q
                ) VALUES (
                    $1,
                    $2,
                    $3,
                    $4,
                    $5,
                    $6,
                    $7,
                    $8,
                    $9
                )
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, guid));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.D));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.DP));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.DQ));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.Exponent));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.InverseQ));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.Modulus));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.P));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Binary, rsaParameters.Q));
            command.ExecuteNonQuery();
            return Task.CompletedTask;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
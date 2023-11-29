using System;
using System.Data.Common;

namespace Module.Accountant;

public class SignupService: IDisposable {

    private readonly DbDataSource _dbDataSource;
    private readonly DbConnection _dbConnection;

    public SignupService(DbDataSource dbDataSource) {
        _dbDataSource = dbDataSource;
    }

    public async Task<int> Routine(CancellationToken ct) {
        return 0;
    }

    public void Dispose() {
        _dbConnection?.Dispose();
    }
}
using System.Data.Common;

namespace Route;

public class PostWaterSell {

    private readonly DbDataSource _dbDataSource;
    public PostWaterSell(
        DbDataSource dbDataSource
    ) {
        this._dbDataSource = dbDataSource;
    }

}
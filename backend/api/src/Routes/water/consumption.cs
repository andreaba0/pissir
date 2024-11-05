namespace Routes;

public class WaterConsumption
{
    public struct GetData
    {
        public string field_id;
        public DateTime data;
        public float amount_used;
        public float amount_ordered;
    }
    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        RouteValueDictionary routeValues,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA, User.Role.FA });

        using DbConnection connection = dbDataSource.OpenConnection();
        DbCommand command = dbDataSource.CreateCommand();
        if (User.GetRole(user) == User.Role.FA)
        {
            command.CommandText = $@"
                select farm_field_id, sum(qty) as amount_ordered, offer.publish_date as publish_date, (
                    select sum(water_used) as amount_used
                    from actuator_log as al inner join object_logger as ol on al.object_id=ol.id
                    where ol.farm_field_id = farm_field_id and date_trunc('day', al.timestamp) = date_trunc('day', offer.publish_date)
                ) as amount_used
                from buy_order inner join offer
                on buy_order.offer_id = offer.id
                where vat_number = $1
                group by farm_field_id, publish_date
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        }
        else
        {
            command.CommandText = $@"
                select ff.id, sum(purchased_liters) as amount_ordered, offer.publish_date as publish_date, (
                    select sum(water_used) as amount_used
                    from actuator_log as al inner join object_logger as ol on al.object_id=ol.id
                    where ol.farm_field_id = ff.id and date_trunc('day', al.timestamp) = date_trunc('day', offer.publish_date)
                )
                from buy_order inner join offer
                on buy_order.offer_id = offer.id
                inner join farm_field as ff on buy_order.farm_field_id = ff.id
                where offer.vat_number = $1
                group by ff.id, publish_date
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        }
        using DbDataReader reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            return new ValueTask<string>("[]");
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            GetData item = new GetData()
            {
                field_id = reader.GetString(0),
                data = reader.GetDateTime(2),
                amount_ordered = reader.GetFloat(1),
                amount_used = reader.GetFloat(3)
            };
        }
        return new ValueTask<string>(JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            IncludeFields = true
        }));
    }
}
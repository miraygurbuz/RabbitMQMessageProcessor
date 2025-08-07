using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task TxtToSql(string json)
    {
        var jObj = JObject.Parse(json);

        var machineId = (string)jObj["data"]?["machineId"];
        var sensorType = (string)jObj["data"]?["sensorType"];
        var sensorValue = (string)jObj["data"]?["sensorValue"];
        var transactionId = (string)jObj["data"]?["transactionId"];
        var broadcastDate = (DateTime?)jObj["data"]?["broadcastDate"];

        using var conn = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO [Message]
                (MachineId, SensorType, SensorValue, TransactionId,
                 BroadcastDate) 
                VALUES 
                (@MachineId, @SensorType, @SensorValue, @TransactionId,
                 @BroadcastDate)";

        await conn.ExecuteAsync(sql, new
        {
            MachineId = machineId,
            SensorType = sensorType,
            SensorValue = sensorValue,
            TransactionId = transactionId,
            BroadcastDate = broadcastDate
        });
    }
}

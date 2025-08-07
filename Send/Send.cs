using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync("machine",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

int counter = 1;

while (true)
{
    var payload = new
    {
        data = new
        {
            machineId = "MACHINE987",
            sensorType = "VOLTAGE",
            sensorValue = "230.5",
            transactionId = "TRX" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
            broadcastDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        }
    };

    string message = JsonSerializer.Serialize(payload);
    var body = Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(exchange: string.Empty,
                                    routingKey: "machine",
                                    body: body,
                                    mandatory: true);

    Console.WriteLine($" [x] {counter} Sent {message}");
    counter++;
    await Task.Delay(2000);
}
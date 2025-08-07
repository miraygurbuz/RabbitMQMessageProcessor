using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class Receiver
{
    private readonly DbHelper _db;

    public Receiver(string connectionString)
    {
        _db = new DbHelper(connectionString);
    }

    public async Task Start()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "machine",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await ProcessUninsertedMessages();
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        });

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($" [x] Received {message}");

            Directory.CreateDirectory("incoming_messages");
            Directory.CreateDirectory("processed_messages");

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var unique = Guid.NewGuid().ToString().Substring(0, 6);
            var fileName = $"incoming_messages/{timestamp}_{unique}.txt";
            File.WriteAllText(fileName, message);

            try
            {
                await _db.TxtToSql(message);
                Console.WriteLine(" [x] Inserted into DB");
                var destFileName = $"processed_messages/{timestamp}_{unique}.txt";
                File.Move(fileName, destFileName);
                Console.WriteLine(" [x] Moved to processed_messages");

            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] DB insert error: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync("machine", autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public async Task ProcessUninsertedMessages()
    {
        Console.WriteLine(" [>] Moving files");
        var incomingPath = "incoming_messages";
        var processedPath = "processed_messages";

        Directory.CreateDirectory(incomingPath);
        Directory.CreateDirectory(processedPath);

        var files = Directory.GetFiles(incomingPath, "*.txt");

        foreach (var file in files)
        {
            try
            {
                var message = await File.ReadAllTextAsync(file);
                await _db.TxtToSql(message);
                Console.WriteLine($" [x] Re-inserted from file: {file}");

                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(processedPath, fileName);
                File.Move(file, destFile);

                Console.WriteLine($" [x] Moved to processed_messages: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Failed to insert from file {file}: {ex.Message}");
            }
        }
    }
}
using Azure.Storage.Queues;
using System.Text.Json;

namespace CoffeeNChill.Services
{
    public class QueueService   // Microsoft, 2026n; Microsoft, 2026o
    {
        private readonly QueueClient _queueClient;

        private QueueService(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public static async Task<QueueService> CreateAsync(string connectionString, string queueName)
        {
            var queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64   // Microsoft, 2026o
            });

            await queueClient.CreateIfNotExistsAsync();

            return new QueueService(queueClient);
        }

        public async Task EnqueueMessageAsync<T>(T payload) // rudderz243, 2026
        {
            string message = JsonSerializer.Serialize(payload);

            await _queueClient.SendMessageAsync(message);
        }
    }
}
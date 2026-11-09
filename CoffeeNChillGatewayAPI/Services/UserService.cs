using Azure.Data.Tables;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Services
{
    public class UserService    // Microsoft, 2026d; Microsoft, 2026g
    {
        private readonly TableClient _tableClient;

        private UserService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public static async Task<UserService> CreateAsync(string connectionString, string tableName)
        {
            var tableClient = new TableClient(connectionString, tableName);

            await tableClient.CreateIfNotExistsAsync();

            return new UserService(tableClient);
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            await foreach (UserEntity user in _tableClient.QueryAsync<UserEntity>(
                u => u.RowKey == email))
            {
                return user;
            }

            return null;
        }

        public async Task AddUserAsync(UserEntity newUser)
        {
            await _tableClient.AddEntityAsync(newUser);
        }
    }
}

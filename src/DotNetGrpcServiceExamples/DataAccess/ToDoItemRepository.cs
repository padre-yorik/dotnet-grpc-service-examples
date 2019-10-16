using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NewVoiceMedia.DotNetGrpcServiceExamples.Models;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.DataAccess
{

    public interface IToDoItemRepository
    {
        Task<IEnumerable<ToDoItem>> GetAllForAccount(ulong accountId);
        Task<ToDoItem> GetById(string id);
        Task Insert(ToDoItem schedule);
        Task Delete(string id);
    }

    public class ToDoItemRepository : IToDoItemRepository
    {
        private readonly DbOptions _config;

        public ToDoItemRepository(IOptions<DbOptions> dbOptions)
        {
            _config = dbOptions.Value;
        }

        public ToDoItemRepository(DbOptions config)
        {
            _config = config;
        }

        public async Task<IEnumerable<ToDoItem>> GetAllForAccount(ulong accountId)
        {
           const string sql =
                @"SELECT *
                  FROM dotnetcore_example.todoitem
                  WHERE AccountId = @accountId";

            return (await ReadTodos(sql, new { accountId }));
        }

        public async Task<ToDoItem> GetById(string id)
        {
            const string sql =
                @"SELECT *
                  FROM dotnetcore_example.todoitem
                  WHERE ToDoId = @id";

            return (await ReadTodos(sql, new { id })).SingleOrDefault();
        }

        public Task Insert(ToDoItem item)
        {
            const string sql =
                @"INSERT INTO dotnetcore_example.todoitem (ToDoId, AccountId, Name, IsComplete) 
                  VALUES (@ToDoId, @AccountId, @Name, @IsComplete);";

            return Save(item, sql);
        }

        public async Task Delete(string id)
        {
            const string sql =
                @"DELETE FROM dotnetcore_example.todoitem
                  WHERE ToDoId = @id;";

            using (var connection = new MySqlConnection(_config.WriteConnectionString))
            {
                await connection.ExecuteAsync(sql, new { id });
            }

        }

        private async Task<IEnumerable<ToDoItem>> ReadTodos(string sql, object param)
        {
            using (var connection = new MySqlConnection(_config.ReadConnectionString))
            {
                var todos = await connection.QueryAsync<ToDoItem>(sql, param);
                return todos;
            }
        }

        private async Task Save(ToDoItem item, string sql)
        {
            using (var connection = new MySqlConnection(_config.WriteConnectionString))
            {
                await connection.ExecuteAsync(sql, item);
            }
        }
    }
}
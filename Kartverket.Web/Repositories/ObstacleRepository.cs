using Dapper;
using Kartverket.Web.Data;
using MySqlConnector;
using System.Data;

namespace Kartverket.Web.Repositories
{

    public class RepositoryBase
    {
        protected string ConnectionString { get; }
        public RepositoryBase(string connectionString)
        {
            ConnectionString = connectionString;
        }
        protected IDbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }

    public class ObstacleRepository : RepositoryBase, IObstacleRepository
    {
        public ObstacleRepository(string connectionString) : base(connectionString)
        {
        }
      

        public async Task<IEnumerable<ObstacleData>> GetAllObstacleData()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT Id, ObstacleName, ObstacleHeight, ObstacleDescription 
                      FROM ObstacleData";
            return await connection.QueryAsync<ObstacleData>(sql);
        }
        public async Task<ObstacleData?> GetObstacleData(long id)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT Id, ObstacleName, ObstacleHeight, ObstacleDescription 
                      FROM ObstacleData
                      WHERE id = @id";
            return await connection.QueryFirstOrDefaultAsync<ObstacleData>(sql, new { id });
        }
        public async Task InsertObstacleData(ObstacleData data)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO 
                    ObstacleData (ObstacleName, ObstacleHeight, ObstacleDescription) 
                    VALUES (@ObstacleName, @ObstacleHeight, @ObstacleDescription)";
            await connection.ExecuteAsync(sql, data);
        }
    }

}

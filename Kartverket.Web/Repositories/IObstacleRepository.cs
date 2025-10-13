using Kartverket.Web.Data;
using System.Data;

namespace Kartverket.Web.Repositories
{
    public interface IObstacleRepository
    {
        Task InsertObstacleData(ObstacleData data);
        Task<IEnumerable<ObstacleData>> GetAllObstacleData();
    }
}
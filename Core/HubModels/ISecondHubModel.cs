using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.HubModels
{
    public interface ISecondHubModel
    {
        Task<IEnumerable<int>> GetRandomInts(int count, int min, int max);
    }
}

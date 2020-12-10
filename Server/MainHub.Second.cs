using Core.HubModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public partial class MainHub : ISecondHubModel
    {
        static Random rn = new Random();

        public Task<IEnumerable<int>> GetRandomInts(int count, int min, int max)
        {
            return Task.FromResult(Enumerable.Range(0, count).Select(x => rn.Next(min, max)));
        }
    }
}

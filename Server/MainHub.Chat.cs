using Core.HubModels;
using System.Threading.Tasks;

namespace Server
{
    public partial class MainHub : IChatHubModel
    {
        public Task SendMessage(string userName, string message)
        {
            return Clients.All.OnNewChatMessage(userName, message);
        }
    }
}

using System.Threading.Tasks;

namespace Core.HubEvents
{
    public interface IChatHubEvents
    {
        Task OnNewChatMessage(string userName, string message);
    }
}

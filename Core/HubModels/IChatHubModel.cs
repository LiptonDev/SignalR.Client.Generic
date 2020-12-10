using System.Threading.Tasks;

namespace Core.HubModels
{
    public interface IChatHubModel
    {
        Task SendMessage(string userName, string message);
    }
}

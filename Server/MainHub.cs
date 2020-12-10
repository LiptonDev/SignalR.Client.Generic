using Core.HubEvents;
using Microsoft.AspNetCore.SignalR;

namespace Server
{
    /* PARTIAL CLASS (!) */
    public partial class MainHub : Hub<IChatHubEvents>
    {
    }
}
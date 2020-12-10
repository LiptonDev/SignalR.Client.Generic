using Core.HubEvents;
using Core.HubModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client.Generic;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ProxyHubConnection hubConnection = new ProxyHubConnection(x => x.WithUrl("https://localhost:5001/mainhub").WithAutomaticReconnect());

            hubConnection.RegisterCallbacks(new ChatEvents());

            var chatProxy = hubConnection.GetHubProxy<IChatHubModel>();
            var secondProxy = hubConnection.GetHubProxy<ISecondHubModel>();

            await hubConnection.Connection.StartAsync();

            var randInts = await secondProxy.GetRandomInts(10, 0, 10);

            foreach (var item in randInts)
            {
                Console.WriteLine($"Random int: {item}");
            }

            Console.Write("Input name: ");
            var name = Console.ReadLine();

            while (true)
            {
                var msg = Console.ReadLine();

                await chatProxy.SendMessage(name, msg);
            }
        }
    }

    class ChatEvents : IChatHubEvents
    {
        public Task OnNewChatMessage(string userName, string message)
        {
            Console.WriteLine($"New message: {userName} => {message}");

            return Task.CompletedTask;
        }
    }
}

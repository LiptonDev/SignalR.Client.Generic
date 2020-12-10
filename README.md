# SignalR.Client.Generic

## Step 1
Create 'Core' project, which will contains 2 folders:
- HubEvents - contains interfaces, which describe server events.
- HubModels - contains interfaces, which describe server methods.

For current example folder 'HubEvents' contains next interface:
```C#
public interface IChatHubEvents
{
    Task OnNewChatMessage(string userName, string message);
}
```

And folder 'HubModels' contains next interfaces:

```C#
public interface IChatHubModel
{
    Task SendMessage(string userName, string message);
}

public interface ISecondHubModel
{
    Task<IEnumerable<int>> GetRandomInts(int count, int min, int max);
}
```

## Step 2
Create server and hub.  
Hub must implement next interfaces: IChatHubEvents, ISecondHubModel  
In example Hub was created as partial class:  
```C#
public partial class MainHub : Hub<IChatHubEvents>
{
}

public partial class MainHub : IChatHubModel
{
    public Task SendMessage(string userName, string message)
    {
        return Clients.All.OnNewChatMessage(userName, message);
    }
}

public partial class MainHub : ISecondHubModel
{
    static Random rn = new Random();

    public Task<IEnumerable<int>> GetRandomInts(int count, int min, int max)
    {
        return Task.FromResult(Enumerable.Range(0, count).Select(x => rn.Next(min, max)));
    }
}
```

Next step: add signalr to services
```C#
> Startup.cs

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<MainHub>("/mainhub");
        });
    }
}
```

## Step 3
Create ProxyHubConnection:
```C#
ProxyHubConnection hubConnection = new ProxyHubConnection(x => x.WithUrl("https://localhost:5001/mainhub").WithAutomaticReconnect());
```

ProxyHubConnection must not implement interfaces: IChatHubModel, ISecondHubModel, but must implement events (IChatHubEvents) in other classes.

ProxyHubConnect you can inherit and override methods: InvokeCoreAsync, InvokeCoreAsync<T>  

Next step: implement server events
```C#
class ChatEvents : IChatHubEvents
{
    public Task OnNewChatMessage(string userName, string message)
    {
        Console.WriteLine($"New message: {userName} => {message}");

        return Task.CompletedTask;
    }
}
```

Next step: register events in ProxyHubConnection
```C#
hubConnection.RegisterCallbacks(new ChatEvents());
```

Next step: create two HubProxy
```C#
var chatProxy = hubConnection.GetHubProxy<IChatHubModel>();
var secondProxy = hubConnection.GetHubProxy<ISecondHubModel>();
```

Now we ready connect to server (proxy may create after/before connection)
```C#
await hubConnection.Connection.StartAsync();
```

Well, we connected to server. Now we can invoke server method over proxy:
```C#
var randInts = await secondProxy.GetRandomInts(10, 0, 10);

foreach (var item in randInts)
{
    Console.WriteLine($"Random int: {item}");
}
```

It works!

Let's try to send message to chat:
```C#
Console.Write("Input name: ");
var name = Console.ReadLine();

while (true)
{
    var msg = Console.ReadLine();

    await chatProxy.SendMessage(name, msg);
}
```

using Castle.DynamicProxy;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Client.Generic
{
    /// <summary>
    /// Generic client for SignalR.
    /// </summary>
    /// <typeparam name="THubModel">Server hub model.</typeparam>
    public class ProxyHubConnection : IAsyncDisposable
    {
        /// <summary>
        /// A connection used to invoke hub methods on a SignalR Server.
        /// </summary>
        public HubConnection Connection { get; }

        /// <summary>
        /// Occurs when the connection is closed. The connection could be closed due to an 
        /// error or due to either the server or client intentionally closing the connection
        /// without error.
        /// </summary>
        public event Func<Exception, Task> Closed
        {
            add => Connection.Closed += value;
            remove => Connection.Closed -= value;
        }

        /// <summary>
        /// Occurs when the Microsoft.AspNetCore.SignalR.Client.HubConnection successfully
        /// reconnects after losing its underlying connection.
        /// </summary>
        public event Func<string, Task> Reconnected
        {
            add => Connection.Reconnected += value;
            remove => Connection.Reconnected -= value;
        }

        /// <summary>
        /// Occurs when the Microsoft.AspNetCore.SignalR.Client.HubConnection starts reconnecting
        /// after losing its underlying connection.
        /// </summary>
        public event Func<Exception, Task> Reconnecting
        {
            add => Connection.Reconnecting += value;
            remove => Connection.Reconnecting -= value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="createHubConnection">Create hub connection.</param>
        public ProxyHubConnection(Func<IHubConnectionBuilder, IHubConnectionBuilder> hubConnectionBuilder)
        {
            Connection = hubConnectionBuilder.Invoke(new HubConnectionBuilder()).Build();
        }

        /// <summary>
        /// Get hub proxy.
        /// </summary>
        /// <typeparam name="THubProxy">Hub proxy interface.</typeparam>
        /// <returns></returns>
        public THubProxy GetHubProxy<THubProxy>() where THubProxy : class
        {
            var service = new ProxyGenerator().CreateInterfaceProxyWithoutTarget<THubProxy>(new HubInterceptor(this));

            return service;
        }

        /// <summary>
        /// Register callbacks.
        /// </summary>
        /// <returns></returns>
        public IDisposable RegisterCallbacks<TCallbacks>(TCallbacks clientCallbacks) where TCallbacks : class
        {
            var methods = typeof(TCallbacks).GetMethods();
            var disposables = methods.Select(methodInfo =>
            {
                var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                return Connection.On(methodInfo.Name, parameterTypes, (parameters, _) => (Task)methodInfo.Invoke(clientCallbacks, parameters), null);
            }).ToArray();

            return new ListenerDisposable(disposables);
        }

        /// <summary>
        /// Invokes a hub method on the server using the specified method name and arguments.
        /// </summary>
        /// <param name="methodName">The name of the server method to invoke.</param>
        /// <param name="args">The arguments used to invoke the server method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns></returns>
        internal protected virtual Task InvokeCoreAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return Connection.InvokeCoreAsync(methodName, args, cancellationToken);
        }

        /// <summary>
        /// Invokes a hub method on the server using the specified method name, return type and arguments.
        /// </summary>
        /// <param name="methodName">The name of the server method to invoke.</param>
        /// <param name="returnType">The return type of the server method.</param>
        /// <param name="args">The arguments used to invoke the server method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns></returns>
        internal protected virtual async Task<T> InvokeCoreAsync<T>(string methodName, Type returnType, object[] args, CancellationToken cancellationToken = default)
        {
            var res = await Connection.InvokeCoreAsync(methodName, returnType, args, cancellationToken);
            return (T)res;
        }

        /// <summary>
        /// Disposes the Microsoft.AspNetCore.SignalR.Client.HubConnection.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}

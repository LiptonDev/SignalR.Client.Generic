using Castle.DynamicProxy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Client.Generic
{
    /// <summary>
    /// Dynamic proxy for hub interface.
    /// </summary>
    /// <typeparam name="THubModel"></typeparam>
    class HubInterceptor : StandardInterceptor
    {
        /// <summary>
        /// Hub connection.
        /// </summary>
        private readonly ProxyHubConnection connection;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HubInterceptor(ProxyHubConnection connection)
        {
            this.connection = connection;
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            var methodInfo = invocation.Method;

            if (methodInfo.ReturnType.IsGenericType)
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments().Single();
                var genericResult = connection.InvokeCoreAsync<object>(methodInfo.Name, returnType, invocation.Arguments);

                var sourceType = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
                var taskCompletionSource = Activator.CreateInstance(sourceType);

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        var result = await genericResult;
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetResult)).Invoke(taskCompletionSource, new[] { result });
                    }
                    catch (OperationCanceledException)
                    {
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetCanceled)).Invoke(taskCompletionSource, null);
                    }
                    catch (Exception e)
                    {
                        sourceType.GetMethod(nameof(TaskCompletionSource<object>.TrySetException)).Invoke(taskCompletionSource, new[] { e });
                    }
                });
                invocation.ReturnValue = sourceType.GetProperty(nameof(TaskCompletionSource<object>.Task)).GetValue(taskCompletionSource);
            }
            else
            {
                invocation.ReturnValue = connection.InvokeCoreAsync(methodInfo.Name, invocation.Arguments);
            }
        }
    }
}

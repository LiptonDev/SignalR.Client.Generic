using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Client.Generic
{
    internal class ListenerDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public ListenerDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables) disposable.Dispose();
        }
    }
}

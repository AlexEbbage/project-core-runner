using System;

namespace CoreRacer.Common.Events
{
    public sealed class DisposableSubscription : IDisposable
    {
        private Action _dispose;

        public DisposableSubscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_dispose == null)
                return;

            var action = _dispose;
            _dispose = null;
            action.Invoke();
        }
    }
}

using System;
using System.Collections.Generic;

namespace CoreRacer.Common.Events
{
    /// <summary>
    /// Simple typed event bus. Use for cross-system notifications; keep direct service calls for commands.
    /// </summary>
    public sealed class GameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(TEvent);
            List<Delegate> handlers;
            if (!_handlers.TryGetValue(type, out handlers))
            {
                handlers = new List<Delegate>();
                _handlers[type] = handlers;
            }

            handlers.Add(handler);
            return new DisposableSubscription(() => handlers.Remove(handler));
        }

        public void Publish<TEvent>(TEvent evt)
        {
            List<Delegate> handlers;
            if (!_handlers.TryGetValue(typeof(TEvent), out handlers))
                return;

            // Copy to prevent mutation during dispatch.
            var snapshot = handlers.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
                ((Action<TEvent>)snapshot[i]).Invoke(evt);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}

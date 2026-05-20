using System;
using System.Collections.Generic;

namespace CoreRacer.Common
{
    /// <summary>
    /// Lightweight service registry for a small Unity project. Keeps dependencies explicit without adding a heavy DI framework.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
        }

        public void Register(Type type, object service)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (!type.IsInstanceOfType(service))
                throw new ArgumentException($"Service instance does not implement {type.Name}.", nameof(service));

            _services[type] = service;
        }

        public T Get<T>() where T : class
        {
            object service;
            if (_services.TryGetValue(typeof(T), out service))
                return (T)service;

            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            object value;
            if (_services.TryGetValue(typeof(T), out value))
            {
                service = (T)value;
                return true;
            }

            service = null;
            return false;
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}

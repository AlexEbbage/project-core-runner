using System;
using CoreRacer.Common;

namespace CoreRacer.Bootstrap
{
    /// <summary>
    /// Process-wide registry. Keep this as a composition convenience, not as a dumping ground for logic.
    /// </summary>
    public static class GameServices
    {
        public static ServiceRegistry Registry { get; private set; }

        public static bool IsReady => Registry != null;

        /// <summary>
        /// Raised after a complete registry is installed or cleared. Consumers which may be enabled
        /// before the bootstrap scene can use this to retry dependency resolution safely.
        /// </summary>
        public static event Action<ServiceRegistry> RegistryChanged;

        public static void SetRegistry(ServiceRegistry registry)
        {
            if (ReferenceEquals(Registry, registry))
                return;

            Registry = registry;
            RegistryChanged?.Invoke(Registry);
        }

        public static void ClearRegistry(ServiceRegistry expectedRegistry = null)
        {
            if (expectedRegistry != null && !ReferenceEquals(Registry, expectedRegistry))
                return;

            Registry = null;
            RegistryChanged?.Invoke(null);
        }

        public static T Get<T>() where T : class
        {
            if (Registry == null)
                throw new InvalidOperationException("Core Racer services are not ready. Ensure GameBootstrapper is active and executes first.");

            return Registry.Get<T>();
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            service = null;
            return Registry != null && Registry.TryGet(out service);
        }
    }
}

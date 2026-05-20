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

        public static void SetRegistry(ServiceRegistry registry)
        {
            Registry = registry;
        }

        public static T Get<T>() where T : class
        {
            return Registry.Get<T>();
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            service = null;
            return Registry != null && Registry.TryGet(out service);
        }
    }
}

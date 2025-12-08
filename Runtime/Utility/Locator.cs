namespace Shared.Foundation
{
    public static class Locator<T> where T : class
    {
        private static bool _locked;
        public static T Instance { get; private set; }
        public static bool IsSet => Instance != null;

        /// <summary>Đặt service. Nếu lockOnce=true, chỉ cho set lần đầu.</summary>
        public static void Set(T instance, bool lockOnce = false)
        {
            if (instance == null) throw new System.ArgumentNullException(nameof(instance));
            if (_locked && Instance != null && !ReferenceEquals(Instance, instance))
                throw new System.InvalidOperationException($"{typeof(T).Name} already set and locked.");

            Instance = instance;
            _locked |= lockOnce;
        }

        public static bool TryGet(out T svc) { svc = Instance; return svc != null; }

        public static void Clear()
        {
            if (_locked) return; // tùy chọn: không cho clear khi đã lock
            Instance = null;
        }
    }

}

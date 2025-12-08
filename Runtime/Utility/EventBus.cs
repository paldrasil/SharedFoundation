using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(in T e);
        void Clear(); // hủy tất cả listener (khi rời scope)
    }

    public sealed class EventBus : IEventBus
    {
        readonly Dictionary<Type, List<Delegate>> _map = new();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_map.TryGetValue(t, out var list)) _map[t] = list = new();
            list.Add(handler);
            return new Subscription(() => list.Remove(handler));
        }
        public void Unsubscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_map.TryGetValue(t, out var list)) return;
            list.Remove(handler);
        }
        public void Publish<T>(in T e)
        {
            if (_map.TryGetValue(typeof(T), out var list))
                for (int i = 0; i < list.Count; i++) ((Action<T>)list[i]).Invoke(e);
        }
        public void Clear() => _map.Clear();

        sealed class Subscription : IDisposable
        {
            readonly Action _dispose; bool _done;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose() { if (_done) return; _done = true; _dispose(); }
        }
    }
}

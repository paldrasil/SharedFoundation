using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public interface ITrackable
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }

    public sealed class ObjectRegistry : MonoBehaviour
    {
        // Type -> set of instances
        private readonly Dictionary<Type, HashSet<ITrackable>> _map = new();
        
        // Type -> singleton instance
        private readonly Dictionary<Type, ITrackable> _singletons = new();

        public event System.Action<ITrackable> OnAdded;
        public event System.Action<ITrackable> OnRemoved;

        public void Register<T>(T obj) where T : class, ITrackable
        {
            var type = typeof(T);
            if (!_map.TryGetValue(type, out var set))
            {
                set = new HashSet<ITrackable>();
                _map[type] = set;
            }
            set.Add(obj);
            OnAdded?.Invoke(obj);
        }

        public void Unregister<T>(T obj) where T : class, ITrackable
        {
            var type = typeof(T);
            if (_map.TryGetValue(type, out var set))
            {
                set.Remove(obj);
                OnRemoved?.Invoke(obj);
                if (set.Count == 0) _map.Remove(type);
            }
        }

        // Trả về read-only view để duyệt nhanh (không alloc list mới)
        public IReadOnlyCollection<ITrackable> QueryRaw(Type type)
        {
            return _map.TryGetValue(type, out var set) ? (IReadOnlyCollection<ITrackable>)set : Array.Empty<ITrackable>();
        }

        // Query<T> (alloc ít): fill vào results do caller truyền vào
        public void QueryNonAlloc<T>(List<T> results) where T : class, ITrackable
        {
            results.Clear();
            if (_map.TryGetValue(typeof(T), out var set))
            {
                foreach (var it in set)
                    if (it is T t) results.Add(t);
            }
        }

        // Query<T> (alloc list mới) – tiện khi không quan trọng GC
        public List<T> Query<T>() where T : class, ITrackable
        {
            var list = new List<T>();
            QueryNonAlloc(list);
            return list;
        }

        public List<ITrackable> QueryAll()
        {
            var list = new List<ITrackable>();
            foreach (var p in _map)
            {
                foreach (var it in p.Value)
                    list.Add(it);
            }
            return list;
        }

        public void QueryAllNonAlloc(List<ITrackable> results)
        {
            results.Clear();
            foreach (var p in _map)
            {
                foreach (var it in p.Value)
                    results.Add(it);
            }
        }

        public List<T> QueryAllComponentsOfType<T>() where T : Component  
        {
            var list = new List<T>();
            foreach (var p in _map)
            {
                foreach (var it in p.Value)
                {
                    var component = it.gameObject.GetComponent<T>();
                    if (component)
                    {
                        list.Add(component);
                    }
                }
            }
            return list;
        }

        public void QueryAllComponentsOfTypeNonAlloc<T>(List<T> results) where T : Component
        {
            results.Clear();
            foreach (var p in _map)
            {
                foreach (var it in p.Value)
                {
                    var component = it.gameObject.GetComponent<T>();
                    if (component)
                    {
                        results.Add(component);
                    }
                }
            }
        }

        // Singleton methods
        public void RegisterSingleton<T>(T obj) where T : class, ITrackable
        {
            var type = typeof(T);
            
            // Nếu đã có singleton cũ, unregister nó trước
            if (_singletons.TryGetValue(type, out var oldSingleton))
            {
                Unregister(oldSingleton);
            }
            
            // Register singleton mới
            _singletons[type] = obj;
            Register(obj);
        }

        public T GetSingleton<T>() where T : class, ITrackable
        {
            var type = typeof(T);
            return _singletons.TryGetValue(type, out var singleton) ? (T)singleton : null;
        }

        public bool HasSingleton<T>() where T : class, ITrackable
        {
            return _singletons.ContainsKey(typeof(T));
        }

        public void UnregisterSingleton<T>() where T : class, ITrackable
        {
            var type = typeof(T);
            if (_singletons.TryGetValue(type, out var singleton))
            {
                _singletons.Remove(type);
                Unregister(singleton);
            }
        }

        public void Cleanup()
        {
            _map.Clear();
            _singletons.Clear();
        }
    }

}

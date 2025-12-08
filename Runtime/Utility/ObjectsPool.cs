using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shared.Foundation
{
    public class ObjectsPool : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public string key;
            public GameObject prefab;
            [Min(0)] public int preload = 8;
            public Transform defaultParent; // optional
        }

        [Header("Initial Entries (optional)")]
        public List<Entry> initialEntries = new();

        // prefabId -> queue
        private readonly Dictionary<int, Queue<GameObject>> _queues = new();
        // prefabId -> default parent
        private readonly Dictionary<int, Transform> _defaultParents = new();

        private void Awake()
        {
            WarmupAll();
        }

        #region Public API

        public void WarmupAll()
        {
            foreach (var e in initialEntries)
            {
                if (e?.prefab == null) continue;
                Preload(e.prefab, e.preload, e.defaultParent);
            }
        }

        public void Preload(GameObject prefab, int count, Transform defaultParent = null)
        {
            if (prefab == null) return;

            int id = prefab.GetInstanceID();
            if (!_queues.ContainsKey(id))
            {
                _queues[id] = new Queue<GameObject>(count);
                _defaultParents[id] = defaultParent ? defaultParent : transform;
            }
            else if (defaultParent && _defaultParents[id] != defaultParent)
            {
                _defaultParents[id] = defaultParent;
            }

            for (int i = 0; i < count; i++)
                _queues[id].Enqueue(CreateInstance(prefab, _defaultParents[id]));
        }

        public GameObject FindPrefab(string prefabKey)
        {
            var entry = initialEntries.FirstOrDefault(x => x.key == prefabKey);
            if (entry == null) return null;
            return entry.prefab;
        }

        public GameObject Spawn(string prefabKey, Vector3 pos, Quaternion rot, float life = -1f, Transform parentOverride = null)
        {
            var entry = initialEntries.FirstOrDefault(x => x.key == prefabKey);
            if (entry == null) return null;
            return Spawn(entry.prefab, pos, rot, life, parentOverride ? parentOverride : entry.defaultParent);
        }

        public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, float life = -1f, Transform parentOverride = null)
        {
            if (prefab == null) return null;

            int id = prefab.GetInstanceID();
            if (!_queues.TryGetValue(id, out var q))
            {
                // on-demand create sub-queue
                _queues[id] = q = new Queue<GameObject>();
                _defaultParents[id] = transform;
            }

            var go = q.Count > 0 ? q.Dequeue() : CreateInstance(prefab, _defaultParents[id]);

            bool isUI = go.TryGetComponent<RectTransform>(out var rt);

            // Parent
            if (parentOverride)
                go.transform.SetParent(parentOverride, worldPositionStays: !isUI);
            else if (go.transform.parent != _defaultParents[id])
                go.transform.SetParent(_defaultParents[id], worldPositionStays: !isUI);

            // Placement
            if (isUI)
            {
                // UI đặt tọa độ sau (vd convert world->canvas); reset transform
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }
            else
            {
                go.transform.SetPositionAndRotation(pos, rot);
            }

            go.SetActive(true);

            if (life > 0f) StartCoroutine(DespawnAfter(go, life));
            return go;
        }

        public void Despawn(GameObject go)
        {
            if (!go) return;
            if (!go.TryGetComponent(out PooledMeta meta) || meta.owner != this)
            {
                // không thuộc pool này → destroy cho an toàn
                Destroy(go);
                return;
            }

            // Reset common state
            if (go.TryGetComponent<ParticleSystem>(out var ps))
            {
                ps.Clear(true);
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            go.SetActive(false);

            // Trả về default parent
            if (_defaultParents.TryGetValue(meta.prefabKeyId, out var def) && go.transform.parent != def)
                go.transform.SetParent(def, false);

            if (!_queues.TryGetValue(meta.prefabKeyId, out var q))
                _queues[meta.prefabKeyId] = q = new Queue<GameObject>();

            q.Enqueue(go);
        }

        public void ClearPrefab(GameObject prefab, bool destroy = true)
        {
            if (!prefab) return;
            int id = prefab.GetInstanceID();
            if (_queues.TryGetValue(id, out var q))
            {
                if (destroy)
                    foreach (var go in q) if (go) Destroy(go);
                q.Clear();
            }
            _queues.Remove(id);
            _defaultParents.Remove(id);
        }

        public void ClearAll(bool destroy = true)
        {
            if (destroy)
            {
                foreach (var kv in _queues)
                    foreach (var go in kv.Value) if (go) Destroy(go);
            }
            _queues.Clear();
            _defaultParents.Clear();
        }

        #endregion

        #region Internal

        private GameObject CreateInstance(GameObject prefab, Transform parent)
        {
            var go = Instantiate(prefab, parent);
            go.SetActive(false);

            var meta = go.GetComponent<PooledMeta>();
            if (!meta) meta = go.AddComponent<PooledMeta>();
            meta.owner = this;
            meta.prefabKeyId = prefab.GetInstanceID();

            return go;
        }

        public System.Collections.IEnumerator DespawnAfter(GameObject go, float t)
        {
            yield return new WaitForSeconds(t);
            Despawn(go);
        }

        private class PooledMeta : MonoBehaviour
        {
            public ObjectsPool owner;
            public int prefabKeyId;
        }

        #endregion
    }

}

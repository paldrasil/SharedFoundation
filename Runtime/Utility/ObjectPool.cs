
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Entry { public GameObject prefab; public int preload = 8; }

        public Entry config;
        public Transform defaultParent;

        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        void Awake()
        {
            if (config?.prefab == null) return;
            for (int i = 0; i < config.preload; i++) _pool.Enqueue(Create());
        }

        GameObject Create()
        {
            var parent = defaultParent ? defaultParent : transform;
            var go = Instantiate(config.prefab, parent);
            go.SetActive(false);
            return go;
        }

        public GameObject Spawn(Vector3 pos, Quaternion rot, float life = -1f, Transform parentOverride = null)
        {
            var go = _pool.Count > 0 ? _pool.Dequeue() : Create();
            if (parentOverride && go.transform.parent != parentOverride)
                go.transform.SetParent(parentOverride, worldPositionStays: false);

            //go.transform.SetPositionAndRotation(pos, rot);
            go.SetActive(true);
            if (go.TryGetComponent<RectTransform>(out var rt))
            {
                // Giữ nguyên anchoredPosition, sẽ được đặt bởi spawner (theo screen/canvas)
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }
            else
            {
                go.transform.SetPositionAndRotation(pos, rot);
            }

            if (life > 0f) StartCoroutine(DespawnAfter(go, life));
            return go;
        }

        System.Collections.IEnumerator DespawnAfter(GameObject go, float t)
        {
            yield return new WaitForSeconds(t);
            Despawn(go);
        }

        public void Despawn(GameObject go)
        {
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }
}

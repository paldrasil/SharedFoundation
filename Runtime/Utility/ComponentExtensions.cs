using System;
using UnityEngine;

namespace Shared.Foundation
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Lấy component nếu có, nếu không thì Add mới và trả về.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }

        public static T GetOrAddComponent<T>(this Component c) where T : Component
        {
            return c.gameObject.GetOrAddComponent<T>();
        }

        public static Transform FindRecursive(this Transform self, string exactName) => self.FindRecursive(child => child.name == exactName);

        public static Transform FindRecursive(this Transform self, Func<Transform, bool> selector)
        {
            foreach (Transform child in self)
            {
                if (selector(child))
                {
                    return child;
                }

                var finding = child.FindRecursive(selector);

                if (finding != null)
                {
                    return finding;
                }
            }

            return null;
        }
    }
}

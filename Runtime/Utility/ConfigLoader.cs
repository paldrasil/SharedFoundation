using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Shared.Foundation
{
    public static class ConfigLoader<T> where T : new()
    {
        [System.Serializable] private class Wrapper<U> { public int version; public List<U> items; }

        public static List<T> FromResource(string resourcePath)
        {
            var baseConfigs = JsonConvert.DeserializeObject<Wrapper<T>>(Resources.Load<TextAsset>(resourcePath).text).items;
            return baseConfigs;
        }

        public static List<T> FromJsonText(string jsonText)
        {
            if(string.IsNullOrEmpty(jsonText))
                return new List<T>();
       
            var root = JSON.Parse(jsonText);
            var items = root["items"].AsArray;
            if (items == null)
                throw new System.Exception($"Invalid config format: {jsonText}. Expected 'items' array.");

            var result = new List<T>();
            foreach (var child in items.Children)
            {
                var constructor = typeof(T).GetConstructor(new[] { typeof(JSONNode) });
                if (constructor == null) {
                    throw new System.Exception($"Constructor not found for {typeof(T)}");
                }
                var mapped = (T)constructor.Invoke(new object[] { child });
                result.Add(mapped);
            }
            return result;
        }
    }
}

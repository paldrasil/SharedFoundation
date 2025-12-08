using UnityEngine;
using UnityEditor;
using System.IO;

namespace Shared.Foundation
{
    public class DatabaseSOSyncEditor
    {
        public static T LoadOrCreateDataItem<T>(string id, string subFoler, out bool isCreate) where T : ScriptableObject
        {
            isCreate = false;
            T asset = default(T);
            if (string.IsNullOrEmpty(id))
            {
                return asset;
            }
    #if UNITY_EDITOR
            string path = $"{subFoler}/{id}.asset";
            if (File.Exists(path))
            {
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                isCreate = true;
            }
    #endif
            return asset;

        }

        public static T SyncItemDatabase<T>() where T : ScriptableObject
        {
    #if UNITY_EDITOR
            string dbName = typeof(T).Name;
            //dbName = dbName.Substring(0, dbName.Length - 2);
            string databasePath = $"Assets/Resources/Databases/{dbName}.asset";

            var db = AssetDatabase.LoadAssetAtPath<T>(databasePath);
            if (db == null)
            {
                Debug.LogError($"❌ {dbName} not found at " + databasePath);
                return default(T);
            }
            var field = typeof(T).GetField("items");
            if (field == null)
            {
                Debug.LogError($"❌ {dbName} don't have  items field");
                return default(T);
            }

            var itemsList = field.GetValue(db) as System.Collections.IList;
            if (itemsList == null)
            {
                Debug.LogError($"❌ 'items' field in {dbName} is not a list");
                return default;
            }

            itemsList.Clear();

            string searchPath = "Assets/StandardAssets/ScriptableObjects";
            var elementType = field.FieldType.GetGenericArguments()[0];
            string[] guids = AssetDatabase.FindAssets($"t:{elementType.Name}", new[] { searchPath });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, elementType);
                if (asset != null)
                {
                    itemsList.Add(asset);
                }
            }
            EditorUtility.SetDirty(db);
    #endif
            return db;
        }
    }
}

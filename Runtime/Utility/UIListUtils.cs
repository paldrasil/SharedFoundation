using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public static class UIListUtils
    {
        /// <summary>
        /// Tạo danh sách item UI từ prefab, có thể ẩn bớt nếu dư.
        /// </summary>
        /// <typeparam name="T">Kiểu component bạn muốn lấy (vd: SkillNodeLevel)</typeparam>
        /// <param name="prefab">Prefab mẫu</param>
        /// <param name="parent">Parent transform chứa các item</param>
        /// <param name="list">Danh sách các item hiện có</param>
        /// <param name="count">Số lượng item cần tạo/hiện</param>
        /// <param name="onSetup">Callback sau khi item được hiển thị</param>
        public static void SyncListItems<T>(
            GameObject prefab,
            Transform parent,
            List<T> list,
            int count,
            Action<T, int> onSetup = null
        ) where T : Component
        {
            // Tạo thêm nếu thiếu
            while (list.Count < count)
            {
                var go = GameObject.Instantiate(prefab, parent);
                var comp = go.GetComponent<T>();
                list.Add(comp);
            }

            // Ẩn bớt nếu dư
            for (int i = count; i < list.Count; i++)
                list[i].gameObject.SetActive(false);

            // Hiển thị và gọi setup
            for (int i = 0; i < count; i++)
            {
                list[i].gameObject.SetActive(true);
                onSetup?.Invoke(list[i], i);
            }
        }
    }

}

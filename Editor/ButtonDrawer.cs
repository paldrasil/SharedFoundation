#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shared.Foundation;
using UnityEditor;
using UnityEngine;

namespace Shared.Foundation
{
    static class ButtonCache
    {
        // Cache theo Type -> entries đã sắp xếp
        static readonly Dictionary<Type, List<ButtonEntry>> _cache = new();

        public static List<ButtonEntry> Get(Type t)
        {
            if (_cache.TryGetValue(t, out var entries)) return entries;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = t.GetMethods(flags)
                .Where(m =>
                    m.GetCustomAttributes(typeof(ButtonAttribute), true).Any() &&
                    m.GetParameters().Length == 0 // bản nhẹ: không tham số
                );

            var list = new List<ButtonEntry>();
            foreach (var m in methods)
            {
                foreach (ButtonAttribute attr in m.GetCustomAttributes(typeof(ButtonAttribute), true))
                {
                    list.Add(new ButtonEntry(m, attr));
                }
            }
            // Sắp xếp theo Order rồi theo tên để ổn định
            list.Sort((a, b) =>
            {
                int o = a.Attr.Order.CompareTo(b.Attr.Order);
                return o != 0 ? o : string.Compare(a.Method.Name, b.Method.Name, StringComparison.Ordinal);
            });

            _cache[t] = list;
            return list;
        }

        public class ButtonEntry
        {
            public readonly MethodInfo Method;
            public readonly ButtonAttribute Attr;
            public readonly bool IsStatic;

            public ButtonEntry(MethodInfo method, ButtonAttribute attr)
            {
                Method = method;
                Attr = attr;
                IsStatic = method.IsStatic;
            }
        }
    }

    abstract class BaseButtonInspector<T> : Editor where T : UnityEngine.Object
    {
        public override void OnInspectorGUI()
        {
            // Vẽ Inspector gốc trước
            DrawDefaultInspector();

            var type = target.GetType();
            var entries = ButtonCache.Get(type);
            if (entries.Count == 0) return;

            // Gom theo group để có thể xếp hàng
            var groups = entries.GroupBy(e => e.Attr.Group ?? string.Empty);

            EditorGUILayout.Space();
            foreach (var g in groups)
            {
                var groupName = g.Key;
                if (!string.IsNullOrEmpty(groupName))
                {
                    EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
                }

                // Mini thì xếp ngang
                var minis = g.Where(e => e.Attr.Style == ButtonStyle.Mini).ToList();
                var normals = g.Where(e => e.Attr.Style == ButtonStyle.Normal).ToList();
                var larges = g.Where(e => e.Attr.Style == ButtonStyle.Large).ToList();

                if (minis.Count > 0)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        foreach (var e in minis) DrawButton(e);
                    }
                }

                foreach (var e in normals) DrawButton(e);
                foreach (var e in larges) DrawButton(e, height: 30);
            }
        }

        void DrawButton(ButtonCache.ButtonEntry entry, float? height = null)
        {
            // Ẩn theo VisibleIf
            if (!EvaluateBool(entry, entry.Attr.VisibleIf, defaultValue: true))
                return;

            // Tính enable theo Playmode
            bool enableByMode =
                entry.Attr.EnableMode == ButtonEnableMode.Always ||
                (entry.Attr.EnableMode == ButtonEnableMode.EditorOnly && !Application.isPlaying) ||
                (entry.Attr.EnableMode == ButtonEnableMode.PlaymodeOnly && Application.isPlaying);

            // EnableIf (biến bool)
            bool enableByFlag = EvaluateBool(entry, entry.Attr.EnabledIf, defaultValue: true);

            using (new EditorGUI.DisabledScope(!(enableByMode && enableByFlag)))
            {
                var label = string.IsNullOrEmpty(entry.Attr.Label) ? Nicify(entry.Method.Name) : entry.Attr.Label;

                var guiContent = new GUIContent(label);
                bool clicked;
                if (height.HasValue)
                {
                    var rect = GUILayoutUtility.GetRect(GUI.skin.button.CalcSize(guiContent).x, height.Value);
                    clicked = GUI.Button(rect, guiContent);
                }
                else
                {
                    clicked = GUILayout.Button(guiContent);
                }

                if (clicked)
                {
                    Invoke(entry);
                }
            }
        }

        static string Nicify(string name)
        {
            return ObjectNames.NicifyVariableName(name.Replace("_", " "));
        }

        bool EvaluateBool(ButtonCache.ButtonEntry entry, string memberName, bool defaultValue)
        {
            if (string.IsNullOrEmpty(memberName)) return defaultValue;

            // Hỗ trợ static hoặc instance; tìm field/property bool
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var t = entry.Method.DeclaringType;

            var f = t.GetField(memberName, flags);
            if (f != null && f.FieldType == typeof(bool))
            {
                var owner = f.IsStatic ? null : (object)target;
                return (bool)f.GetValue(owner);
            }

            var p = t.GetProperty(memberName, flags);
            if (p != null && p.PropertyType == typeof(bool))
            {
                var owner = p.GetGetMethod(true)?.IsStatic == true ? null : (object)target;
                return (bool)p.GetValue(owner, null);
            }

            return defaultValue;
        }

        void Invoke(ButtonCache.ButtonEntry entry)
        {
            var method = entry.Method;

            try
            {
                // Multi-object: gọi cho mọi target
                foreach (var t in targets)
                {
                    // Undo/dirty
                    if (!entry.IsStatic)
                        Undo.RegisterCompleteObjectUndo(t, $"Invoke {method.Name}");

                    object owner = entry.IsStatic ? null : t;
                    object result = method.Invoke(owner, null);

                    // Nếu trả về giá trị, log ra console
                    if (method.ReturnType != typeof(void))
                        Debug.Log($"[{method.DeclaringType.Name}.{method.Name}] -> {result}", (UnityEngine.Object)t);

                    // Nếu đang Play và method trả về IEnumerator, cố gắng StartCoroutine (tiện)
                    if (Application.isPlaying && result is System.Collections.IEnumerator e && t is MonoBehaviour mb)
                        mb.StartCoroutine(e);

                    if (entry.Attr.MarkDirty && t) {
                        EditorUtility.SetDirty(t);
                        // Prefab stage
                        PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                Debug.LogException(ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    // Áp dụng cho mọi MonoBehaviour
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    class ButtonInspectorForMB : BaseButtonInspector<MonoBehaviour> {}

    // Áp dụng cho mọi ScriptableObject
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    class ButtonInspectorForSO : BaseButtonInspector<ScriptableObject> {}
}
#endif

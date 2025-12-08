#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Shared.Foundation
{
    public static class EasyButtonsCompat
    {
        // Vẽ tất cả method có [EasyButtons.Button] cho mảng targets (multi-object)
        public static void DrawButtons(UnityEngine.Object[] targets)
        {
            if (targets == null || targets.Length == 0) return;
            var t = targets[0].GetType();

            // Tìm attribute Button của EasyButtons qua tên type để không hard-ref assembly
            // Thường là "EasyButtons.ButtonAttribute"
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = t.GetMethods(flags)
                .Select(m => new { m, attr = GetEasyButtonsButtonAttr(m) })
                .Where(x => x.attr != null && x.m.GetParameters().Length == 0)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            foreach (var x in methods)
            {
                if (!IsVisibleInCurrentMode(x.attr)) continue;

                // Lấy label (fallback: Nicify(methodName))
                var label = GetAttrString(x.attr, "Name")
                            ?? GetAttrString(x.attr, "Text")
                            ?? GetAttrString(x.attr, "Label")
                            ?? ObjectNames.NicifyVariableName(x.m.Name);

                // Spacing (nếu EasyButtons có)
                var spacing = GetAttrFloat(x.attr, "Spacing", defaultValue: 0f);
                if (spacing > 0f) EditorGUILayout.Space(spacing);

                using (new EditorGUI.DisabledScope(!IsEnabledInCurrentMode(x.attr)))
                {
                    if (GUILayout.Button(label))
                    {
                        foreach (var obj in targets)
                        {
                            try
                            {
                                var uo = obj as UnityEngine.Object;
                                if (uo) Undo.RegisterCompleteObjectUndo(uo, $"Invoke {x.m.Name}");
                                x.m.Invoke(obj, null);
                                if (uo) EditorUtility.SetDirty(uo);
                            }
                            catch (Exception e) { Debug.LogException(e); }
                        }
                    }
                }
            }
        }

        // ---- Attribute helpers (reflection-safe) ----
        static Attribute GetEasyButtonsButtonAttr(MethodInfo m)
        {
            return m.GetCustomAttributes(inherit: true)
                    .OfType<Attribute>()
                    .FirstOrDefault(a =>
                    {
                        var tn = a.GetType().FullName ?? a.GetType().Name;
                    // nhận diện bằng tên để không hard-depend assembly
                    return tn.EndsWith(".ButtonAttribute", StringComparison.Ordinal)
                               && (tn.Contains("Shared.Foundation"));
                    });
        }

        static bool IsVisibleInCurrentMode(Attribute attr)
        {
            // EasyButtons có thể có enum "Mode { Always, Editor, Play }"
            var modeProp = attr?.GetType().GetProperty("Mode")
                      ?? attr?.GetType().GetProperty("ModeType")
                      ?? attr?.GetType().GetProperty("VisibleIn");
            if (modeProp == null) return true;

            var mode = modeProp.GetValue(attr, null);
            if (mode == null) return true;

            var name = mode.ToString();
            if (string.Equals(name, "Always", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(name, "Editor", StringComparison.OrdinalIgnoreCase)
             || string.Equals(name, "EditorOnly", StringComparison.OrdinalIgnoreCase)) return !Application.isPlaying;
            if (string.Equals(name, "Play", StringComparison.OrdinalIgnoreCase)
             || string.Equals(name, "PlayModeOnly", StringComparison.OrdinalIgnoreCase)) return Application.isPlaying;

            return true;
        }

        static bool IsEnabledInCurrentMode(Attribute attr)
        {
            // Bạn có thể mở rộng nếu attribute có flag DisableInPlayMode/Editor…
            return true;
        }

        static string GetAttrString(Attribute attr, string propName)
        {
            var p = attr.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return p != null ? p.GetValue(attr, null) as string : null;
        }

        static float GetAttrFloat(Attribute attr, string propName, float defaultValue)
        {
            var p = attr.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (p == null) return defaultValue;
            var v = p.GetValue(attr, null);
            if (v is float f) return f;
            if (v is double d) return (float)d;
            if (v is int i) return i;
            return defaultValue;
        }
    }
}
#endif

using System;

namespace Shared.Foundation
{
    public enum ButtonEnableMode
    {
        Always,               // luôn bấm được
        EditorOnly,           // chỉ khi !Application.isPlaying
        PlaymodeOnly          // chỉ khi Application.isPlaying
    }

    public enum ButtonStyle { Normal, Mini, Large }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>Nhãn hiển thị (mặc định = tên method)</summary>
        public string Label { get; }
        /// <summary>Bật/tắt theo trạng thái playmode</summary>
        public ButtonEnableMode EnableMode { get; set; } = ButtonEnableMode.Always;
        /// <summary>Thứ tự vẽ (nhỏ vẽ trước)</summary>
        public int Order { get; set; } = 0;
        /// <summary>Đặt true để MarkDirty đối tượng sau khi bấm</summary>
        public bool MarkDirty { get; set; } = true;
        /// <summary>Tên field/property bool (instance/static) để quyết định có hiển thị</summary>
        public string VisibleIf { get; set; }
        /// <summary>Tên field/property bool để quyết định có enabled</summary>
        public string EnabledIf { get; set; }
        /// <summary>Nhóm nút (cùng group sẽ nằm trên cùng 1 dòng khi style=Mini)</summary>
        public string Group { get; set; }
        /// <summary>Kiểu hiển thị nút</summary>
        public ButtonStyle Style { get; set; } = ButtonStyle.Normal;

        public ButtonAttribute(string label = null) { Label = label; }
    }

}

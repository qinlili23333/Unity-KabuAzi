using UnityEditor;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// GUI styles to be used in editor GUIs.
    /// </summary>
    internal static class EditorGUIStyles {
        private static bool _isCached;
        private static bool? _prevIsProSkin;

        public static GUIStyle RichTextLabel;
        public static GUIStyle RichTextLabelWordWrap;
        public static GUIStyle LinkLabel;
        public static GUIStyle LargeButton;
        public static GUIStyle QuickGuideTitle;
        public static Texture2D InfoIconSmall;
        public static Texture2D WarningIconSmall;
        public static Texture2D ErrorIconSmall;
        public static Color EditorWindowBackgroundColor;

        public static void Cache() {
            if (!_prevIsProSkin.HasValue || EditorGUIUtility.isProSkin != _prevIsProSkin) {
                _isCached = false;
                _prevIsProSkin = EditorGUIUtility.isProSkin;
            }

            if (_isCached)
                return;

            _isCached = true;
            RichTextLabel = new GUIStyle(GUI.skin.label);
            RichTextLabel.richText = true;

            RichTextLabelWordWrap = new GUIStyle(RichTextLabel);
            RichTextLabelWordWrap.wordWrap = true;

            InfoIconSmall = EditorGUIUtilityExposed.LoadIcon("console.infoicon.sml");
            WarningIconSmall = EditorGUIUtilityExposed.LoadIcon("console.warnicon.sml");
            ErrorIconSmall = EditorGUIUtilityExposed.LoadIcon("console.erroricon.sml");

            EditorWindowBackgroundColor = EditorGUIUtility.isProSkin ? new Color32(49, 49, 49, 255) : new Color32(194, 194, 194, 255);

            LinkLabel = new GUIStyle(GUI.skin.label);
            LinkLabel.normal.textColor = new Color32(63, 128, 229, 255);
            LinkLabel.wordWrap = false;

            LargeButton = "LargeButton";

            QuickGuideTitle = new GUIStyle(GUI.skin.label);
            QuickGuideTitle.fontSize = 17;
        }
    }
}

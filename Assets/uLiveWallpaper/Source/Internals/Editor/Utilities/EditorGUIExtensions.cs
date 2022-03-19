using UnityEngine;
using UnityEditor;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Extensions of EditorGUI.
    /// </summary>
    internal static class EditorGUIExtensions {
        /// <summary>
        /// Setups the rect to act as a hyperlink.
        /// </summary>
        /// <param name="rect">
        /// Link <see cref="Rect"/>.
        /// </param>
        /// <param name="url">
        /// The URL.
        /// </param>
        public static void SetLinkRect(Rect rect, string url) {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) {
                Application.OpenURL(url);
            }
        }
    }
}

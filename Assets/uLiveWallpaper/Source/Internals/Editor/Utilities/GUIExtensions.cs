using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    internal static class GUIExtensions {
        public static void ResetFocus() {
            GUI.SetNextControlName("");
            GUI.FocusControl("");
        }
    }
}

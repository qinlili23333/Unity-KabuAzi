using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Exposes non-public members of <see cref="EditorGUIUtility"/> via reflection.
    /// </summary>
    internal static class EditorGUIUtilityExposed {
        private static readonly MethodInfo _loadIconMethodInfo;

        static EditorGUIUtilityExposed() {
            _loadIconMethodInfo =
                typeof(EditorGUIUtility)
                .GetMethod(
                    "LoadIcon",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(string) },
                    null
                );
        }

        public static Texture2D LoadIcon(string path) {
            return (Texture2D) _loadIconMethodInfo.Invoke(null, new object[] { path });
        }
    }
}

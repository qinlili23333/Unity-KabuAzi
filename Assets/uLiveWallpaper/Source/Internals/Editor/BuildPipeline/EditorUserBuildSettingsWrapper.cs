using System.Reflection;
using UnityEditor;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Wrapper for Unity 5.5+ <c>EditorUserBuildSettings.androidBuildSystem</c>.
    /// </summary>
    public static class EditorUserBuildSettingsWrapper {
        private static readonly PropertyInfo _androidBuildSystemPropertyInfo;

        public static AndroidBuildSystem androidBuildSystem {
            get {
                if (_androidBuildSystemPropertyInfo == null)
                    return AndroidBuildSystem.ADT;

                return (AndroidBuildSystem) _androidBuildSystemPropertyInfo.GetValue(null, null);
            }
            set {
                if (_androidBuildSystemPropertyInfo == null)
                    return;

                _androidBuildSystemPropertyInfo.SetValue(null, (int) value, null);
            }
        }

        static EditorUserBuildSettingsWrapper() {
            _androidBuildSystemPropertyInfo = typeof(EditorUserBuildSettings).GetProperty("androidBuildSystem");
        }

        /// <summary>Type of Android build system.</summary>
        public enum AndroidBuildSystem {
            Internal,
            Gradle,
            ADT,
        }
    }
}
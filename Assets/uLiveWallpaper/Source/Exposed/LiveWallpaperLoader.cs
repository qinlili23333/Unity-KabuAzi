#if UNITY_ANDROID

using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Internal {
    internal static class LiveWallpaperLoader {
#if UNITY_5_0 || UNITY_5_1
        [RuntimeInitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void OnLoad() {
            Load();
        }

        private static void Load() {
            // Call the static constructor
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(LiveWallpaper).TypeHandle);
        }
    }
}

#endif
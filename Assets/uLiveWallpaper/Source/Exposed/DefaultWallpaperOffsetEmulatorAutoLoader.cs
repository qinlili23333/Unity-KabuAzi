#if UNITY_ANDROID

using UnityEngine;
using Object = UnityEngine.Object;

namespace LostPolygon.uLiveWallpaper.Internal {
    internal static class DefaultWallpaperOffsetEmulatorAutoLoader {
        private const string kResourceName = "DefaultWallpaperOffsetEmulator";

        [RuntimeInitializeOnLoadMethod]
        private static void OnLoad() {
            Load();
        }

        private static void Load() {
            if (LiveWallpaper.Emulation.GetWallpaperOffsetEmulator() != null)
                return;

            GameObject gameObjectEmulatorPrefab = Resources.Load<GameObject>(kResourceName);
            if (gameObjectEmulatorPrefab == null) {
                Debug.LogErrorFormat("Default wallpaper offset emulator prefab '{0}' not found. Please re-import uLiveWallpaper.", kResourceName);
                return;
            }

            GameObject emulator = (GameObject) Object.Instantiate(gameObjectEmulatorPrefab, Vector3.zero, Quaternion.identity);
            emulator.name = "[" + gameObjectEmulatorPrefab.name + "]";
        }
    }
}

#endif
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    internal static class UnityVersionUtility {
        public static UnityVersionParser UnityVersion { get; private set; }

        public static bool IsGradleBuildSystemSupported {
            get {
                // Unity 5.5+
                return
                    UnityVersion.VersionMajor > 5 ||
                    UnityVersion.VersionMajor == 5 &&
                    UnityVersion.VersionMinor >= 5;
            }
        }

        static UnityVersionUtility() {
            UnityVersion = new UnityVersionParser(Application.unityVersion);
        }
    }
}

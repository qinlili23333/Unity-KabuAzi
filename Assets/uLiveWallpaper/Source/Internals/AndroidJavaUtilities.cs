#if UNITY_ANDROID

using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Internal {
    /// <summary>
    /// Utility methods for working with Android Java objects.
    /// </summary>
    internal static class AndroidJavaUtilities {
        /// <summary>
        /// Returns whether <paramref name="androidJavaObject"/> is actually null,
        /// whether on C# or on Java level.
        /// </summary>
        /// <param name="androidJavaObject">The <see cref="AndroidJavaObject"/> to be checked against null.</param>
        /// <returns>true if <paramref name="androidJavaObject"/> is null, false otherwise.</returns>
        public static bool IsNull(this AndroidJavaObject androidJavaObject) {
            return
                androidJavaObject == null ||
                androidJavaObject.GetRawObject() == IntPtr.Zero ||
                androidJavaObject.GetRawClass() == IntPtr.Zero;
        }

        /// <summary>
        /// Returns whether <paramref name="androidJavaClass"/> is actually null,
        /// whether on C# or on Java level.
        /// </summary>
        /// <param name="androidJavaClass">The <see cref="AndroidJavaClass"/> to be checked against null.</param>
        /// <returns>true if <paramref name="androidJavaClass"/> is null, false otherwise.</returns>
        public static bool IsNull(this AndroidJavaClass androidJavaClass) {
            return
                androidJavaClass == null ||
                androidJavaClass.GetRawClass() == IntPtr.Zero;
        }
    }
}

#endif
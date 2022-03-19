using System;
using System.IO;
using UnityEditor;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    internal static class PathUtilities {
        /// <summary>
        /// Combines file system paths correctly.
        /// </summary>
        /// <param name="paths">
        /// The paths to combine.
        /// </param>
        /// <returns>
        /// The resulting combined path.
        /// </returns>
        public static string Combine(params string[] paths) {
            if (paths == null)
                throw new ArgumentNullException("paths");

            if (paths.Length == 2)
                return Path.Combine(paths[0], paths[1]);

            string result = paths[0];
            for (int i = 1; i < paths.Length; i++) {
                result = Path.Combine(result, paths[i]);
            }

            return result;
        }

        /// <summary>
        /// Replaces slashes in a path with correct for current OS.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The corrected path.
        /// </returns>
        public static string FixPathSlashes(this string path) {
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Returns a name of temporary file in project's Temp directory.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetTempFilePath() {
            return Path.GetFullPath(FileUtil.GetUniqueTempPathInProject().FixPathSlashes());
        }
    }
}
using System;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Shared constants.
    /// </summary>
    internal static class Constants {
        private const string kVersion = "1.4.0";
        private const string kVersionPatch = "2";
        public const string kVersionFull = kVersion + "." + kVersionPatch;
        public static readonly DateTime kReleaseDate = new DateTime(2016, 12, 10);

        public const string kULiveWallpaperLibraryName = "LP_uLiveWallpaper";
    }
}

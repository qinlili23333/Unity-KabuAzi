#if UNITY_ANDROID

using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
        /// <summary>
        /// Current wallpaper offset state.
        /// </summary>
        public static WallpaperOffsetData WallpaperOffset { get; private set; }

        /// <summary>
        /// Current wallpaper virtual desired size, in pixels.
        /// </summary>
        public static WallpaperDesiredSizeData WallpaperDesiredSize { get; private set; }

        /// <summary>
        /// Whether wallpaper is currently visible.
        /// </summary>
        /// <returns>true if wallpaper is  visible, false if it isn't, or on error.</returns>
        public static bool IsVisible { get; private set; }

        /// <summary>
        /// Whether wallpaper is currently in preview mode.
        /// </summary>
        /// <returns>true if wallpaper is in preview mode, false if it isn't, or on error.</returns>
        public static bool IsPreview { get; private set; }

        /// <summary>
        /// Represents information about wallpaper offset.
        /// </summary>
        public struct WallpaperOffsetData {
            /// <summary>
            /// Normalized offset. Values range from 0 to 1, with 0.5 being at center.
            /// </summary>
            public Vector2 Offset { get; private set; }

            /// <summary>
            /// Normalized offset step. Values range from 0 to 1.
            /// </summary>
            /// <example>
            /// OffsetStep.x being equals 0.25 indicates that launcher has
            /// 5 screens (i.e. OffsetStep.x = 0, 0.25, 0.5, 0.75, 1.0).
            /// </example>
            public Vector2 OffsetStep { get; private set; }

            /// <summary>
            /// Pixel offset. Values are in pixels.
            /// </summary>
            /// <remarks>
            /// Gives you an indication of how much the launcher "wants" you to shift your
            /// imagery based on the screen you're on.
            /// </remarks>
            public Point PixelOffset { get; private set; }

            /// <summary>
            /// Number of screens in the launcher, from 1 to N.
            /// </summary>
            public int HomeScreenCount {
                get { return Mathf.RoundToInt(1f / OffsetStep.x) + 1; }
            }

            /// <summary>
            /// Current launcher home screen, from 0 to HomeScreenCount - 1.
            /// </summary>
            public int CurrentHomeScreen {
                get { return Mathf.RoundToInt(Offset.x * (HomeScreenCount - 1)); }
            }

            public WallpaperOffsetData(Vector2 offset, Vector2 offsetStep, Point pixelOffset)
                : this() {
                Offset = offset;
                OffsetStep = offsetStep;
                PixelOffset = pixelOffset;
            }
        }

        /// <summary>
        /// Represents information about wallpaper virtual desired size.
        /// </summary>
        public struct WallpaperDesiredSizeData {
            /// <summary>
            /// Virtual wallpaper desired width.
            /// </summary>
            public int Width { get; private set; }

            /// <summary>
            /// Virtual wallpaper desired height.
            /// </summary>
            public int Height { get; private set; }

            public WallpaperDesiredSizeData(int width, int height)
                : this() {
                Width = width;
                Height = height;
            }
        }

        /// <summary>
        /// Represents a 2D integer point.
        /// </summary>
        public struct Point {
            public int x;
            public int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        private static void SetInitialState() {
#if UNITY_EDITOR
            WallpaperOffset =
                new WallpaperOffsetData(
                    new Vector2(0.5f, 0f),
                    new Vector2(1f, 0f),
                    new Point(-Screen.width / 2, 0)
                );
            IsPreview = false;
            IsVisible = true;
#endif
        }
    }
}

#endif
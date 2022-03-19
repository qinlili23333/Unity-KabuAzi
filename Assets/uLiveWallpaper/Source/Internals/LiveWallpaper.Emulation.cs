#if UNITY_ANDROID

using LostPolygon.uLiveWallpaper.Internal;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
        private delegate void OffsetChangePostprocessor(ref WallpaperOffsetData offset);

        private static OffsetChangePostprocessor _offsetChangePostprocessor;
#endif

        /// <summary>
        /// Provides access to features allowing to override current LiveWallpaper state.
        /// </summary>
        public static class Emulation {
            /// <summary>
            /// Delegate callback, used by <seealso cref="ILiveWallpaperOffsetEmulator"/> to set the wallpaper offset.
            /// </summary>
            /// <param name="wallpaperOffset">The wallpaper offset.</param>
            public delegate void SetWallpaperOffsetCallback(WallpaperOffsetData wallpaperOffset);

            /// <summary>
            /// Currently active wallpaper offset emulator.
            /// </summary>
            private static ILiveWallpaperOffsetEmulator _emulator;

            static Emulation() {
                // Subscribe to Unity Update() message
                LiveWallpaperUnityMessagesForwarder.UpdateEntered += UnityUpdate;
            }

            /// <summary>
            /// Gets current wallpaper offset emulator.
            /// </summary>
            /// <returns>Current wallpaper offset emulator, or null if none is set.</returns>
            public static ILiveWallpaperOffsetEmulator GetWallpaperOffsetEmulator() {
                return _emulator;
            }

            /// <summary>
            /// Sets the wallpaper offset emulator.
            /// If another emulator was registered at the moment of the call,
            /// it is immediately unregistered.
            /// </summary>
            /// <param name="emulator">The emulator instance, or null to unregister any current emulator.</param>
            public static void SetWallpaperOffsetEmulator(ILiveWallpaperOffsetEmulator emulator) {
                if (_emulator == emulator)
                    return;

                if (emulator != null) {
                    UnregisterWallpaperOffsetEmulator();

                    _emulator = emulator;
                    _emulator.OnRegister(SetWallpaperOffset);
#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    _offsetChangePostprocessor = PostprocessOffsetChange;
#endif
                } else {
                    UnregisterWallpaperOffsetEmulator();
                }
            }

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
            /// <summary>
            /// Postprocesses the wallpaper offset change event that came from Java side.
            /// </summary>
            /// <param name="offset">The offset.</param>
            private static void PostprocessOffsetChange(ref WallpaperOffsetData offset) {
                if (_emulator == null)
                    return;

                _emulator.HandleOffsetChange(ref offset);
            }
#endif

            /// <summary>
            /// Unities the update.
            /// </summary>
            /// <param name="deltaTime">The time delta.</param>
            private static void UnityUpdate(float deltaTime) {
#if UNITY_EDITOR
                EmulateStateForEditor();
#endif
                if (_emulator == null)
                    return;

                _emulator.UpdateState(deltaTime);
            }

            /// <summary>
            /// Unregisters current wallpaper offset emulator, if any.
            /// </summary>
            private static void UnregisterWallpaperOffsetEmulator() {
                if (_emulator == null)
                    return;

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                _offsetChangePostprocessor = null;
#endif
                _emulator.OnUnregister();
                _emulator = null;
            }

            /// <summary>
            /// Used as callback for <seealso cref="SetWallpaperOffsetCallback"/>.
            /// </summary>
            /// <param name="wallpaperOffset">The wallpaper offset.</param>
            private static void SetWallpaperOffset(WallpaperOffsetData wallpaperOffset) {
                WallpaperOffset = wallpaperOffset;
                if (OffsetsChanged != null)
                    OffsetsChanged(WallpaperOffset.Offset, WallpaperOffset.OffsetStep, WallpaperOffset.PixelOffset);
            }

#if UNITY_EDITOR
            /// <summary>
            /// Emulates the state for Unity Editor.
            /// </summary>
            private static void EmulateStateForEditor() {
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                if (WallpaperDesiredSize.Width != screenWidth || WallpaperDesiredSize.Height != screenHeight) {
                    WallpaperDesiredSize = new WallpaperDesiredSizeData(screenWidth, screenHeight);
                    if (DesiredSizeChanged != null)
                        DesiredSizeChanged(screenWidth, screenWidth);
                }
            }
#endif
        }
    }
}

#endif
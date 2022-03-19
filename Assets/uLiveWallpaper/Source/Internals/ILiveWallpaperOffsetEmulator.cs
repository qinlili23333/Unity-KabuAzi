#if UNITY_ANDROID

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Implement this interface to implement a wallpaper offset emulator.
    /// </summary>
    public interface ILiveWallpaperOffsetEmulator {
        /// <summary>
        /// Gets a value indicating whether current launcher correctly reports wallpaper offset data.
        /// </summary>
        bool IsOffsetChangedWorking { get; }

        /// <summary>
        /// Called after this emulator was registered as an active emulator.
        /// </summary>
        /// <param name="setWallpaperOffset">The callback to apply the wallpaper offset data.</param>
        void OnRegister(LiveWallpaper.Emulation.SetWallpaperOffsetCallback setWallpaperOffset);

        /// <summary>
        /// Called before this emulator was unregistered from being an active emulator.
        /// </summary>
        void OnUnregister();

        /// <summary>
        /// Generic Update event.
        /// </summary>
        /// <param name="deltaTime">The delta time.</param>
        void UpdateState(float deltaTime);

        /// <summary>
        /// Postprocesses the wallpaper offset change event that came from Java side.
        /// </summary>
        /// <param name="offset">The offset.</param>
        void HandleOffsetChange(ref LiveWallpaper.WallpaperOffsetData offset);
    }
}

#endif

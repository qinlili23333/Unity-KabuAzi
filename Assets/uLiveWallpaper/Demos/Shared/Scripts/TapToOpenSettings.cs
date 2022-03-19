using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Starts the settings Activity after a succession of quick taps.
    /// </summary>
    public class TapToOpenSettings : MonoBehaviour {
#pragma warning disable 0618
#pragma warning disable 0414
        [Tooltip("If checked, tapping will open the wallpaper preview screen instead of starting the Settings Activity.")]
        [SerializeField]
        private bool _openPreviewScreen;

        [Tooltip("Fully qualified name of settings Activity.\nLeave empty to use the default name.")]
        [SerializeField]
        private string _settingsActivityClassName = "";

        /// <summary>
        /// Whether to disable this function in preview mode.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to disable this function in preview mode.")]
        private bool _disableInPreview = true;

        /// <summary>
        /// Number of consequent taps required to open settings.
        /// </summary>
        [SerializeField]
        [Range(2, 5)]
        [Tooltip("Number of consequent taps required to open settings.")]
        private int _numberOfTaps = 2;

        /// <summary>
        /// Maximum time between taps to count them as one sequence.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 2f)]
        [Tooltip("Maximum time between taps to count them as one sequence.")]
        private float _maxTimeBetweenTaps = 0.25f;

        /// <summary>
        /// Relative maximum distance between sequential taps.
        /// For example, value of 0.5 allows sequential taps to be half a screen away from each other.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 1f)]
        [Tooltip("Relative maximum distance between sequential taps.\n" +
                 "For example, value of 0.5 allows sequential taps to be half a screen away from each other.")]
        private float _tapZoneRadiusRelative = 0.15f;
#pragma warning restore 0414
#pragma warning restore 0618

#if UNITY_ANDROID
        private void OnEnable() {
            LiveWallpaper.MultiTapDetector.NumberOfTaps = _numberOfTaps;
            LiveWallpaper.MultiTapDetector.MaxTimeBetweenTaps = (long) (_maxTimeBetweenTaps * 1000f);
            LiveWallpaper.MultiTapDetector.TapZoneRadiusRelative = _tapZoneRadiusRelative;

            // Subscribe to the event
            LiveWallpaper.MultiTapDetected += LiveWallpaperOnMultiTapDetected;
        }

        private void OnDisable() {
            // Unsubscribe from the event. This is important, otherwise you'll get a memory leak
            LiveWallpaper.MultiTapDetected -= LiveWallpaperOnMultiTapDetected;
        }

        private void LiveWallpaperOnMultiTapDetected(Vector2 lastTapPosition) {
            if (_disableInPreview && LiveWallpaper.IsPreview)
                return;

            DoAction();
        }

        private void DoAction() {
            if (_openPreviewScreen) {
                LiveWallpaper.OpenPreviewScreen();
            } else {
                if (!string.IsNullOrEmpty(_settingsActivityClassName)) {
                    LiveWallpaper.StartActivity(_settingsActivityClassName);
                } else {
                    LiveWallpaper.StartDefaultSettingsActivity();
                }
            }
        }
#endif

    }
}

using System;
using System.Collections;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Updates quality settings and target frame rate using values from preferences.
    /// </summary>
    public class GraphicsSettingsApplier : MonoBehaviour {
#pragma warning disable 0618
#pragma warning disable 0414
        [SerializeField]
        private string _qualitySettingsIndexPreferencesKey = "general_graphics_quality";

        [SerializeField]
        private string _framesPerSecondPreferencesKey = "general_frames_per_second";

        [SerializeField]
        private bool _applyExpensiveChanges = false;

        [SerializeField]
        private bool _applyInEditor = false;

        private const string kDefaultFramesPerSecondValue = "30";
        private const string kDefaultQualityLevelValue = "0";
#pragma warning restore 0414
#pragma warning restore 0618

#if UNITY_ANDROID
        private int _savedQualitySettingsIndex;
        private int _savedFramesPerSecond;
        private bool _waitingForQualitySettingsUpdate;

        private void Awake() {
            _savedQualitySettingsIndex = QualitySettings.GetQualityLevel();
            _savedFramesPerSecond = Application.targetFrameRate;
        }

        private void OnEnable() {
            // Exit if running in Editor
#if UNITY_EDITOR
            if (!_applyInEditor)
                return;
#endif

            // Subscribe to the event
            LiveWallpaper.PreferenceChanged += LiveWallpaperOnPreferenceChanged;

            // Apply the initial values
            Apply();
        }

        private void OnDisable() {
            // Exit if running in Editor
#if UNITY_EDITOR
            if (!_applyInEditor)
                return;
#endif

            // Unsubscribe from the event. This is important, otherwise you'll get a memory leak
            LiveWallpaper.PreferenceChanged -= LiveWallpaperOnPreferenceChanged;

            // Revert to the original settings
            Revert();
        }

        private void LiveWallpaperOnPreferenceChanged(string key) {
            // Update Unity settings when preference was changed in Android
            if (key == _framesPerSecondPreferencesKey) {
                int framesPerSecond = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_framesPerSecondPreferencesKey, kDefaultFramesPerSecondValue));
                Application.targetFrameRate = framesPerSecond;

                Debug.LogFormat("GraphicsSettingsApplier: framerate set to {0}", Application.targetFrameRate);
            } else if (key == _qualitySettingsIndexPreferencesKey) {
                // Delay updating quality settings to avoid GL context loss
                if (!_waitingForQualitySettingsUpdate) {
                    _waitingForQualitySettingsUpdate = true;
                    StartCoroutine(UpdateQualitySettings());
                }
            }
        }

        private void Revert() {
            QualitySettings.SetQualityLevel(_savedQualitySettingsIndex, true);
            Application.targetFrameRate = _savedFramesPerSecond;
        }

        private void Apply() {
            int qualitySettingsIndex = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_qualitySettingsIndexPreferencesKey, kDefaultQualityLevelValue));
            int framesPerSecond = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_framesPerSecondPreferencesKey, kDefaultFramesPerSecondValue));

            QualitySettings.SetQualityLevel(qualitySettingsIndex, _applyExpensiveChanges);
            Application.targetFrameRate = framesPerSecond;

            Debug.LogFormat("GraphicsSettingsApplier: framerate set to {0}", Application.targetFrameRate);
            Debug.LogFormat("GraphicsSettingsApplier: quality level set set to {0}", QualitySettings.names[QualitySettings.GetQualityLevel()]);
        }

        private IEnumerator UpdateQualitySettings() {
            // Delay updating quality settings by one frame to avoid GL context loss
            yield return new WaitForEndOfFrame();

            _waitingForQualitySettingsUpdate = false;

            int qualitySettingsIndex = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_qualitySettingsIndexPreferencesKey, kDefaultQualityLevelValue));
            QualitySettings.SetQualityLevel(qualitySettingsIndex, _applyExpensiveChanges);
            Debug.LogFormat("GraphicsSettingsApplier: quality level set set to {0}", QualitySettings.names[QualitySettings.GetQualityLevel()]);
        }
#endif
    }
}

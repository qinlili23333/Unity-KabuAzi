using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Reads the number of objects from preferences and sets up <see cref="ObjectSpawner"/> accordingly.
    /// </summary>
    public class NumberOfObjectsSettingsApplier : MonoBehaviour {
#pragma warning disable 0414
        [SerializeField]
        private string _numberOfObjectPreferencesKey = "scene_cubes_count";
#pragma warning restore 0414

        [SerializeField]
        private ObjectSpawner _objectSpawner;

#if !UNITY_ANDROID
        private void Awake() {
            _objectSpawner.Respawn();
        }
#else
        private void Awake() {
            // Read the initial value and spawn the objects
            _objectSpawner.NumberOfObjects = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_numberOfObjectPreferencesKey, "15"));
            _objectSpawner.Respawn();
        }

        private void OnEnable() {
            // Subscribe to the event
            LiveWallpaper.PreferenceChanged += LiveWallpaperOnPreferenceChanged;
        }

        private void OnDisable() {
            // Unsubscribe from the event. This is important, otherwise you'll get a memory leak
            LiveWallpaper.PreferenceChanged -= LiveWallpaperOnPreferenceChanged;
        }

        private void LiveWallpaperOnPreferenceChanged(string key) {
            if (key == _numberOfObjectPreferencesKey) {
                // Read the preference value and update the object spawner
                _objectSpawner.NumberOfObjects = Convert.ToInt32(LiveWallpaper.Preferences.GetString(_numberOfObjectPreferencesKey, "15"));
                _objectSpawner.Respawn();
            }
        }
#endif
    }
}

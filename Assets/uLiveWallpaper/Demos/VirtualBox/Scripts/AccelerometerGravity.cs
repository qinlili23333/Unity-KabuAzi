using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Changes gravity to match the device orientation.
    /// </summary>
    /// <remarks>
    /// In Editor, accelerometer data is emulated with mouse cursor position relative to screen center.
    /// </remarks>
    public class AccelerometerGravity : MonoBehaviour {
        private const string kDefaultGravityScaleStringValue = "30";
#pragma warning disable 0414
        [SerializeField]
        private float _lerpSpeed = 2f;

        [SerializeField]
        private string _gravityScalePreferenceKey = "scene_gravity_scale";

        private Vector3 _savedGravity;
        private float _gravityScale = Convert.ToSingle(kDefaultGravityScaleStringValue);
#pragma warning restore 0414

#if UNITY_ANDROID
        public float LerpSpeed {
            get { return _lerpSpeed; }
            set { _lerpSpeed = value; }
        }

        private void OnEnable() {
            UpdateGravityScale();

            // Subscribe to the event
            LiveWallpaper.PreferenceChanged += LiveWallpaperOnPreferenceChanged;

            // Save gravity vector
            _savedGravity = Physics.gravity;
        }

        private void OnDisable() {
            // Unsubscribe from the event. This is important, otherwise you'll get a memory leak
            LiveWallpaper.PreferenceChanged -= LiveWallpaperOnPreferenceChanged;

            // Restore gravity vector
            Physics.gravity = _savedGravity;
        }

        private void LiveWallpaperOnPreferenceChanged(string key) {
            if (key == _gravityScalePreferenceKey) {
                UpdateGravityScale();
            }
        }

        private void UpdateGravityScale() {
            // Read the preference value and update the object spawner
            _gravityScale =
                float.TryParse(LiveWallpaper.Preferences.GetString(_gravityScalePreferenceKey, kDefaultGravityScaleStringValue), out _gravityScale) ?
                    _gravityScale :
                    Convert.ToSingle(kDefaultGravityScaleStringValue);

            // Do not allow too small and negative values
            if (_gravityScale < 0.1f) {
                LiveWallpaper.Preferences.StartEditing();
                LiveWallpaper.Preferences.SetString(_gravityScalePreferenceKey, kDefaultGravityScaleStringValue);
                LiveWallpaper.Preferences.FinishEditing();
            }
        }

        private void Update() {
            // Calculate the gravity vector
            Vector3 acceleration;
            if (Application.platform == RuntimePlatform.Android
#if UNITY_EDITOR
                || UnityEditor.EditorApplication.isRemoteConnected
#endif
                ) {
                acceleration = Input.acceleration;
                acceleration.z *= -1f;
            } else {
                Vector3 centerPosition = Vector3.one * 0.5f;
                centerPosition.z = 2.5f;
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 2.5f;
                acceleration = Camera.main.ScreenToWorldPoint(mousePosition) - Camera.main.ViewportToWorldPoint(centerPosition);

                acceleration.z = 0f;
                acceleration.Normalize();
            }

            // Apply the scale
            acceleration *= _gravityScale;

            // Smoothly change the gravity
            Physics.gravity = Vector3.Lerp(Physics.gravity, acceleration, _lerpSpeed * Time.deltaTime);
        }
#endif
    }
}
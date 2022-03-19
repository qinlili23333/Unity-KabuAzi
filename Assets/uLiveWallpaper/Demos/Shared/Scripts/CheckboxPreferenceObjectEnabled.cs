using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Updates quality settings and target frame rate using values from preferences.
    /// </summary>
    public class CheckboxPreferenceObjectEnabled : MonoBehaviour {
#pragma warning disable 0618
#pragma warning disable 0414
        [SerializeField]
        private string _checkboxPreferenceKey;

        [SerializeField]
        private bool _defaultValue;

        [SerializeField]
        private bool _inverseValue;

        [SerializeField]
        private Behaviour[] _controlledBehaviours;

        [SerializeField]
        private GameObject[] _controlledGameObjects;
#pragma warning restore 0414
#pragma warning restore 0618

#if UNITY_ANDROID
        private void OnEnable() {
            // Subscribe to the event
            LiveWallpaper.PreferenceChanged += LiveWallpaperOnPreferenceChanged;
            Apply();
        }

        private void OnDisable() {
            // Unsubscribe from the event. This is important, otherwise you'll get a memory leak
            LiveWallpaper.PreferenceChanged -= LiveWallpaperOnPreferenceChanged;
        }

        private void LiveWallpaperOnPreferenceChanged(string key) {
            if (key != _checkboxPreferenceKey)
                return;

            Apply();
        }

        private void Apply() {
            bool value = LiveWallpaper.Preferences.GetBool(_checkboxPreferenceKey, _defaultValue);
            for (int i = 0; i < _controlledBehaviours.Length; i++) {
                Behaviour controlledBehaviour = _controlledBehaviours[i];
                if (controlledBehaviour == null)
                    continue;

                controlledBehaviour.enabled = _inverseValue ? !value : value;
            }

            for (int i = 0; i < _controlledGameObjects.Length; i++) {
                GameObject controlledGameObject = _controlledGameObjects[i];
                if (controlledGameObject == null)
                    continue;

                controlledGameObject.SetActive(_inverseValue ? !value : value);
            }
        }
#endif
    }
}

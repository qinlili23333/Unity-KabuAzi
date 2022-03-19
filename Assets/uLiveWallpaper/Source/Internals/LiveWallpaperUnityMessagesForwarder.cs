using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Internal {
    /// <summary>
    /// Provides static access to global Unity messages.
    /// </summary>
    internal sealed class LiveWallpaperUnityMessagesForwarder : SingletonMonoBehaviour<LiveWallpaperUnityMessagesForwarder> {
#if UNITY_ANDROID
        public static event Action<float> UpdateEntered;
        public static event Action GenericMessageEntered;
        public static event Action<bool> ApplicationPaused;

        [RuntimeInitializeOnLoadMethod]
        public static void UpdateInstanceOnLoad() {
            TryUpdateInstance();
        }

        protected override void Awake() {
            base.Awake();

            // Make it hidden and indestructible
            gameObject.hideFlags =
                HideFlags.NotEditable |
                HideFlags.HideInHierarchy |
                HideFlags.DontSaveInBuild |
                HideFlags.DontSaveInEditor;
        }

        private void OnApplicationFocus(bool focusStatus) {
            if (!focusStatus)
                return;

            if (GenericMessageEntered != null)
                GenericMessageEntered();
        }

        private void OnApplicationPause(bool pauseStatus) {
            if (ApplicationPaused != null)
                ApplicationPaused(pauseStatus);

            if (GenericMessageEntered != null)
                GenericMessageEntered();
        }

        private void Update() {
            if (UpdateEntered != null)
                UpdateEntered(Time.deltaTime);

            if (GenericMessageEntered != null)
                GenericMessageEntered();
        }

        private void FixedUpdate() {
            if (GenericMessageEntered != null)
                GenericMessageEntered();
        }
#endif
    }
}
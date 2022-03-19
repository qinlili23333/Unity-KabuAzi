using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Internal {
    public abstract class WallpaperOffsetEmulatorBase<T> : SingletonMonoBehaviour<T>
#if UNITY_ANDROID
        , ILiveWallpaperOffsetEmulator
#endif
        where T : MonoBehaviour {
        [SerializeField]
        protected bool _registerOnStart = true;

        [SerializeField]
        protected bool _emulationEnabled = true;

#if UNITY_ANDROID
        private bool _isOffsetChangedWorking = false;

        public bool RegisterOnStart {
            get { return _registerOnStart; }
            set { _registerOnStart = value; }
        }

        public bool EmulationEnabled {
            get { return _emulationEnabled; }
            set { _emulationEnabled = value; }
        }

        protected LiveWallpaper.Emulation.SetWallpaperOffsetCallback SetWallpaperOffsetCallback { get; private set; }

        private void OnEnable() {
            if (_isDestroyed || !_registerOnStart)
                return;

            if (LiveWallpaper.Emulation.GetWallpaperOffsetEmulator() != null)
                return;

            LiveWallpaper.Emulation.SetWallpaperOffsetEmulator(this);
        }

        private void OnDisable() {
            if (!ReferenceEquals(LiveWallpaper.Emulation.GetWallpaperOffsetEmulator(), this))
                return;

            LiveWallpaper.Emulation.SetWallpaperOffsetEmulator(null);
        }

        #region ILiveWallpaperOffsetEmulator

        public bool IsOffsetChangedWorking {
            get { return _isOffsetChangedWorking; }
            protected set { _isOffsetChangedWorking = value; }
        }

        public virtual void OnRegister(LiveWallpaper.Emulation.SetWallpaperOffsetCallback setWallpaperOffset) {
            SetWallpaperOffsetCallback = setWallpaperOffset;
        }

        public virtual void OnUnregister() {
            SetWallpaperOffsetCallback = null;
        }

        public abstract void UpdateState(float deltaTime);

        public virtual void HandleOffsetChange(ref LiveWallpaper.WallpaperOffsetData offset) {
            if (!_isOffsetChangedWorking &&
                !Mathf.Approximately(offset.Offset.x, 0.0f) &&
                !Mathf.Approximately(offset.Offset.x, 0.5f)
                ) {
                _isOffsetChangedWorking = true;
            }
        }

        #endregion
#endif
    }
}
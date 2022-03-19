using System;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Internal;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Default wallpaper scrolling data emulator.
    /// </summary>
    public class DefaultWallpaperOffsetEmulator : WallpaperOffsetEmulatorBase<DefaultWallpaperOffsetEmulator> {
#pragma warning disable 0414
        [SerializeField]
        [Range(1, 15)]
        private int _homeScreensCount = 3;

        [SerializeField]
        private float _flingMinVelocity = 100f;

        [SerializeField]
        [Range(0.1f, 100f)]
        private float _snapLerpFactor = 20f;
#pragma warning restore 0414

#if UNITY_ANDROID
        private float _offsetStepX;
        private float _currentOffsetX;
        private float _currentDestinationOffsetX;
        private bool _isSnapping;
        private bool _isOffsetsSetAfterRegister;
        private Vector2 _prevTouchPosition;
        private Vector2 _lastNonZeroDelta;
        private float _lastNonZeroDeltaDeltaTime;

        /// <summary>
        /// Gets or sets the minimum fling velocity to snap to the screen in the direction of fling.
        /// </summary>
        public float FlingMinVelocity {
            get { return _flingMinVelocity; }
            set {
                _flingMinVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the virtual home screens count.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
        public int HomeScreensCount {
            get { return _homeScreensCount; }
            set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");

                if (_homeScreensCount == value)
                    return;

                _homeScreensCount = value;
                SetInitialValues();
            }
        }

        /// <summary>
        /// Gets or sets the value indicating how fast the position 
        /// will snap to nearest home screen position.
        /// </summary>
        public float SnapLerpFactor {
            get { return _snapLerpFactor; }
            set {
                if (value < Vector3.kEpsilon)
                    throw new ArgumentOutOfRangeException("value");

                _snapLerpFactor = value;
            }
        }

        /// <summary>
        /// Gets the width of the single screen, in pixels.
        /// </summary>
        private int SingleScreenWidth {
            get { return _homeScreensCount > 1 ? Screen.width * (_homeScreensCount - 1) : Screen.width; }
        }

        private int WallpaperRealWidth {
            get { return LiveWallpaper.WallpaperDesiredSize.Width / 2; }
        }

        private void SetInitialValues() {
            _offsetStepX = 1f / (_homeScreensCount - 1);
            _currentOffsetX = 
                _homeScreensCount > 1 ?
                _offsetStepX * Mathf.Round((_homeScreensCount - 1) * 0.5f) :
                0.5f;
            _currentDestinationOffsetX = _currentOffsetX;
        }

        #region ILiveWallpaperOffsetEmulator

        public override void OnRegister(LiveWallpaper.Emulation.SetWallpaperOffsetCallback setWallpaperOffset) {
            base.OnRegister(setWallpaperOffset);

            LiveWallpaper.VisibilityChanged += LiveWallpaperOnVisibilityChanged;
            SetInitialValues();
            _isOffsetsSetAfterRegister = false;
        }

        public override void OnUnregister() {
            base.OnUnregister();

            _currentOffsetX = 0.5f;
            UpdateWallpaperOffset();
            LiveWallpaper.VisibilityChanged -= LiveWallpaperOnVisibilityChanged;
        }

        public override void UpdateState(float deltaTime) {
            if (! EmulationEnabled || (!LiveWallpaper.IsPreview && IsOffsetChangedWorking)) {
                _currentOffsetX = LiveWallpaper.WallpaperOffset.Offset.x;
                return;
            }

            _offsetStepX = 1f / (_homeScreensCount - 1);
#if UNITY_EDITOR
            bool isEditorMouseEvent = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);
#endif

            float newOffsetX = _currentOffsetX;
            if (_homeScreensCount > 1 && (Input.touchCount > 0
#if UNITY_EDITOR
                || isEditorMouseEvent
#endif
                )
                ) {
                TouchPhase touchPhase;
                Vector2 touchPosition;
                float touchDeltaTime;
#if UNITY_EDITOR
                if (!isEditorMouseEvent) {
                    Touch mainTouch = Input.GetTouch(0);
                    touchPhase = mainTouch.phase;
                    touchPosition = mainTouch.position;
                    touchDeltaTime = mainTouch.deltaTime;
                } else {
#endif
                    touchPhase = TouchPhase.Moved;
                    if (Input.GetMouseButtonDown(0)) {
                        touchPhase = TouchPhase.Began;
                    } else if (Input.GetMouseButtonUp(0)) {
                        touchPhase = TouchPhase.Ended;
                    }

                    touchPosition = Input.mousePosition;
                    touchDeltaTime = deltaTime;
#if UNITY_EDITOR
                }
#endif

                Vector2 deltaPosition;
                switch (touchPhase) {
                    case TouchPhase.Began:
                        _isSnapping = false;
                        _prevTouchPosition = touchPosition;
                        _lastNonZeroDelta = Vector2.zero;
                        _lastNonZeroDeltaDeltaTime = touchDeltaTime;
                        break;
                    case TouchPhase.Moved:
                        deltaPosition = _prevTouchPosition - touchPosition;
                        if (Mathf.Abs(deltaPosition.x) > Vector3.kEpsilon) {
                            _lastNonZeroDelta = deltaPosition;
                            _lastNonZeroDeltaDeltaTime = touchDeltaTime;
                        }

                        newOffsetX += deltaPosition.x / SingleScreenWidth;
                        _prevTouchPosition = touchPosition;
                        break;
                    case TouchPhase.Ended:
                        deltaPosition = _prevTouchPosition - touchPosition;
                        if (Mathf.Abs(deltaPosition.x) > Vector3.kEpsilon) {
                            _lastNonZeroDelta = deltaPosition;
                            _lastNonZeroDeltaDeltaTime = touchDeltaTime;
                        }

                        float velocity = _lastNonZeroDelta.x / _lastNonZeroDeltaDeltaTime;
                        if (Mathf.Abs(velocity) > _flingMinVelocity) {
                            float endValue = velocity < 0
                                                 ? newOffsetX - newOffsetX % _offsetStepX
                                                 : newOffsetX - newOffsetX % _offsetStepX + _offsetStepX;
                            _currentDestinationOffsetX = endValue;
                        } else {
                            _currentDestinationOffsetX = Mathf.RoundToInt(newOffsetX / _offsetStepX) * _offsetStepX;
                        }

                        _currentDestinationOffsetX = Mathf.Clamp01(_currentDestinationOffsetX);
                        _isSnapping = true;
                        break;
                }
            }

            if (_isSnapping) {
                newOffsetX = Mathf.Lerp(newOffsetX, _currentDestinationOffsetX, _snapLerpFactor * deltaTime);
                if (Mathf.Abs(newOffsetX - _currentDestinationOffsetX) < Vector3.kEpsilon) {
                    _isSnapping = false;
                    newOffsetX = _currentDestinationOffsetX;
                }
            }

            newOffsetX = Mathf.Clamp01(newOffsetX);
            if (!_isOffsetsSetAfterRegister || newOffsetX != _currentOffsetX) {
                _currentOffsetX = newOffsetX;
                _isOffsetsSetAfterRegister = true;
                UpdateWallpaperOffset();
            }
        }

        public override void HandleOffsetChange(ref LiveWallpaper.WallpaperOffsetData offset) {
            base.HandleOffsetChange(ref offset);
            if (IsOffsetChangedWorking)
                return;

            offset = GetCurrentWallpaperOffset();
        }

        private void LiveWallpaperOnVisibilityChanged(bool isVisible) {
            if (isVisible) {
                IsOffsetChangedWorking = false;
            }
        }

        private LiveWallpaper.WallpaperOffsetData GetCurrentWallpaperOffset() {
            LiveWallpaper.WallpaperOffsetData wallpaperOffsetData =
                new LiveWallpaper.WallpaperOffsetData(
                    new Vector2(_currentOffsetX, 0f),
                    new Vector2(_offsetStepX, 0f),
                    new LiveWallpaper.Point(-Mathf.RoundToInt(WallpaperRealWidth * _currentOffsetX), 0)
                    );

            return wallpaperOffsetData;
        }

        private void UpdateWallpaperOffset() {
            if (SetWallpaperOffsetCallback == null)
                return;

            SetWallpaperOffsetCallback(GetCurrentWallpaperOffset());
        }

        #endregion

#endif
    }
}

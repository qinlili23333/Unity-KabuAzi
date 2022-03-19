#if UNITY_ANDROID

using System;
using LostPolygon.uLiveWallpaper.Internal;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
        /// <summary>
        /// Provides access to the multi tap detector.
        /// </summary>
        public static class MultiTapDetector {
#if UNITY_EDITOR && !UNITY_EDITOR_OVERRIDE
            private static readonly EditorImplementation _editorImplementation = new EditorImplementation();
#endif
            /// <summary>
            /// Number of consequent taps required to register an event.
            /// </summary>
            public static int NumberOfTaps {
                get {
#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    return _multiTapDetectorObject.Call<int>("getNumberOfTaps");
#else
                    return _editorImplementation.NumberOfTaps;
#endif
                }
                set {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("value", value, "value must be >= 1");

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    _multiTapDetectorObject.Call("setNumberOfTaps", value);
#else
                    _editorImplementation.NumberOfTaps = value;
#endif
                }
            }

            /// <summary>
            /// Maximum time between taps to count them as one sequence.
            /// </summary>
            public static long MaxTimeBetweenTaps {
                get {
#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    return _multiTapDetectorObject.Call<long>("getMaxTimeBetweenTaps");
#else
                    return _editorImplementation.MaxTimeBetweenTaps;
#endif
                }
                set {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException("value", value, "value must be >= 1");

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    _multiTapDetectorObject.Call("setMaxTimeBetweenTaps", value);
#else
                    _editorImplementation.MaxTimeBetweenTaps = value;
#endif
                }
            }

            /// <summary>
            /// Relative maximum distance between sequential taps.
            /// For example, value of 0.5 allows sequential taps to be half a screen away from each other.
            /// </summary>
            public static float TapZoneRadiusRelative {
                get {
#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    return _multiTapDetectorObject.Call<float>("getTapZoneRadiusRelative");
#else
                    return _editorImplementation.TapZoneRadiusRelative;
#endif
                }
                set {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException("value", value, "value must be > 0");

                    if (value > 1f || value < 0.01f)
                        throw new ArgumentOutOfRangeException("value", value, "value must be in range [0.01, 1]");

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
                    _multiTapDetectorObject.Call("setTapZoneRadiusRelative", value);
#else
                    _editorImplementation.TapZoneRadiusRelative = value;
#endif
                }
            }

#if UNITY_EDITOR
            /// <summary>
            /// Used in Editor as a replacement for Java MultiTapDetector class.
            /// </summary>
            private class EditorImplementation {
                /// <summary>
                /// Number of consequent taps required to register an event.
                /// </summary>
                private int _numberOfTaps = 2;

                /// <summary>
                /// Maximum time between taps to count them as one sequence.
                /// </summary>
                private float _maxTimeBetweenTaps = 0.25f;

                /// <summary>
                /// Relative maximum distance between sequential taps.
                /// For example, value of 0.5 allows sequential taps to be half a screen away from each other.
                /// </summary>
                private float _tapZoneRadiusRelative = 0.15f;

                private float _lastTapTime;
                private Vector2 _lastTapPosition;
                private int _currentTaps;

                public long MaxTimeBetweenTaps {
                    get { return (long) (_maxTimeBetweenTaps * 1000); }
                    set { _maxTimeBetweenTaps = value / 1000f; }
                }

                public int NumberOfTaps {
                    get { return _numberOfTaps; }
                    set { _numberOfTaps = value; }
                }

                public float TapZoneRadiusRelative {
                    get { return _tapZoneRadiusRelative; }
                    set { _tapZoneRadiusRelative = value; }
                }

                public EditorImplementation() {
                    // Subscribe to Unity Update() message
                    LiveWallpaperUnityMessagesForwarder.UpdateEntered += UnityUpdate;
                }

                private void UnityUpdate(float deltaTime) {
                    bool isTapped = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
                    if (isTapped) {
                        Vector2 currentTapPosition = Input.GetMouseButtonDown(0) ? (Vector2) Input.mousePosition : Input.GetTouch(0).position;
                        float currentTime = Time.realtimeSinceStartup;

                        if (_currentTaps > 0) {
                            // Dismiss attempt if tap is too far away from previous one
                            float maxDistance = (Screen.width + Screen.height) * 0.5f * _tapZoneRadiusRelative;
                            float distanceToLastTap = Vector2.Distance(currentTapPosition, _lastTapPosition);
                            if (distanceToLastTap > maxDistance) {
                                _currentTaps = 0;
                            }

                            // Dismiss attempt if taps are too slow
                            if (currentTime - _lastTapTime > _maxTimeBetweenTaps) {
                                _currentTaps = 0;
                            }
                        }

                        _lastTapPosition = currentTapPosition;
                        _lastTapTime = currentTime;
                        _currentTaps++;

                        if (_currentTaps >= _numberOfTaps) {
                            _currentTaps = 0;

                            if (MultiTapDetected != null)
                                MultiTapDetected(_lastTapPosition);
                        }
                    }
                }
            }
#endif

        }
    }
}

#endif
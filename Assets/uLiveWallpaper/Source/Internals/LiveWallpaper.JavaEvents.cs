#if UNITY_ANDROID

using System;
using System.Collections.Generic;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Internal;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
        #region Delegates

        /// <summary>
        /// Delegate for event <see cref="VisibilityChanged"/>.
        /// </summary>
        /// <param name="isVisible">Whether the wallpaper became visible or hidden.</param>
        public delegate void VisibilityChangedHandler(bool isVisible);

        /// <summary>
        /// Delegate for event <see cref="OffsetsChanged"/>.
        /// </summary>
        /// <param name="offset">Normalized offset. Values range from 0 to 1, with 0.5 being at center.</param>
        /// <param name="offsetStep">Normalized offset step. Values range from 0 to 1.</param>
        /// <param name="pixelOffset">Pixel offset. Values are in pixels.</param>
        public delegate void OffsetsChangedHandler(Vector2 offset, Vector2 offsetStep, Point pixelOffset);

        /// <summary>
        /// Delegate for event <see cref="DesiredSizeChanged"/>.
        /// </summary>
        /// <param name="desiredWidth">Desired wallpaper width.</param>
        /// <param name="desiredHeight">Desired wallpaper height.</param>
        public delegate void DesiredSizeChangedHandler(int desiredWidth, int desiredHeight);

        /// <summary>
        /// Delegate for event <see cref="IsPreviewChanged"/>.
        /// </summary>
        /// <param name="isPreview">Whether the wallpaper has entered or exited the preview mode.</param>
        public delegate void IsPreviewChangedHandler(bool isPreview);

        /// <summary>
        /// Delegate for event <see cref="PreferenceChanged"/>.
        /// </summary>
        /// <param name="key">Key of the changed preference.</param>
        public delegate void PreferenceChangedHandler(string key);

        /// <summary>
        /// Delegate for event <see cref="LiveWallpaper.PreferenceActivityTriggered"/>.
        /// </summary>
        public delegate void PreferenceActivityTriggeredHandler();

        /// <summary>
        /// Delegate for event <see cref="MultiTapDetected"/>.
        /// </summary>
        public delegate void MultiTapDetectedHandler(Vector2 lastTapPosition);

        /// <summary>
        /// Delegate for event <see cref="CustomEventReceived"/>.
        /// </summary>
        public delegate void CustomEventReceivedHandler(string eventName, string eventData);

        #endregion

        #region Events

#pragma warning disable 67
        /// <summary>
        /// Called when wallpaper became visible or hidden.
        /// </summary>
        public static event VisibilityChangedHandler VisibilityChanged;

        /// <summary>
        /// Called when wallpaper offsets had changed.
        /// </summary>
        public static event OffsetsChangedHandler OffsetsChanged;

        /// <summary>
        /// Called when wallpaper has entered or exited the preview mode.
        /// </summary>
        public static event IsPreviewChangedHandler IsPreviewChanged;

        /// <summary>
        /// Called when wallpaper desired size changed.
        /// </summary>
        public static event DesiredSizeChangedHandler DesiredSizeChanged;

        /// <summary>
        /// Called when a preference had changed.
        /// </summary>
        public static event PreferenceChangedHandler PreferenceChanged;

        /// <summary>
        /// Called when live wallpaper preferences Activity has started.
        /// </summary>
        public static event PreferenceActivityTriggeredHandler PreferenceActivityTriggered;

        /// <summary>
        /// Called when a quick sequence of taps was detected.
        /// </summary>
        /// <see cref="MultiTapDetector"/>
        public static event MultiTapDetectedHandler MultiTapDetected;

        /// <summary>
        /// Called when a custom event has been received.
        /// Event can be sent from Java with a code
        /// <c>LiveWallpaperUnityFacade.getEventsProxy().customEvent("somethingHappened", "some event data");</c>
        /// </summary>
        public static event CustomEventReceivedHandler CustomEventReceived;
#pragma warning restore 67

        #endregion

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE

        #region Internal stuff

        private static LiveWallpaperJavaEventsListener _javaEventsListener;
        private static IntPtr _jniUnityEventsProxySendEventsMethodId;
        private static readonly jvalue[] _jniEmptyArgArray = new jvalue[0];

        /// <summary>
        /// Attaches the Java events listener and gets current event state.
        /// </summary>
        private static void AttachEvents() {
            // Get event dispatch method
            _jniUnityEventsProxySendEventsMethodId = AndroidJNIHelper.GetMethodID(_unityEventsProxyObject.GetRawClass(), "dispatchEvents", new object[0], false);

            // Start event dispatcher
            LiveWallpaperUnityMessagesForwarder.GenericMessageEntered += DispatchDeferredEvents;

            // Register the event listener
            _javaEventsListener = LiveWallpaperJavaEventsListener.Instance;
            _unityEventsProxyObject.Call("registerLiveWallpaperEventsListener", _javaEventsListener);
        }

        /// <summary>
        /// Dispatches the recorded events.
        /// </summary>
        private static void DispatchDeferredEvents() {
            // Dispatch events received on non-main Unity thread
            AndroidJNI.CallVoidMethod(_unityEventsProxyObject.GetRawObject(), _jniUnityEventsProxySendEventsMethodId, _jniEmptyArgArray);
        }

        #endregion

        #region EventDispatchMethods

        private static class EventDispatchMethods {
            public static void DispatchVisibilityChanged(bool isVisible) {
                IsVisible = isVisible;
                if (VisibilityChanged != null)
                    VisibilityChanged(isVisible);
            }

            public static void DispatchIsPreviewChanged(bool isPreview) {
                IsPreview = isPreview;
                if (IsPreviewChanged != null)
                    IsPreviewChanged(isPreview);
            }

            public static void DispatchDesiredSizeChanged(int desiredWidth, int desiredHeight) {
                WallpaperDesiredSize = new WallpaperDesiredSizeData(desiredWidth, desiredHeight);

                if (DesiredSizeChanged != null)
                    DesiredSizeChanged(desiredWidth, desiredHeight);
            }

            public static void DispatchOffsetsChanged(Vector2 offset, Vector2 offsetStep, Point pixelOffset) {
                WallpaperOffsetData newWallpaperOffset =
                    new WallpaperOffsetData(
                        offset,
                        offsetStep,
                        pixelOffset);

                if (_offsetChangePostprocessor != null) {
                    _offsetChangePostprocessor(ref newWallpaperOffset);
                }
                WallpaperOffset = newWallpaperOffset;

                if (OffsetsChanged != null)
                    OffsetsChanged(WallpaperOffset.Offset, WallpaperOffset.OffsetStep, WallpaperOffset.PixelOffset);
            }

            public static void DispatchPreferenceChanged(string key) {
                if (PreferenceChanged != null)
                    PreferenceChanged(key);
            }

            public static void DispatchPreferenceActivityTriggered() {
                if (PreferenceActivityTriggered != null)
                    PreferenceActivityTriggered();
            }

            public static void DispatchMultiTapDetected(Vector2 lastTapPosition) {
                if (MultiTapDetected != null)
                    MultiTapDetected(lastTapPosition);
            }

            public static void DispatchCustomEventReceived(string eventName, string eventData) {
                if (CustomEventReceived != null)
                    CustomEventReceived(eventName, eventData);
            }
        }

        #endregion

        #region LiveWallpaperJavaEventsListener

        /// <summary>
        /// An implementation of the Java interface that listens to the events from Java side.
        /// </summary>
        /// <seealso cref="UnityEngine.AndroidJavaProxy" />
        private sealed class LiveWallpaperJavaEventsListener : AndroidJavaProxy {
            private static LiveWallpaperJavaEventsListener _instance;
            private static readonly object[] _emptyArgs = new object[0];
            private volatile bool _lastHashCodeCallMarker;

            public static LiveWallpaperJavaEventsListener Instance {
                get { return _instance ?? (_instance = new LiveWallpaperJavaEventsListener()); }
            }

            private LiveWallpaperJavaEventsListener()
                : base(kLiveWallpaperEventsListenerInterfaceName) {
            }

            private void visibilityChanged(bool isVisible) {
                 EventDispatchMethods.DispatchVisibilityChanged(isVisible);
            }

            private void isPreviewChanged(bool isPreview) {
                 EventDispatchMethods.DispatchIsPreviewChanged(isPreview);
            }

            private void desiredSizeChanged(int desiredWidth, int desiredHeight) {
                 EventDispatchMethods.DispatchDesiredSizeChanged(desiredWidth, desiredHeight);
            }

            private void offsetsChanged(float xOffset, float yOffset, float xOffsetStep, float yOffsetStep, int xPixelOffset, int yPixelOffset) {
                 EventDispatchMethods.DispatchOffsetsChanged(
                    new Vector2(xOffset, yOffset),
                    new Vector2(xOffsetStep, yOffsetStep),
                    new Point(xPixelOffset, yPixelOffset));
            }

            private void preferenceChanged(string key) {
                 EventDispatchMethods.DispatchPreferenceChanged(key);
            }

            private void preferencesActivityTriggered() {
                 EventDispatchMethods.DispatchPreferenceActivityTriggered();
            }

            private void multiTapDetected(float xPosition, float yPosition) {
                 EventDispatchMethods.DispatchMultiTapDetected(new Vector2(xPosition, yPosition));
            }

            private void customEventReceived(string eventName, string eventData) {
                 EventDispatchMethods.DispatchCustomEventReceived(eventName, eventData);
            }

            public override AndroidJavaObject Invoke(string methodName, object[] args) {
                throw new InvalidOperationException("This should not be called.");
            }

            public override AndroidJavaObject Invoke(string methodName, AndroidJavaObject[] javaArgs) {
                switch (methodName) {
                    case "visibilityChanged":
                        bool isVisible = javaArgs[0].Call<bool>("booleanValue", _emptyArgs);
                        visibilityChanged(isVisible);
                        break;
                    case "isPreviewChanged":
                        bool isPreview = javaArgs[0].Call<bool>("booleanValue", _emptyArgs);
                        isPreviewChanged(isPreview);
                        break;
                    case "desiredSizeChanged":
                        int desiredWidth = javaArgs[0].Call<int>("intValue", _emptyArgs);
                        int desiredHeight = javaArgs[1].Call<int>("intValue", _emptyArgs);
                        desiredSizeChanged(desiredWidth, desiredHeight);
                        break;
                    case "offsetsChanged":
                        float xOffset = javaArgs[0].Call<float>("floatValue", _emptyArgs);
                        float yOffset = javaArgs[1].Call<float>("floatValue", _emptyArgs);
                        float xOffsetStep = javaArgs[2].Call<float>("floatValue", _emptyArgs);
                        float yOffsetStep = javaArgs[3].Call<float>("floatValue", _emptyArgs);
                        int xPixelOffset = javaArgs[4].Call<int>("intValue", _emptyArgs);
                        int yPixelOffset = javaArgs[5].Call<int>("intValue", _emptyArgs);
                        offsetsChanged(xOffset, yOffset, xOffsetStep, yOffsetStep, xPixelOffset, yPixelOffset);
                        break;
                    case "preferenceChanged":
                        string key = javaArgs[0].Call<string>("toString");
                        preferenceChanged(key);
                        break;
                    case "preferencesActivityTriggered":
                        preferencesActivityTriggered();
                        break;
                    case "multiTapDetected":
                        float xPosition = javaArgs[0].Call<float>("floatValue", _emptyArgs);
                        float yPosition = javaArgs[1].Call<float>("floatValue", _emptyArgs);
                        multiTapDetected(xPosition, yPosition);
                        break;
                    case "customEventReceived":
                        string eventName = javaArgs[0].Call<string>("toString");
                        string eventData = javaArgs[1].Call<string>("toString");
                        customEventReceived(eventName, eventData);
                        break;
                    case "toString":
                        return JavaToString();
                    case "hashCode":
                        return JavaHashCode();
                    case "equals":
                        return JavaEquals(javaArgs[0]);
                    default:
                        throw new ArgumentException(string.Format("Unknown {0} method '{1}'.", typeof(LiveWallpaperJavaEventsListener).Name, methodName));
                }

                return null;
            }

            private AndroidJavaObject JavaHashCode() {
                _lastHashCodeCallMarker = true;
                int result = GetHashCode();
                return new AndroidJavaObject("java.lang.Integer", result);
            }

            private AndroidJavaObject JavaEquals(AndroidJavaObject other) {
                _lastHashCodeCallMarker = false;
                other.Call<int>("hashCode");
                bool result = _lastHashCodeCallMarker;
                _lastHashCodeCallMarker = false;
                return new AndroidJavaObject("java.lang.Boolean", result);
            }

            private AndroidJavaObject JavaToString() {
                return new AndroidJavaObject("java.lang.String", "LiveWallpaperJavaEventsListener");
            }
        }

        #endregion

#endif
    }
}

#endif
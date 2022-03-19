#if UNITY_ANDROID

using System;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Internal;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Main uLiveWallpaper class. Enables access to wallpaper-related APIs.
    /// </summary>
    public static partial class LiveWallpaper {
        /// <summary>
        /// Fully qualified of main plugin facade Java class.
        /// </summary>
        private const string kFacadeClassName = "com.lostpolygon.unity.livewallpaper.LiveWallpaperUnityFacade";

        /// <summary>
        /// Fully qualified of ILiveWallpaperEventsListener Java interface.
        /// </summary>
        private const string kLiveWallpaperEventsListenerInterfaceName = "com.lostpolygon.unity.livewallpaper.ILiveWallpaperEventsListener";

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
        /// <summary>
        /// A reference to the Java LiveWallpaperFacade object.
        /// </summary>
        private static readonly AndroidJavaObject _facadeObject;

        /// <summary>
        /// A reference to the Java LiveWallpaperFacade.PreferencesEditorFacade object.
        /// </summary>
        private static readonly AndroidJavaObject _preferencesEditorFacadeObject;

        /// <summary>
        /// A reference to the Java MultiTapDetector object.
        /// </summary>
        private static readonly AndroidJavaObject _multiTapDetectorObject;

        /// <summary>
        /// A reference to the Java UnityEventsProxy object.
        /// </summary>
        private static readonly AndroidJavaObject _unityEventsProxyObject;

        /// <summary>
        /// Whether the LiveWallpaperMediator is available and was loaded successfully.
        /// </summary>
        private static readonly bool _isFacadeObjectAvailable;
#endif

        /// <summary>
        /// Initializes <see cref="LiveWallpaper"/> class.
        /// Retrieves required Java objects.
        /// </summary>
        static LiveWallpaper() {
            SetInitialState();

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
            _isFacadeObjectAvailable = false;
            _facadeObject = null;
            _preferencesEditorFacadeObject = null;

            if (Application.platform == RuntimePlatform.Android) {
                // Retrieve Java objects
                try {
                    using (AndroidJavaClass facadeClass = new AndroidJavaClass(kFacadeClassName)) {
                        if (!facadeClass.IsNull()) {
                            _facadeObject = facadeClass.CallStatic<AndroidJavaObject>("getInstance");
                            _isFacadeObjectAvailable = !_facadeObject.IsNull();
                            if (_isFacadeObjectAvailable) {
                                _preferencesEditorFacadeObject = _facadeObject.Call<AndroidJavaObject>("getPreferencesEditorFacade");
                                _multiTapDetectorObject = _facadeObject.Call<AndroidJavaObject>("getMultiTapDetector");
                                _unityEventsProxyObject = _facadeObject.CallStatic<AndroidJavaObject>("getEventsProxy");

                                // Start event listening
                                AttachEvents();
                            } else
                                throw new Exception("LiveWallpaperUnityFacade.getInstance() returned null.");
                        } else
                            throw new Exception(string.Format("Can't find class '{0}'.", kFacadeClassName));
                    }
                } catch (Exception e) {
                    Debug.LogException(e);
                    Debug.LogError("LiveWallpaperFacade initialization failed. Probably .aar not present?");
                }
            }
#endif

            LiveWallpaperUnityMessagesForwarder.UpdateInstanceOnLoad();
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(MultiTapDetector).TypeHandle);

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
            // Send the OnApplicationPaused event to Java side, used for Unity Activity
            LiveWallpaperUnityMessagesForwarder.ApplicationPaused += OnApplicationPaused;
#endif
        }

#if !UNITY_EDITOR || UNITY_EDITOR_OVERRIDE
        private static void OnApplicationPaused(bool isPaused) {
            _facadeObject.Call("updateUnityPlayerActivityContext");
        }
#endif

        [RuntimeInitializeOnLoadMethod]
        private static void InitializationMethod() {
            LiveWallpaperUnityMessagesForwarder.UpdateInstanceOnLoad();
        }
    }
}

#endif
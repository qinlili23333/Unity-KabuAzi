#if UNITY_ANDROID

using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
        /// <summary>
        /// Gets the Android Application Context.
        /// </summary>
        /// <returns>Application Context, or null on error.</returns>
        public static AndroidJavaObject GetApplicationContext() {
#if UNITY_EDITOR
            return null;
#else
            if (!_isFacadeObjectAvailable)
                return null;

            return _facadeObject.Call<AndroidJavaObject>("getApplicationContext");
#endif
        }

        /// <summary>
        /// Gets the package name of the current default launcher.
        /// </summary>
        public static string GetDefaultLauncherPackageName() {
#if UNITY_EDITOR
            return "com.android.launcher";
#else
            if (!_isFacadeObjectAvailable)
                return "";

            using (AndroidJavaObject context = GetApplicationContext()) {
                using (AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager")) {
                    const string INTENT_ACTION_MAIN = "android.intent.action.MAIN";
                    const string INTENT_CATEGORY_HOME = "android.intent.category.HOME";
                    const string INTENT_CATEGORY_DEFAULT = "android.intent.category.DEFAULT";
                    using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", INTENT_ACTION_MAIN)) {
                        intent.Call<AndroidJavaObject>("addCategory", INTENT_CATEGORY_HOME);
                        intent.Call<AndroidJavaObject>("addCategory", INTENT_CATEGORY_DEFAULT);
                        using (AndroidJavaObject resolveInfo = packageManager.Call<AndroidJavaObject>("resolveActivity", intent, 0)) {
                            using (AndroidJavaObject activityInfo = resolveInfo.Get<AndroidJavaObject>("activityInfo")) {
                                using (AndroidJavaObject applicationInfo = activityInfo.Get<AndroidJavaObject>("applicationInfo")) {
                                    string packageName = applicationInfo.Get<string>("packageName");
                                    return packageName;
                                }
                            }
                        }
                    }
                }
            }

            /*
            // Equivalent Java code

            PackageManager pm = getApplicationContext().getPackageManager();
            Intent intent = new Intent(Intent.ACTION_MAIN);
            intent.addCategory(Intent.CATEGORY_HOME);
            intent.addCategory(Intent.CATEGORY_DEFAULT);
            final ResolveInfo resolveInfo = pm.resolveActivity(intent, 0);
            return resolveInfo.activityInfo.applicationInfo.packageName);
            */
#endif
        }

        /// <summary>
        /// Attempts to start the Settings Activity named <c>SettingsActivity</c>.
        /// </summary>
        public static void StartDefaultSettingsActivity() {
            string activityClassFullyQualifiedName = Application.identifier + ".SettingsActivity";

            StartActivity(activityClassFullyQualifiedName);
        }

        /// <summary>
        /// Starts an Android Activity using the Application Context.
        /// </summary>
        /// <param name="activityClassName">Fully qualified name of the Activity class.</param>
        /// <seealso cref="GetApplicationContext"/>
        public static void StartActivity(string activityClassName) {
            if (String.IsNullOrEmpty(activityClassName))
                throw new ArgumentNullException("activityClassName");

#if UNITY_EDITOR
            Debug.LogFormat("LiveWallpaper.StartActivity(\"{0}\") called", activityClassName);
#else
            if (!_isFacadeObjectAvailable)
                return;

            Debug.LogFormat("Attempting to start Activity '{0}'", activityClassName);
            using (AndroidJavaClass settingsActivityClass = new AndroidJavaClass(activityClassName)) {
                using (AndroidJavaObject context = GetApplicationContext()) {
                    using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", context, settingsActivityClass)) {
                        const int FLAG_ACTIVITY_NEW_TASK = 0x10000000;
                        intent.Call<AndroidJavaObject>("setFlags", FLAG_ACTIVITY_NEW_TASK);
                        context.Call("startActivity", intent);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Opens the live wallpaper preview screen.
        /// </summary>
        /// <seealso cref="GetApplicationContext"/>
        public static void OpenPreviewScreen() {
#if UNITY_EDITOR
            Debug.LogFormat("LiveWallpaper.OpenPreviewScreen() called");
#else
            if (!_isFacadeObjectAvailable)
                return;

            Debug.LogFormat("Attempting to start wallpaper preview screen");
            using (AndroidJavaClass liveWallpaperUtilityClass = new AndroidJavaClass("com.lostpolygon.unity.livewallpaper.LiveWallpaperUtility")) {
                using (AndroidJavaObject context = GetApplicationContext()) {
                    liveWallpaperUtilityClass.CallStatic<bool>("openWallpaperPreview", context, true);
                }
            }
#endif
        }

        /// <summary>
        /// Open the URL using Android API.
        /// </summary>
        public static void OpenURL(string url) {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

#if UNITY_EDITOR
            Application.OpenURL(url);
#else
            if (!_isFacadeObjectAvailable)
                return;

            Debug.LogFormat("Attempting to open URL '{0}'", url);

            const string ACTION_VIEW = "android.intent.action.VIEW";
            const int FLAG_ACTIVITY_NEW_TASK = 0x10000000;
            using (AndroidJavaObject context = GetApplicationContext()) {
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri")) {
                    using (AndroidJavaObject uriUrl = uriClass.CallStatic<AndroidJavaObject>("parse", url)) {
                        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW, uriUrl)) {
                            intent.Call<AndroidJavaObject>("setFlags", FLAG_ACTIVITY_NEW_TASK);
                            context.Call("startActivity", intent);
                        }
                    }
                }
            }

            /*

            // Equivalent Java code
            Uri uriUrl = Uri.parse( url );
            Intent myIntent = new Intent(Intent.ACTION_VIEW, uriUrl);
            myIntent.setFlags( Intent.FLAG_ACTIVITY_NEW_TASK );
            startActivity( myIntent );

            */
#endif
        }
    }
}

#endif
#if UNITY_ANDROID

using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    public static partial class LiveWallpaper {
        /// <summary>
        /// Provides access to Android preferences.
        /// </summary>
        /// <remarks>
        /// Before editing any values, <see cref="StartEditing"/> must be called.
        /// You must call <see cref="FinishEditing"/> to submit the changes after you've finished editing,
        /// other your actions will have no effect.
        /// </remarks>
        public static class Preferences {
            private const string kClassCastExceptionMessage = "cannot be cast to";
#if UNITY_EDITOR
            private static bool _isEditingStarted;
#endif

            /// <summary>
            /// Starts preference editing session.
            /// </summary>
            /// <returns>true, if session was started successfully, false if session was already started, or on error.</returns>
            public static bool StartEditing() {
#if UNITY_EDITOR
                if (!_isEditingStarted) {
                    _isEditingStarted = true;
                    return true;
                }

                return false;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("startEditing");
#endif
            }

            /// <summary>
            /// Closes the preference editing session and saves changes to storage.
            /// </summary>
            /// <returns>true, if session was closed successfully, false if no session was started, or on error.</returns>
            public static bool FinishEditing() {
#if UNITY_EDITOR
                if (_isEditingStarted) {
                    PlayerPrefs.Save();
                    _isEditingStarted = false;

                    return true;
                }

                return false;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("finishEditing");
#endif
            }

            /// <summary>
            /// Checks whether the preferences contains a preference.
            /// </summary>
            /// <param name="key">Preference key to check.</param>
            /// <returns>true if the preference exists, false otherwise.</returns>
            public static bool HasKey(string key) {
#if UNITY_EDITOR
                return PlayerPrefs.HasKey(key);
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("hasKey", key);
#endif
            }

            /// <summary>
            /// Retrieves a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="defaultValue">Value to be returned if preference had no value, or on error.</param>
            /// <returns>Value of the preference with key <paramref name="key"/>,
            /// <paramref name="defaultValue"/> if preference has no value, or on error.</returns>
            public static bool GetBool(string key, bool defaultValue = false) {
#if UNITY_EDITOR
                return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
#else
                if (!_isFacadeObjectAvailable)
                    return defaultValue;

                try {
                    return _preferencesEditorFacadeObject.Call<bool>("getBoolean", key, defaultValue);
                } catch (AndroidJavaException e) {
                    if (e.Message.Contains(kClassCastExceptionMessage))
                        throw new InvalidPreferenceTypeException(key, "bool", e);

                    throw;
                }
#endif
            }

            /// <summary>
            /// Retrieves a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="defaultValue">Value to be returned if preference had no value, or on error.</param>
            /// <returns>Value of the preference with key <paramref name="key"/>,
            /// <paramref name="defaultValue"/> if preference has no value, or on error.</returns>
            public static int GetInt(string key, int defaultValue = 0) {
#if UNITY_EDITOR
                return PlayerPrefs.GetInt(key, defaultValue);
#else
                if (!_isFacadeObjectAvailable)
                    return defaultValue;

                try {
                    return _preferencesEditorFacadeObject.Call<int>("getInt", key, defaultValue);
                } catch (AndroidJavaException e) {
                    if (e.Message.Contains(kClassCastExceptionMessage))
                        throw new InvalidPreferenceTypeException(key, "int", e);

                    throw;
                }
#endif
            }

            /// <summary>
            /// Retrieves a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="defaultValue">Value to be returned if preference had no value, or on error.</param>
            /// <returns>Value of the preference with key <paramref name="key"/>,
            /// <paramref name="defaultValue"/> if preference has no value, or on error.</returns>
            public static long GetLong(string key, long defaultValue = 0) {
#if UNITY_EDITOR
                long result;
                string prefsValue = PlayerPrefs.GetString(key, defaultValue.ToString());
                if (!long.TryParse(prefsValue, out result)) {
                    result = defaultValue;
                }

                return result;
#else
                if (!_isFacadeObjectAvailable)
                    return defaultValue;

                try {
                    return _preferencesEditorFacadeObject.Call<long>("getLong", key, defaultValue);
                } catch (AndroidJavaException e) {
                    if (e.Message.Contains(kClassCastExceptionMessage))
                        throw new InvalidPreferenceTypeException(key, "long", e);

                    throw;
                }
#endif
            }

            /// <summary>
            /// Retrieves a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="defaultValue">Value to be returned if preference had no value, or on error.</param>
            /// <returns>Value of the preference with key <paramref name="key"/>,
            /// <paramref name="defaultValue"/> if preference has no value, or on error.</returns>
            public static float GetFloat(string key, float defaultValue = 0f) {
#if UNITY_EDITOR
                return PlayerPrefs.GetFloat(key, defaultValue);
#else
                if (!_isFacadeObjectAvailable)
                    return defaultValue;

                try {
                    return _preferencesEditorFacadeObject.Call<float>("getFloat", key, defaultValue);
                } catch (AndroidJavaException e) {
                    if (e.Message.Contains(kClassCastExceptionMessage))
                        throw new InvalidPreferenceTypeException(key, "float", e);

                    throw;
                }
#endif
            }

            /// <summary>
            /// Retrieves a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="defaultValue">Value to be returned if preference had no value, or on error.</param>
            /// <returns>Value of the preference with key <paramref name="key"/>,
            /// <paramref name="defaultValue"/> if preference has no value, or on error.</returns>
            public static string GetString(string key, string defaultValue = "") {
#if UNITY_EDITOR
                return PlayerPrefs.GetString(key, defaultValue);
#else
                if (!_isFacadeObjectAvailable)
                    return defaultValue;

                try {
                    return _preferencesEditorFacadeObject.Call<string>("getString", key, defaultValue);
                } catch (AndroidJavaException e) {
                    if (e.Message.Contains(kClassCastExceptionMessage))
                        throw new InvalidPreferenceTypeException(key, "string", e);

                    throw;
                }
#endif
            }

            /// <summary>
            /// Sets a preference value.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="value">Value to be written into preference.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool SetBool(string key, bool value) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.SetInt(key, value ? 1 : 0);
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("putBoolean", key, value);
#endif
            }

            /// <summary>
            /// Sets a preference value.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="value">Value to be written into preference.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool SetInt(string key, int value) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.SetInt(key, value);
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("putInt", key, value);
#endif
            }

            /// <summary>
            /// Sets a preference value.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="value">Value to be written into preference.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool SetLong(string key, long value) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.SetString(key, value.ToString());
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("putLong", key, value);
#endif
            }

            /// <summary>
            /// Sets a preference value.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="value">Value to be written into preference.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool SetFloat(string key, float value) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.SetFloat(key, value);
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("putFloat", key, value);
#endif
            }

            /// <summary>
            /// Sets a preference value.
            /// </summary>
            /// <param name="key">Preference key.</param>
            /// <param name="value">Value to be written into preference.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool SetString(string key, string value) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.SetString(key, value);
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("putString", key, value);
#endif
            }

            /// <summary>
            /// Removes a value from the preferences.
            /// </summary>
            /// <param name="key">Preference key to remove.</param>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool DeleteKey(string key) {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.DeleteKey(key);
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("remove", key);
#endif
            }

            /// <summary>
            /// Removes all keys and values from the preferences. Use with caution.
            /// </summary>
            /// <returns>true on success, false if editing session has not been started, on error.</returns>
            public static bool DeleteAll() {
#if UNITY_EDITOR
                if (!_isEditingStarted)
                    return false;

                PlayerPrefs.DeleteAll();
                return true;
#else
                if (!_isFacadeObjectAvailable)
                    return false;

                return _preferencesEditorFacadeObject.Call<bool>("clear");
#endif
            }
        }
    }
}

#endif
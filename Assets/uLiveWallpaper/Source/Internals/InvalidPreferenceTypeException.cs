#if UNITY_ANDROID

using System;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// This exception is thrown when stored preference is different than type it was attempted to get as.
    /// </summary>
    public class InvalidPreferenceTypeException : Exception {
        public string PreferenceKey { get; private set; }
        public string AttemptedAsType { get; private set; }

        public InvalidPreferenceTypeException(string preferenceKey, string attemptedAsType, Exception innerException)
            : base(
                  string.Format(
                      "Preference '{0}' was attempted to be read as type '{1}', but the stored value is of different type.\n{2}",
                      preferenceKey,
                      attemptedAsType,
                      innerException)) {
            PreferenceKey = preferenceKey;
            AttemptedAsType = attemptedAsType;
        }
    }
}

#endif
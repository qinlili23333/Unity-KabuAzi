using System;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Utilities for working with enums.
    /// </summary>
    internal static class EnumExtensions {
        public static bool HasFlag<T>(this Enum type, T flag) {
            long keysVal = Convert.ToInt64(type);
            long flagVal = Convert.ToInt64(flag);

            return (keysVal & flagVal) == flagVal;
        }

        public static Enum SetFlag<T>(this Enum type, T flag, bool setFlag = true) {
            long enumValue = Convert.ToInt64(type);
            long flagValue = Convert.ToInt64(flag);
            if (setFlag) {
                enumValue |= flagValue;
            } else {
                enumValue &= ~flagValue;
            }

            return (Enum) Enum.ToObject(type.GetType(), enumValue);
        }
    }
}
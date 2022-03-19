using System;
using System.Collections.Generic;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Utilities for working with dictionaries.
    /// </summary>
    internal static class IDictionaryExtensions {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            foreach (KeyValuePair<TKey, TValue> item in collection) {
                source.Add(item.Key, item.Value);
            }
        }
    }
}
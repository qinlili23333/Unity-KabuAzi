using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Helper class for managing resources in Editor code.
    /// </summary>
    internal static class EditorResourcesManager {
        private const string kPersistentMarkerName = "uLiveWallpaperPersistentMarker";
        private static readonly Dictionary<string, Object> _resourcesMap = new Dictionary<string, Object>();
        private static TextAsset _persistentMarker;

        public static T Get<T>(string path, bool silentErrors = false) where T : Object {
            Object resource;
            bool isFound = _resourcesMap.TryGetValue(path, out resource);
            if (!isFound || resource == null) {
                string fullPath = GetFullAssetPath(path);
                resource = AssetDatabase.LoadAssetAtPath(fullPath, typeof(T));
                if (resource != null) {
                    _resourcesMap[path] = resource;
                } else {
                    if (!silentErrors) {
                        Debug.LogErrorFormat("Resource '{0}' not found", path);
                    }
                }
            }

            return (T) resource;
        }

        private static string GetFullAssetPath(string path) {
            return PersistentEditorResourcesPath + "/" + path;
        }

        private static string PersistentEditorResourcesPath {
            get {
                if (_persistentMarker == null) {
                    string[] foundAssets = AssetDatabase.FindAssets("t:TextAsset " + kPersistentMarkerName);
                    foreach (string foundAsset in foundAssets) {
                        string assetPath = AssetDatabase.GUIDToAssetPath(foundAsset);
                        if (Path.GetFileName(assetPath) != kPersistentMarkerName + ".txt")
                            continue;

                        _persistentMarker = (TextAsset) AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
                        if (_persistentMarker != null)
                            break;
                    }

                    if (_persistentMarker == null)
                        throw new FileNotFoundException(kPersistentMarkerName + " not found. Please re-import uLiveWallpaper.");
                }

                string markerPath = AssetDatabase.GetAssetPath(_persistentMarker);
                DirectoryInfo persistentDataDirectoryInfo = new DirectoryInfo(markerPath).Parent;
                string relativePath = IOUtilities.MakeRelativePath(persistentDataDirectoryInfo.FullName, Application.dataPath);
                return relativePath;
            }
        }
    }
}

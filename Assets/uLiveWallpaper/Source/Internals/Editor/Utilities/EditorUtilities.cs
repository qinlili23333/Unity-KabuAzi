using System.Linq;
using UnityEditor;
#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
using UnityEngine.SceneManagement;
#endif

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Editor-related utility methods.
    /// </summary>
    internal static class EditorUtilities {
        /// <summary>
        /// Returns current build scenes. Correctly handles the case when no scenes are added to Build Settings.
        /// </summary>
        /// <returns>
        /// The list of current build scenes.
        /// </returns>
        public static string[] GetCurrentBuildScenes() {
            string[] buildScenes =
                EditorBuildSettings
                    .scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray();

            if (buildScenes.Length == 0) {
#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                buildScenes =
                    GetAllScenes()
                    .Where(scene => scene.isLoaded && scene.IsValid() && !string.IsNullOrEmpty(scene.path))
                    .Select(scene => scene.path)
                    .ToArray();
#else
                buildScenes =
                    string.IsNullOrEmpty(EditorApplication.currentScene) ?
                        new string[0] :
                        new[] { EditorApplication.currentScene };
#endif
            }

            buildScenes =
                buildScenes
                    .Where(scene => !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(scene)))
                    .ToArray();

            return buildScenes;
        }

#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
        private static Scene[] GetAllScenes() {
            Scene[] scenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < scenes.Length; i++) {
                scenes[i] = SceneManager.GetSceneAt(i);
            }

            return scenes;
        }
#endif
    }
}

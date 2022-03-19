using System.IO;
using UnityEditor;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    /// <summary>
    /// Build pipeline related utility methods.
    /// </summary>
    public static class BuildPipelineUtilities {
        /// <summary>
        /// Creates an Android project.
        /// </summary>
        /// <param name="path">Project destination path.</param>
        /// <param name="buildSystem">The Android build system that the created project will use.</param>
        /// <param name="buildScenes">Scenes to include into build. Scene list from Build Settings will be used if this argument is set to null.</param>
        /// <param name="buildOptions"><see cref="BuildOptions" />.</param>
        /// <param name="packageName">Android package name.</param>
        /// <param name="unityProjectPath">The resulting ADT project may contain multiple modules.
        /// This variable will contain the path to the Unity module.</param>
        /// <returns></returns>
        public static bool BuildAndroidProject(
            string path,
            EditorUserBuildSettingsWrapper.AndroidBuildSystem buildSystem,
            string[] buildScenes,
            BuildOptions buildOptions,
            string packageName,
            out string unityProjectPath) {
            unityProjectPath = null;

            // Create Android project
            buildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;

            string currentPackageName = PlayerSettings.applicationIdentifier;
            if (!string.IsNullOrEmpty(packageName)) {
                PlayerSettings.applicationIdentifier = packageName;
            }

            EditorUserBuildSettingsWrapper.AndroidBuildSystem currentBuildSystem = EditorUserBuildSettingsWrapper.androidBuildSystem;
            try {
                EditorUserBuildSettingsWrapper.androidBuildSystem = buildSystem;
                string error = BuildPipeline.BuildPlayer(buildScenes, path, BuildTarget.Android, buildOptions).ToString();
                if (!string.IsNullOrEmpty(error))
                    return false;
            } finally {
                PlayerSettings.applicationIdentifier = currentPackageName;
                EditorUserBuildSettingsWrapper.androidBuildSystem = currentBuildSystem;
            }

            // Get the Unity module path
            string productName = PlayerSettings.productName;
            unityProjectPath = Path.Combine(path, productName);

            return true;
        }

        /// <summary>
        /// Gets currently active <see cref="BuildOptions"/>.
        /// </summary>
        /// <returns></returns>
        public static BuildOptions GetCurrentBuildOptions() {
            BuildOptions buildOptions = BuildOptions.None;
            if (EditorUserBuildSettings.development)
                buildOptions |= BuildOptions.Development;
            if (EditorUserBuildSettings.allowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;
            if (EditorUserBuildSettings.connectProfiler)
                buildOptions |= BuildOptions.ConnectWithProfiler;

            return buildOptions;
        }
    }
}
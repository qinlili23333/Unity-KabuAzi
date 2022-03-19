using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    /// <summary>
    /// User interface and related utility methods.
    /// </summary>
    internal static class LiveWallpaperBuildGuiUtility {
        public static ProjectCreateState GetProjectCreateState(string projectPath) {
            bool directoryExists = Directory.Exists(projectPath);
            if (directoryExists) {
                if (!IOUtilities.IsDirectory(projectPath)) {
                    return ProjectCreateState.ProjectPathIsNotDirectory;
                }

                if (!IOUtilities.IsDirectoryEmpty(projectPath)) {
                    AndroidBuildSystem buildSystem = ProjectDataExtractor.GetProjectType(projectPath);
                    switch (buildSystem) {
                        case AndroidBuildSystem.Gradle:
                            return ProjectCreateState.DetectedAndroidStudioProject;
                        case AndroidBuildSystem.Adt:
                            return ProjectCreateState.DetectedEclipseAdtProject;
                        case AndroidBuildSystem.NotDetected:
                            // Look for Unity project
                            string productName = PlayerSettings.productName;
                            string unityProjectPath = Path.Combine(projectPath, productName);
                            if (Directory.Exists(unityProjectPath)) {
                                buildSystem = ProjectDataExtractor.GetProjectType(unityProjectPath);
                                switch (buildSystem) {
                                    case AndroidBuildSystem.Gradle:
                                        return ProjectCreateState.DetectedAndroidStudioProject;
                                    case AndroidBuildSystem.Adt:
                                        return ProjectCreateState.DetectedEclipseAdtProject;
                                }
                            }
                            return ProjectCreateState.DirectoryNotEmpty;
                    }
                }
            } else {
                if (string.IsNullOrEmpty(projectPath)) {
                    return ProjectCreateState.ProjectPathEmpty;
                }

                bool parentDirectoryExists = false;
                bool invalidPath = false;
                try {
                    string parentDirectoryPath = Directory.GetParent(projectPath).FullName;
                    parentDirectoryExists = Directory.Exists(parentDirectoryPath);
                } catch (ArgumentException) {
                    invalidPath = true;
                } catch {
                    // Ignored
                }

                if (invalidPath)
                    return ProjectCreateState.ProjectPathIsInvalid;

                if (!parentDirectoryExists)
                    return ProjectCreateState.ParentDirectoryNotExists;
            }

            return ProjectCreateState.CanCreateProject;
        }

        public static ProjectUpdateState GetProjectUpdateState(string projectPath) {
            bool directoryExists = Directory.Exists(projectPath);
            if (directoryExists) {
                if (!IOUtilities.IsDirectory(projectPath)) {
                    return ProjectUpdateState.ProjectPathIsNotDirectory;
                }

                if (!IOUtilities.IsDirectoryEmpty(projectPath)) {
                    AndroidBuildSystem buildSystem = ProjectDataExtractor.GetProjectType(projectPath);
                    switch (buildSystem) {
                        case AndroidBuildSystem.Gradle:
                            return ProjectUpdateState.DetectedAndroidStudioProject;
                        case AndroidBuildSystem.Adt:
                            return ProjectUpdateState.DetectedEclipseAdtProject;
                        case AndroidBuildSystem.NotDetected:
                            return ProjectUpdateState.NoProjectDetected;
                    }
                } else {
                    return ProjectUpdateState.NoProjectDetected;
                }
            } else {
                if (string.IsNullOrEmpty(projectPath))
                    return ProjectUpdateState.ProjectPathEmpty;

                return ProjectUpdateState.DirectoryNotExists;
            }

            return ProjectUpdateState.Unknown;
        }

        public static void UpdateLiveWallpaperProject(ProjectSettingsContainer projectSettings) {
            try {
                LiveWallpaperGradleProjectUpdater updater = new LiveWallpaperGradleProjectUpdater(projectSettings.ProjectUpdatePath);
                updater.BuildOptions = BuildPipelineUtilities.GetCurrentBuildOptions();
                updater.BuildScenes = EditorUtilities.GetCurrentBuildScenes();
                updater.StagingProjectBuildFinished += () => EditorUtility.DisplayProgressBar("uLiveWallpaper", "Post-processing project files", 1f);

                updater.UpdateProject();

                Debug.Log("<i>Android Studio</i> project updated successfully.");
            } catch (UnauthorizedAccessException e) {
                Debug.LogError("It seems you have no access to the project directory, or perhaps" +
                               "some other application is blocking access to the project directory." +
                               "Try closing all applications that could possibly use the project directory, or do a reboot, and try again.\n" +
                               e);
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void CreateLiveWallpaperProject(ProjectSettingsContainer projectSettings) {
            try {
                AndroidBuildSystem buildSystem = UnityVersionUtility.IsGradleBuildSystemSupported ? AndroidBuildSystem.Gradle : AndroidBuildSystem.Adt;

                LiveWallpaperProjectBuilder builder = new LiveWallpaperProjectBuilder(projectSettings.ProjectCreatePath);
                builder.BuildSystem = buildSystem;
                builder.BuildOptions = BuildPipelineUtilities.GetCurrentBuildOptions();
                builder.BuildScenes = EditorUtilities.GetCurrentBuildScenes();
                builder.SettingsActivityGenerationMode = projectSettings.LiveWallpaperSettingsActivityGenerationMode;
                builder.LauncherShortcutGenerationMode = projectSettings.LiveWallpaperLauncherShortcutGenerationMode;
                builder.LiveWallpaperBuildOptions = projectSettings.LiveWallpaperBuildOptions;

                builder.StagingProjectBuildFinished += () => EditorUtility.DisplayProgressBar("uLiveWallpaper", "Post-processing project files", 1f);

                builder.BuildProject();

                // Show warning if ADT project has multiple modules
                if (buildSystem == AndroidBuildSystem.Adt && Path.GetFullPath(builder.ProjectRootPath) != Path.GetFullPath(builder.UnityProjectPath)) {
                    string[] modules = Directory.GetDirectories(builder.ProjectRootPath);
                    string mainModule = Path.GetFileName(builder.UnityProjectPath);
                    string warningMessage =
                        "Exported Eclipse ADT project was split into multiple projects. " +
                        "This is likely because your project has other native Android libraries (from other Asset Store assets, for example).\n\n" +
                        "Additional projects are:\n" +
                        modules
                            .Select(s => Path.GetFileName(s))
                            .Except(new[] { mainModule })
                            .Select(s => "-\u00A0" + s)
                            .Aggregate((current, next) => current + "\n" + next) +
                        "\n\n" +
                        "The Eclipse ADT project you must import into Android Studio is '" + mainModule +
                        "', located at:\n" +
                        Path.GetFullPath(builder.UnityProjectPath).Replace(' ', '\u00A0') +
                        "\n\n";

                    EditorUtility.DisplayDialog("Eclipse ADT project split", warningMessage, "OK");
                }

                // Open the directory with the built project
                string revealedFilePath = UnityVersionUtility.IsGradleBuildSystemSupported ? "build.gradle" : "AndroidManifest.xml";
                EditorUtility.RevealInFinder(Path.GetFullPath(PathUtilities.Combine(builder.UnityProjectPath, revealedFilePath)));

                Debug.Log(
                    buildSystem == AndroidBuildSystem.Gradle ?
                    "Project created successfully. Now open it in <i>Android Studio</i>." :
                    "Project created successfully. Now import it to <i>Android Studio</i>.");
            } catch (UnauthorizedAccessException e) {
                Debug.LogError("It seems you have no access to the project directory, or perhaps " +
                               "some other application is blocking access to the project directory. " +
                               "Try closing all applications that could possibly use the project directory, or do a reboot, and try again.\n" +
                               e);
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        public enum ProjectCreateState {
            Unknown,
            CanCreateProject,
            ProjectPathIsNotDirectory,
            ParentDirectoryNotExists,
            ProjectPathIsInvalid,
            DetectedAndroidStudioProject,
            DetectedEclipseAdtProject,
            ProjectPathEmpty,
            DirectoryNotEmpty
        }

        public enum ProjectUpdateState {
            Unknown,
            ProjectPathEmpty,
            DirectoryNotExists,
            ProjectPathIsNotDirectory,
            DetectedAndroidStudioProject,
            DetectedEclipseAdtProject,
            NoProjectDetected
        }
    }
}
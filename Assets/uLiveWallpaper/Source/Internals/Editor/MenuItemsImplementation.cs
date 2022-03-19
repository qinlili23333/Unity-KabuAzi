using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    public static class MenuItemsImplementation {
        public static void UpdateProject() {
            ProjectSettingsContainer projectSettings = ProjectSettingsContainer.Instance;

            bool isError = true;
            LiveWallpaperBuildGuiUtility.ProjectUpdateState projectUpdateState =
                LiveWallpaperBuildGuiUtility.GetProjectUpdateState(projectSettings.ProjectUpdatePath);
            switch (projectUpdateState) {
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DetectedAndroidStudioProject:
                    isError = false;
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DetectedEclipseAdtProject:
                    Debug.LogErrorFormat(
                        "Project Update failed. Detected <i>Eclipse ADT</i> project at path '{0}'. Please import the project to <i>Android Studio</i> first.",
                        projectSettings.ProjectUpdatePath
                        );
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.NoProjectDetected:
                    Debug.LogErrorFormat(
                        "Project Update failed. No project detected at path '{0}'. Please select a valid directory with <i>Android Studio</i> project.",
                        projectSettings.ProjectUpdatePath
                        );
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.ProjectPathEmpty:
                    Debug.Log("Project Update failed. Project path is empty.");
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DirectoryNotExists:
                    Debug.LogErrorFormat(
                        "Project Update failed. Directory '{0}' does not exists. Please select a valid directory with <i>Android Studio</i> project.",
                        projectSettings.ProjectUpdatePath
                        );
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.ProjectPathIsNotDirectory:
                    Debug.LogErrorFormat(
                        "Project Update failed. '{0}' is not a directory. Please select a valid empty directory.",
                        projectSettings.ProjectUpdatePath
                        );
                    break;
                default:
                    throw
                        new InvalidEnumArgumentException(
                            "projectUpdateState",
                            (int) projectUpdateState,
                            typeof(LiveWallpaperBuildGuiUtility.ProjectUpdateState));
            }

            if (isError) {
                projectSettings.MainWindowCurrentTab = ProjectSettingsContainer.MainWindowState.UpdateProject;
                EditorWindow.GetWindow<LiveWallpaperBuildWindow>(true, "uLiveWallpaper", true);
                return;
            }

            try {
                LiveWallpaperBuildGuiUtility.UpdateLiveWallpaperProject(projectSettings);
            } catch (OperationCanceledException) {
            }
        }
    }
}
using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    /// <summary>
    /// Builds Unity player data and updates Gradle project with it.
    /// </summary>
    public class LiveWallpaperGradleProjectUpdater : LiveWallpaperProjectManipulatorBase {
        private ProjectDataExtractor.ProjectData _updatedProjectData;
        private XmlDocument _androidManifestXmlDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveWallpaperGradleProjectUpdater"/> class.
        /// </summary>
        /// <param name="projectRootPath">Project root path.</param>
        public LiveWallpaperGradleProjectUpdater(string projectRootPath)
            : base(projectRootPath) {
        }

        /// <summary>
        /// Builds Unity player data and updates Android Studio project with it.
        /// </summary>
        /// <exception cref="System.OperationCanceledException"></exception>
        public void UpdateProject() {
            // Build staging Android project. It is only used to retrieve Unity player data
            string stagingAdtProjectPath = PathUtilities.GetTempFilePath() + "_uLWP_Temp";

            try {
                if (_buildScenes == null) {
                    _buildScenes = EditorUtilities.GetCurrentBuildScenes();
                }

                _updatedProjectData = ProjectDataExtractor.ExtractProjectData(_projectRootPath);
                LoadAndroidManifest(_updatedProjectData.AndroidManifestPath);

                // Build staging project
                string unityProjectPath = BuildStagingProject(stagingAdtProjectPath);
                InvokeStagingProjectBuildFinished();

                // Update Android Studio project
                UpdateGradleProject(unityProjectPath);

                // Save AndroidManifest.xml
                SaveAndUnloadAndroidManifest(_updatedProjectData.AndroidManifestPath);
            } finally {
                IOUtilities.DeleteDirectoryIfExists(stagingAdtProjectPath, true);
            }
        }

        private void UpdateGradleProject(string stagingAdtProjectPath) {
            // Extract project data from staging and updated projects
            ProjectDataExtractor.ProjectData stagingProjectData = ProjectDataExtractor.ExtractProjectData(stagingAdtProjectPath);

            // Update unity-classes.jar
            try {
                if (!File.Exists(_updatedProjectData.UnityClassesJarPath)) {
                    // Try unity-classes module
                    string unityClassModuleJarPath = PathUtilities.Combine(_projectRootPath, "unity-classes", "unity-classes.jar");
                    if (File.Exists(unityClassModuleJarPath)) {
                        IOUtilities.DeleteFileIfExists(unityClassModuleJarPath);
                        IOUtilities.AttemptPotentiallyFailingOperation(() => File.Move(stagingProjectData.UnityClassesJarPath, unityClassModuleJarPath));
                    } else {
                        Debug.LogWarningFormat(
                            "'{0}' or '{1}' file not found. Unity Java library not updated.",
                            _updatedProjectData.UnityClassesJarPath,
                            unityClassModuleJarPath);
                    }
                } else {
                    IOUtilities.DeleteFileIfExists(_updatedProjectData.UnityClassesJarPath);
                    IOUtilities.AttemptPotentiallyFailingOperation(() => File.Move(stagingProjectData.UnityClassesJarPath, _updatedProjectData.UnityClassesJarPath));
                }
            } catch (Exception e) {
                Debug.LogWarningFormat("Unity Java library not updated due to error: {0}", e.ToString());
            }

            // Delete all Unity assets and move new from the staging project
            IOUtilities.DeleteDirectoryIfExists(_updatedProjectData.UnityAssetsBinPath, true);
            try {
                IOUtilities.MoveDirectory(stagingProjectData.UnityAssetsBinPath, _updatedProjectData.UnityAssetsBinPath);
            } catch (Exception e) {
                Debug.LogErrorFormat("Error while moving directory '{0}' to '{1}', falling back to copying:\r\n" + e);
                IOUtilities.CopyDirectory(stagingProjectData.UnityAssetsBinPath, _updatedProjectData.UnityAssetsBinPath);
            }

            // Update StreamingAssets
            IOUtilities.MoveDirectoryContents(stagingProjectData.UnityAssetsPath, _updatedProjectData.UnityAssetsPath, true);

            // Delete old native libraries
            foreach (var architectureLibraryInfo in _updatedProjectData.ArchitectureLibraryInfos) {
                foreach (var libraryInfo in architectureLibraryInfo.LibrariesInfos) {
                    if (!libraryInfo.FileExists)
                        continue;

                    IOUtilities.AttemptPotentiallyFailingOperation(() => File.Delete(libraryInfo.Path));
                }

                if (IOUtilities.IsDirectoryEmpty(architectureLibraryInfo.ArchitecturePath)) {
                    IOUtilities.AttemptPotentiallyFailingOperation(() => Directory.Delete(architectureLibraryInfo.ArchitecturePath));
                }
            }

            // Move new native libraries
            foreach (var architectureLibraryInfo in stagingProjectData.ArchitectureLibraryInfos) {
                string updatedArchitectureFolderPath = Path.Combine(_updatedProjectData.UnityLibsPath, architectureLibraryInfo.ArchitectureName);
                Directory.CreateDirectory(updatedArchitectureFolderPath);

                foreach (var libraryInfo in architectureLibraryInfo.LibrariesInfos) {
                    string updatedLibraryPath = Path.Combine(updatedArchitectureFolderPath, libraryInfo.Name);
                    if (libraryInfo.FileExists) {
                        IOUtilities.AttemptPotentiallyFailingOperation(() => File.Move(libraryInfo.Path, updatedLibraryPath));
                    }
                }
            }

            // Update LP_uLiveWallpaper.aar
            string localULiveWallpaperAarLibraryPath = GetULiveWallpaperAarLocalLibraryPath();
            if (localULiveWallpaperAarLibraryPath != null) {
                string style1UpdatedULiveWallpaperAarLibraryModulePath = Path.Combine(_projectRootPath, Constants.kULiveWallpaperLibraryName);
                string style1UpdatedULiveWallpaperAarLibraryPath = Path.Combine(style1UpdatedULiveWallpaperAarLibraryModulePath, Constants.kULiveWallpaperLibraryName + ".aar");

                if (Directory.Exists(style1UpdatedULiveWallpaperAarLibraryModulePath)) {
                    IOUtilities.AttemptPotentiallyFailingOperation(() => File.Copy(localULiveWallpaperAarLibraryPath, style1UpdatedULiveWallpaperAarLibraryPath, true));
                } else {
                    string libsPath = Path.Combine(_projectRootPath, "libs");
                    string style2UpdatedULiveWallpaperAarLibraryPath = Path.Combine(libsPath, Constants.kULiveWallpaperLibraryName + ".aar");
                    if (File.Exists(style2UpdatedULiveWallpaperAarLibraryPath)) {
                        IOUtilities.AttemptPotentiallyFailingOperation(() => File.Copy(localULiveWallpaperAarLibraryPath, style2UpdatedULiveWallpaperAarLibraryPath, true));
                    } else {
                        Debug.LogWarningFormat(
                            "'{0}' file or '{1}' module directory not found. uLiveWallpaper library not updated.",
                            style1UpdatedULiveWallpaperAarLibraryModulePath,
                            style2UpdatedULiveWallpaperAarLibraryPath);
                    }
                }
            } else {
                throw new FileNotFoundException(string.Format("{0} library could not be found in Unity project. Please re-import uLiveWallpaper.", Constants.kULiveWallpaperLibraryName));
            }

            // Update <meta-data android:name="uLiveWallpaper.unityversion" android:value="5.3.6"/>
            UpdateUnityVersionMetaDataTag(_androidManifestXmlDocument);

            // Removes Activities that come from uLiveWallpaper library
            RemoveLibraryAndroidActivities(_androidManifestXmlDocument);
        }

        private string BuildStagingProject(string stagingAdtProjectPath) {
            // Get package name from project AndroidManifest
            XmlElement manifestElement = _androidManifestXmlDocument.DocumentElement;
            XmlAttribute packageAttribute = manifestElement.Attributes["package"];
            if (packageAttribute == null)
                throw new Exception("No 'package' attribute found in " + kAndroidManifestXmlName);

            string foundPackageName = packageAttribute.Value;
            string currentPackageName = PlayerSettings.applicationIdentifier;

            if (foundPackageName != currentPackageName) {
                Debug.LogWarningFormat(
                    "Package name mismatch: bundle identifier is set to '{0}' in Unity project, but the Android Studio project package name is '{1}'. " +
                    "Value from the Android Studio project will be used.\r\n",
                    currentPackageName,
                    foundPackageName
                    );
            }

            BuildOptions buildOptions = _buildOptions | BuildOptions.AcceptExternalModificationsToPlayer;
            bool isSuccess =
                BuildPipelineUtilities.BuildAndroidProject(
                    stagingAdtProjectPath,
                    EditorUserBuildSettingsWrapper.AndroidBuildSystem.ADT,
                    _buildScenes,
                    buildOptions,
                    foundPackageName,
                    out stagingAdtProjectPath
                    );
            PlayerSettings.applicationIdentifier = currentPackageName;

            if (!isSuccess)
                throw new OperationCanceledException();

            return stagingAdtProjectPath;
        }

        private void SaveAndUnloadAndroidManifest(string manifestPath) {
            _androidManifestXmlDocument.SaveAsUtf8(manifestPath);
            _androidManifestXmlDocument = null;
        }

        private void LoadAndroidManifest(string manifestPath) {
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("AndroidManifest.xml not found in Gradle project", kAndroidManifestXmlName);

            _androidManifestXmlDocument = new XmlDocument();
            _androidManifestXmlDocument.PreserveWhitespace = true;
            _androidManifestXmlDocument.Load(manifestPath);
        }

        /// <summary>
        /// Gets the path to LP_uLiveWallpaper.aar library in the project.
        /// </summary>
        private static string GetULiveWallpaperAarLocalLibraryPath() {
            string[] foundAssetGuids = AssetDatabase.FindAssets("t:DefaultAsset " + Constants.kULiveWallpaperLibraryName);
            foreach (string assetGuid in foundAssetGuids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (Path.GetExtension(assetPath).ToLowerInvariant() != ".aar")
                    continue;

                PluginImporter pluginImporter = AssetImporter.GetAtPath(assetPath) as PluginImporter;
                if (pluginImporter == null || !pluginImporter.GetCompatibleWithPlatform(BuildTarget.Android))
                    continue;

                return assetPath;
            }

            return null;
        }
    }
}
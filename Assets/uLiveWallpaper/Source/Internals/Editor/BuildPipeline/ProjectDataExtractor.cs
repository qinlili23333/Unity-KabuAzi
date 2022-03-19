using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Utilities for analyzing the project directory and retrieving data from the project.
    /// </summary>
    internal static class ProjectDataExtractor {
        private static readonly string[] kUnityAndroidArchitectures =
            {
                "armeabi-v7a",
                "x86"
            };

        private static readonly string[] kUnityAndroidLibraries =
            {
                "libmain.so",
                "libunity.so",
                "libmono.so",
                "libil2cpp.so",
                "libil2cpp.so.debug",
            };

        /// <summary>
        /// Analyzes the project directory and extracts data regarding
        /// important files and directories in the project.
        /// </summary>
        /// <param name="projectPath">
        /// Project path.
        /// </param>
        /// <returns>
        /// <see cref="ProjectData"/> of <paramref name="projectPath"/>.
        /// </returns>
        public static ProjectData ExtractProjectData(string projectPath) {
            AndroidBuildSystem buildSystem = GetProjectType(projectPath);

            switch (buildSystem) {
                case AndroidBuildSystem.Gradle:
                    return ExtractProjectDataFromGradleProject(projectPath);
                case AndroidBuildSystem.Adt:
                    return ExtractProjectDataFromAdtProject(projectPath);
                default:
                    throw new Exception("Unknown project type.");
            }
        }

        /// <summary>
        /// Analyzes the project and returns the <see cref="AndroidBuildSystem"/>.
        /// </summary>
        /// <param name="projectPath">
        /// The project path.
        /// </param>
        /// <returns>
        /// The <see cref="AndroidBuildSystem"/>.
        /// </returns>
        public static AndroidBuildSystem GetProjectType(string projectPath) {
            string buildGradlePath = PathUtilities.Combine(projectPath, "build.gradle");

            bool isGradleProject = File.Exists(buildGradlePath);
            if (isGradleProject)
                return AndroidBuildSystem.Gradle;

            string projectPropertiesPath = PathUtilities.Combine(projectPath, "project.properties");
            string androidManifestPath = PathUtilities.Combine(projectPath, "AndroidManifest.xml");
            string libsPath = PathUtilities.Combine(projectPath, "libs");
            string assetsPath = PathUtilities.Combine(projectPath, "assets");

            bool isAdtProject =
                File.Exists(projectPropertiesPath) &&
                File.Exists(androidManifestPath) &&
                Directory.Exists(assetsPath) &&
                Directory.Exists(libsPath);

            if (isAdtProject)
                return AndroidBuildSystem.Adt;

            return AndroidBuildSystem.NotDetected;
        }

        /// <summary>
        /// Analyzes the Android Studio project directory and extracts data regarding
        /// important files and directories in the project.
        /// </summary>
        /// <param name="projectPath">
        /// Project path.
        /// </param>
        /// <returns>
        /// <see cref="ProjectData"/> of <paramref name="projectPath"/>.
        /// </returns>
        private static ProjectData ExtractProjectDataFromGradleProject(string projectPath) {
            // Unity 5.5 creates Gradle projects with a single module, located at the root
            string[] moduleNames =  { "" };

            string gradleSettingsPath = PathUtilities.Combine(projectPath, "settings.gradle");
            if (File.Exists(gradleSettingsPath)) {
                string gradleSettingsContents = File.ReadAllText(gradleSettingsPath);
                moduleNames =
                    Regex
                        .Matches(gradleSettingsContents, @"':(.+?)'", RegexOptions.CultureInvariant)
                        .Cast<Match>()
                        .Select(match => match.Groups[1].Value)
                        .Distinct()
                        .Concat(moduleNames)
                        .ToArray();
            }

            foreach (string moduleName in moduleNames) {
                string modulePath = PathUtilities.Combine(projectPath, moduleName);
                string moduleUnityClassesJarPath = PathUtilities.Combine(modulePath, "libs", "unity-classes.jar");
                string moduleSrcMainPath = PathUtilities.Combine(modulePath, "src", "main");
                string assetsPath = PathUtilities.Combine(moduleSrcMainPath, "assets");
                string libsPath = PathUtilities.Combine(moduleSrcMainPath, "jniLibs");
                string androidManifestPath = PathUtilities.Combine(moduleSrcMainPath, "AndroidManifest.xml");
                bool isMaybeUnityModule =
                    Directory.Exists(assetsPath) &&
                    Directory.Exists(libsPath) &&
                    File.Exists(androidManifestPath);

                if (!isMaybeUnityModule)
                    continue;

                ProjectData projectData =
                    GenerateProjectData(
                        androidManifestPath,
                        moduleUnityClassesJarPath,
                        assetsPath,
                        libsPath
                    );

                return projectData;
            }

            throw new Exception("No Unity module found in Gradle project.");
        }

        /// <summary>
        /// Analyzes the ADT project directory and extracts data regarding
        /// important files and directories in the project.
        /// </summary>
        /// <param name="projectPath">
        /// Project path.
        /// </param>
        /// <returns>
        /// <see cref="ProjectData"/> of <paramref name="projectPath"/>.
        /// </returns>
        private static ProjectData ExtractProjectDataFromAdtProject(string projectPath) {
            string projectPropertiesPath = PathUtilities.Combine(projectPath, "project.properties");
            string androidManifestPath = PathUtilities.Combine(projectPath, "AndroidManifest.xml");
            string libsPath = PathUtilities.Combine(projectPath, "libs");
            string assetsPath = PathUtilities.Combine(projectPath, "assets");

            bool isValid =
                File.Exists(projectPropertiesPath) &&
                File.Exists(androidManifestPath) &&
                Directory.Exists(assetsPath) &&
                Directory.Exists(libsPath);

            if (!isValid)
                throw new InvalidDataException("ADT project doesn't looks like an ADT project.");

            string unityClassesJarPath = PathUtilities.Combine(libsPath, "unity-classes.jar");
            ProjectData projectData =
                GenerateProjectData(
                    androidManifestPath,
                    unityClassesJarPath,
                    assetsPath,
                    libsPath
                );

            return projectData;
        }

        private static ProjectData GenerateProjectData(string androidManifestPath, string unityClassesJarPath, string unityAssetsPath, string unityLibsPath) {
            string unityAssetsBinPath = Path.Combine(unityAssetsPath, "bin");
            List<ProjectData.ArchitectureLibraryInfo> libraryInfos = new List<ProjectData.ArchitectureLibraryInfo>();
            foreach (string architectureName in kUnityAndroidArchitectures) {
                string architecturePath = Path.Combine(unityLibsPath, architectureName);
                if (!Directory.Exists(architecturePath))
                    continue;

                List<ProjectData.ArchitectureLibraryInfo.LibraryInfo> foundLibraries = new List<ProjectData.ArchitectureLibraryInfo.LibraryInfo>();
                foreach (string libraryName in kUnityAndroidLibraries) {
                    string libraryPath = Path.Combine(architecturePath, libraryName);
                    ProjectData.ArchitectureLibraryInfo.LibraryInfo libraryInfo =
                        new ProjectData.ArchitectureLibraryInfo.LibraryInfo(libraryPath, libraryName, File.Exists(libraryPath));
                    foundLibraries.Add(libraryInfo);
                }

                ProjectData.ArchitectureLibraryInfo architectureLibraryInfo =
                    new ProjectData.ArchitectureLibraryInfo(architectureName, architecturePath, foundLibraries.ToArray());
                libraryInfos.Add(architectureLibraryInfo);
            }

            ProjectData projectData =
                new ProjectData(
                    androidManifestPath,
                    unityClassesJarPath,
                    unityAssetsPath,
                    unityLibsPath,
                    unityAssetsBinPath,
                    libraryInfos.ToArray()
                );

            return projectData;
        }

        public class ProjectData {
            public string AndroidManifestPath { get; private set; }
            public string UnityClassesJarPath { get; private set; }
            public string UnityAssetsPath { get; private set; }
            public string UnityAssetsBinPath { get; private set; }
            public string UnityLibsPath { get; private set; }
            public ArchitectureLibraryInfo[] ArchitectureLibraryInfos { get; private set; }

            public ProjectData(
                string androidManifestPath,
                string unityClassesJarPath,
                string unityAssetsPath,
                string unityLibsPath,
                string unityAssetsBinPath,
                ArchitectureLibraryInfo[] architectureLibraryInfos
                ) {
                AndroidManifestPath = androidManifestPath;
                UnityClassesJarPath = unityClassesJarPath;
                UnityAssetsPath = unityAssetsPath;
                UnityLibsPath = unityLibsPath;
                UnityAssetsBinPath = unityAssetsBinPath;
                ArchitectureLibraryInfos = architectureLibraryInfos;
            }

            public class ArchitectureLibraryInfo {
                public string ArchitectureName { get; private set; }
                public string ArchitecturePath { get; private set; }
                public LibraryInfo[] LibrariesInfos { get; private set; }

                public ArchitectureLibraryInfo(string architectureName, string architecturePath, LibraryInfo[] librariesInfos) {
                    ArchitectureName = architectureName;
                    ArchitecturePath = architecturePath;
                    LibrariesInfos = librariesInfos;
                }

                public class LibraryInfo {
                    public string Path { get; private set; }
                    public string Name { get; private set; }
                    public bool FileExists { get; private set; }

                    public LibraryInfo(string path, string name, bool fileExists) {
                        Path = path;
                        Name = name;
                        FileExists = fileExists;
                    }
                }
            }
        }
    }
}

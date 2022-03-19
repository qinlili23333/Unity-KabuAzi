using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DotLiquid;
using UnityEditor;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    /// <summary>
    /// Builds a Live Wallpaper project.
    /// </summary>
    public class LiveWallpaperProjectBuilder : LiveWallpaperProjectManipulatorBase {
        private const string kLiveWallpaperServiceClassName = "LiveWallpaperService";
        private const string kLiveWallpaperServiceClassFileName = kLiveWallpaperServiceClassName + ".java";
        private const string kWallpaperResXmlName = "wallpaper";
        private const string kWallpaperResXmlFileName = kWallpaperResXmlName + ".xml";
        private const string kPreferencesResXmlName = "preferences";
        private const string kPreferencesResXmlFileName = kPreferencesResXmlName + ".xml";

        private const string kSettingsActivityClassName = "SettingsActivity";

        private static readonly string[] kUnityActivityNames =
        {
            "UnityPlayerActivity.java",
            "UnityPlayerNativeActivity.java",
            "UnityPlayerProxyActivity.java"
        };

        private LiveWallpaperBuildOptionsFlags _liveWallpaperBuildOptions;
        private LiveWallpaperSettingsActivityGenerationMode _settingsActivityGenerationMode;
        private LiveWallpaperLauncherShortcutGenerationMode _launcherShortcutGenerationMode;

        private string _settingsActivityName = kSettingsActivityClassName;
        private string _unityProjectPath;
        private XmlDocument _androidManifestXmlDocument;
        private AndroidBuildSystem _buildSystem = AndroidBuildSystem.Adt;

        /// <summary>
        /// Android build system.
        /// </summary>
        public AndroidBuildSystem BuildSystem {
            get { return _buildSystem; }
            set { _buildSystem = value; }
        }

        /// <summary>
        /// Live Wallpaper build options flags.
        /// </summary>
        public LiveWallpaperBuildOptionsFlags LiveWallpaperBuildOptions {
            get { return _liveWallpaperBuildOptions; }
            set { _liveWallpaperBuildOptions = value; }
        }

        /// <summary>
        /// Settings Activity generation mode.
        /// </summary>
        public LiveWallpaperSettingsActivityGenerationMode SettingsActivityGenerationMode {
            get { return _settingsActivityGenerationMode; }
            set { _settingsActivityGenerationMode = value; }
        }

        /// <summary>
        /// Launcher Shortcut generation mode.
        /// </summary>
        public LiveWallpaperLauncherShortcutGenerationMode LauncherShortcutGenerationMode {
            get { return _launcherShortcutGenerationMode; }
            set { _launcherShortcutGenerationMode = value; }
        }

        /// <summary>
        /// Settings Activity name. Default is "SettingsActivity".
        /// </summary>
        public string SettingsActivityName {
            get { return _settingsActivityName; }
            set { _settingsActivityName = value; }
        }

        /// <summary>
        /// Unity project path. May not match <see cref="UnityProjectPath"/> when project has multiple modules.
        /// </summary>
        public string UnityProjectPath {
            get { return _unityProjectPath; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveWallpaperProjectBuilder"/> class.
        /// </summary>
        /// <param name="projectRootPath">Destination project root path.</param>
        public LiveWallpaperProjectBuilder(string projectRootPath)
            : base(projectRootPath) {
        }

        /// <summary>
        /// Builds and post-processes the Android project.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="FileNotFoundException">AndroidManifest.xml not found in Android project</exception>
        public void BuildProject() {
            if (_buildScenes == null) {
                _buildScenes = EditorUtilities.GetCurrentBuildScenes();
            }

            _unityProjectPath = _projectRootPath;
            bool isSuccess = BuildInitialProject();
            if (!isSuccess)
                throw new OperationCanceledException("User has canceled the operation.");

            LoadAndroidManifest();
            PostProcessProject();

            // Avoid creating a sub-directory, if possible
            if (Directory.GetFiles(_projectRootPath).Length == 0 && Directory.GetDirectories(_projectRootPath).Length == 1) {
                IOUtilities.MoveDirectoryContents(_unityProjectPath, _projectRootPath);
                Directory.Delete(_unityProjectPath, false);
                _unityProjectPath = _projectRootPath;
            }
        }

        /// <summary>
        /// Builds the initial project.
        /// </summary>
        /// <returns>true on success, false on error.</returns>
        private bool BuildInitialProject() {
            bool currentPlayerSettingsAndroidIsGame = PlayerSettings.Android.androidIsGame;
            bool currentPlayerSettingsAndroidTVCompatibility = PlayerSettings.Android.androidTVCompatibility;
            AndroidPreferredInstallLocation currentPlayerSetttingsAndroidPreferredInstallLocation = PlayerSettings.Android.preferredInstallLocation;

            bool isSuccess;
            try {
                PlayerSettings.Android.androidIsGame = false;
                PlayerSettings.Android.androidTVCompatibility = false;
                PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.ForceInternal;

                // Disable VR support for the first time
                if (!ProjectSettingsContainer.Instance.IsVirtualRealitySupportDisabledOnce && PlayerSettings.virtualRealitySupported) {
                    PlayerSettings.virtualRealitySupported = false;
                    ProjectSettingsContainer.Instance.IsVirtualRealitySupportDisabledOnce = true;
                }

                BuildOptions buildOptions = _buildOptions;
                buildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                isSuccess = BuildPipelineUtilities.BuildAndroidProject(
                    _unityProjectPath,
                    _buildSystem == AndroidBuildSystem.Adt ?
                        EditorUserBuildSettingsWrapper.AndroidBuildSystem.ADT :
                        EditorUserBuildSettingsWrapper.AndroidBuildSystem.Gradle,
                    _buildScenes,
                    buildOptions,
                    PlayerSettings.applicationIdentifier,
                    out _unityProjectPath
                    );
            } finally {
                PlayerSettings.Android.androidIsGame = currentPlayerSettingsAndroidIsGame;
                PlayerSettings.Android.androidTVCompatibility = currentPlayerSettingsAndroidTVCompatibility;
                PlayerSettings.Android.preferredInstallLocation = currentPlayerSetttingsAndroidPreferredInstallLocation;
            }

            if (!isSuccess)
                return false;

            InvokeStagingProjectBuildFinished();

            return true;
        }

        /// <summary>
        /// Post-processed the Android project to actually make it a Live Wallpaper project.
        /// </summary>
        private void PostProcessProject() {
            if (_buildSystem == AndroidBuildSystem.Gradle) {
                PostProcessBuildGradleFile();
            }

            // Delete LP_uLiveWallpaper library project directory as it is not imported correctly by Android Studio
            string uLiveWallpaperLibraryProjectPath = Path.Combine(_projectRootPath, Constants.kULiveWallpaperLibraryName);
            IOUtilities.DeleteDirectoryIfExists(uLiveWallpaperLibraryProjectPath, true);

            if (_buildSystem == AndroidBuildSystem.Adt) {
                // Delete libs/LP_uLiveWallpaper.aar
                string uLiveWallpaperLibraryPath =
                    PathUtilities.Combine(
                        _unityProjectPath,
                        "libs",
                        Constants.kULiveWallpaperLibraryName + ".aar"
                    );
                IOUtilities.DeleteFileIfExists(uLiveWallpaperLibraryPath);
            }

            // Remove Unity Activities
            RemoveUnityActivitiesFromManifest();
            RemoveUnityActivitiesSource(_unityProjectPath);

            // Remove 'android:debuggable="false"' added by Unity
            RemoveApplicationDebuggableAttribute();

            RemoveApplicationUnityTheme();

            // Removes Activities that come from uLiveWallpaper library
            RemoveLibraryAndroidActivities(_androidManifestXmlDocument);

            // Inject live wallpaper stuff
            InjectLiveWallpaper();
            switch (_settingsActivityGenerationMode) {
                case LiveWallpaperSettingsActivityGenerationMode.Basic:
                    InjectBasicSettingsActivity();
                    break;
                case LiveWallpaperSettingsActivityGenerationMode.FakeInvisible:
                    InjectFakeInvisibleSettingsActivity();
                    break;
            }
            // Add live wallpaper compatible Unity activity
            if ((_liveWallpaperBuildOptions & LiveWallpaperBuildOptionsFlags.CreateUnityActivity) != 0) {
                InjectLiveWallpaperCompatibleUnityActivity();
            }

            // Inject launcher shortcut
            if (LauncherShortcutGenerationMode == LiveWallpaperLauncherShortcutGenerationMode.OpenPreview) {
                InjectLauncherShortcut();
            }

            // Misc stuff
            InjectStringsToStringsXml();
            if (_buildSystem == AndroidBuildSystem.Adt) {
                FixAdtProjectProperties();
            }
            UpdateUnityVersionMetaDataTag(_androidManifestXmlDocument);
            SaveAndUnloadAndroidManifest();
        }

        private void SaveAndUnloadAndroidManifest() {
            _androidManifestXmlDocument.SaveAsUtf8(GetAndroidManifestPath(), true);
            _androidManifestXmlDocument = null;
        }

        private void LoadAndroidManifest() {
            string manifestPath = GetAndroidManifestPath();
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("AndroidManifest.xml not found in Android project", kAndroidManifestXmlName);

            _androidManifestXmlDocument = new XmlDocument();
            _androidManifestXmlDocument.PreserveWhitespace = true;
            _androidManifestXmlDocument.Load(manifestPath);
        }

        private void RemoveApplicationDebuggableAttribute() {
            XmlElement applicationElement = GetFirstChildElementWithName(_androidManifestXmlDocument.DocumentElement, "application");
            XmlAttribute debuggableAttribute = applicationElement.Attributes["android:debuggable"];
            if (debuggableAttribute == null)
                return;

            applicationElement.Attributes.Remove(debuggableAttribute);
        }

        private void RemoveApplicationUnityTheme() {
            XmlElement applicationElement = GetFirstChildElementWithName(_androidManifestXmlDocument.DocumentElement, "application");

            XmlAttribute themeAttribute = applicationElement.Attributes["android:theme"];
            if (themeAttribute == null)
                return;

            if (themeAttribute.Value != "@style/UnityThemeSelector" && themeAttribute.Value != "@android:style/Theme.NoTitleBar")
                return;

            applicationElement.Attributes.Remove(themeAttribute);
        }

        #region Launcher Shortcut generation

        private void InjectLauncherShortcut() {
            AddActivityJava("Templates/StartWallpaperPreviewActivity.java.template", "StartWallpaperPreviewActivity");
            AddActivityElementToManifest("Templates/manifestStartWallpaperPreviewActivity.template");
        }

        #endregion

        #region Live Wallpaper Compatible Unity Activity generation

        private void InjectLiveWallpaperCompatibleUnityActivity() {
            AddActivityJava("Templates/UnityPlayerActivity.java.template", "UnityPlayerActivity");
            AddActivityElementToManifest("Templates/manifestUnityPlayerActivity.template");
        }

        #endregion

        #region Settings Activity generation

        private string GetFullSettingsActivityClassName() {
            return PlayerSettings.applicationIdentifier + "." + _settingsActivityName;
        }

        private void InjectFakeInvisibleSettingsActivity() {
            AddSettingsActivityJava("Templates/FakeInvisibleSettingsActivity.java.template");
            AddSettingsActivityElementToManifest();
        }

        private void InjectBasicSettingsActivity() {
            AddSettingsActivityJava("Templates/SettingsActivity.java.template");
            AddPreferencesXml();

            AddSettingsActivityElementToManifest();
        }

        /// <summary>
        /// Adds Settings Activity element to &lt;application&gt; element in AndroidManifest.xml.
        /// </summary>
        private void AddActivityElementToManifest(string templatePath) {
            Dictionary<string, object> templateVariables = new Dictionary<string, object>();
            templateVariables.AddRange(GetGenericVariables());

            string manifestSettingsActivity = RenderTemplate(templatePath, templateVariables);
            InsertXmlToDocument(_androidManifestXmlDocument, manifestSettingsActivity, "application");
        }

        /// <summary>
        /// Adds Settings Activity element to &lt;application&gt; element in AndroidManifest.xml.
        /// </summary>
        private void AddSettingsActivityElementToManifest() {
            AddActivityElementToManifest("Templates/manifestSettingsActivity.template");
        }

        /// <summary>
        /// Adds the Settings Activity .java source file to the project.
        /// </summary>
        private void AddActivityJava(string templatePath, string className) {
            Dictionary<string, object> templateVariables = GetGenericVariables();

            string javaSourcePath = GetJavaSourcePathInProject();

            string settingsActivityJava = RenderTemplate(templatePath, templateVariables);
            string settingsActivityJavaPath = Path.Combine(javaSourcePath, className + ".java");
            IOUtilities.WriteAllTextUtf8NoBom(settingsActivityJavaPath, settingsActivityJava);
        }

        /// <summary>
        /// Adds the Settings Activity .java source file to the project.
        /// </summary>
        private void AddSettingsActivityJava(string templatePath) {
            AddActivityJava(templatePath, _settingsActivityName);
        }

        /// <summary>
        /// Adds the Preferences definition to "res/xml".
        /// </summary>
        private void AddPreferencesXml() {
            string preferencesXml = RenderTemplate("Templates/" + kPreferencesResXmlFileName + ".template", new Dictionary<string, object>());
            string resXmlPath = PathUtilities.Combine(GetResPath(), "xml");
            string preferencesXmlPath = Path.Combine(resXmlPath, kPreferencesResXmlFileName);
            Directory.CreateDirectory(resXmlPath);
            IOUtilities.WriteAllTextUtf8NoBom(preferencesXmlPath, preferencesXml);
        }

        /// <summary>
        /// Adds strings to "/res/values/strings.xml".
        /// </summary>
        private void InjectStringsToStringsXml() {
            Dictionary<string, object> templateVariables = GetGenericVariables();
            templateVariables.AddRange(GetTemplateVariablesPreferences());

            string stringsXml = RenderTemplate("Templates/strings.template", templateVariables);
            string resValuesPath = PathUtilities.Combine(GetResPath(), "values");
            string stringsXmlPath = Path.Combine(resValuesPath, "strings.xml");

            XmlDocument preferencesStringsDocument = new XmlDocument();
            preferencesStringsDocument.PreserveWhitespace = true;
            preferencesStringsDocument.LoadXml(stringsXml);

            XmlDocument stringsDocument = new XmlDocument();
            stringsDocument.PreserveWhitespace = true;
            stringsDocument.Load(stringsXmlPath);

            XmlElement stringsRootElement = stringsDocument.DocumentElement;
            foreach (XmlNode childNode in preferencesStringsDocument.DocumentElement.ChildNodes) {
                XmlNode childNodeImported = stringsDocument.ImportNode(childNode, true);
                stringsRootElement.AppendChild(childNodeImported);
            }

            stringsDocument.SaveAsUtf8(stringsXmlPath);
        }

        private Dictionary<string, object> GetTemplateVariablesPreferences() {
            Dictionary<string, object> templateVariables = new Dictionary<string, object>();

            string[] qualitySettings = QualitySettings.names;
            List<string> qualitySettingsTitles = new List<string>();
            List<string> qualitySettingsValues = new List<string>();
            for (int i = 0; i < qualitySettings.Length; i++) {
                qualitySettingsTitles.Add(qualitySettings[i]);
                qualitySettingsValues.Add(i.ToString());
            }

            templateVariables.Add("wallpaperSettingsGraphicsQualityTitles", qualitySettingsTitles);
            templateVariables.Add("wallpaperSettingsGraphicsQualityValues", qualitySettingsValues);

            return templateVariables;
        }

        #endregion

        #region Transforming project to Live Wallpaper

        /// <summary>
        /// Injects necessary files and info into project to make it a Live Wallpaper project.
        /// </summary>
        private void InjectLiveWallpaper() {
            AddWallpaperServiceJava();
            AddServiceNodeToManifest();
            AddWallpaperXml();
        }

        /// <summary>
        /// Adds the wallpaper definition XML to "res/xml".
        /// </summary>
        private void AddWallpaperXml() {
            Dictionary<string, object> templateVariables = GetGenericVariables();
            string wallpaperXml = RenderTemplate("Templates/" + kWallpaperResXmlFileName + ".template", templateVariables);

            string resXmlPath = PathUtilities.Combine(GetResPath(), "xml");
            string wallpaperXmlPath = Path.Combine(resXmlPath, kWallpaperResXmlFileName);
            Directory.CreateDirectory(resXmlPath);
            IOUtilities.WriteAllTextUtf8NoBom(wallpaperXmlPath, wallpaperXml);
        }

        private string GetGradleProjectSrcRoot() {
            return PathUtilities.Combine(_unityProjectPath, "src", "main");
        }

        private string GetAndroidManifestPath() {
            switch (_buildSystem) {
                case AndroidBuildSystem.Gradle:
                    return PathUtilities.Combine(GetGradleProjectSrcRoot(), kAndroidManifestXmlName);;
                case AndroidBuildSystem.Adt:
                    return PathUtilities.Combine(_unityProjectPath, kAndroidManifestXmlName);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetResPath() {
            switch (_buildSystem) {
                case AndroidBuildSystem.Gradle:
                    return PathUtilities.Combine(GetGradleProjectSrcRoot(), "res");
                case AndroidBuildSystem.Adt:
                    return PathUtilities.Combine(_unityProjectPath, "res");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Adds the wallpaper service node to &lt;application&gt; node in AndroidManifest.xml.
        /// </summary>
        private void AddServiceNodeToManifest() {
            Dictionary<string, object> templateVariables = GetGenericVariables();
            string manifestService = RenderTemplate("Templates/manifestService.template", templateVariables);
            InsertXmlToDocument(_androidManifestXmlDocument, manifestService, "application");
        }

        /// <summary>
        /// Adds the wallpaper service .java source file to the project.
        /// </summary>
        private void AddWallpaperServiceJava() {
            string javaSourcePath = GetJavaSourcePathInProject();

            Dictionary<string, object> templateVariables = GetGenericVariables();
            string wallpaperService = RenderTemplate("Templates/" + kLiveWallpaperServiceClassFileName + ".template", templateVariables);

            string wallpaperServiceJavaPath = Path.Combine(javaSourcePath, kLiveWallpaperServiceClassFileName);
            IOUtilities.WriteAllTextUtf8NoBom(wallpaperServiceJavaPath, wallpaperService);
        }

        #endregion

        /// <summary>
        /// Modifies the ADT project.properties file.
        /// </summary>
        private void FixAdtProjectProperties() {
            const string projectPropertiesFileName = "project.properties";
            string projectPropertiesPath = Path.Combine(_unityProjectPath, projectPropertiesFileName);
            string properties = File.ReadAllText(projectPropertiesPath);

            // Remove uLiveWallpaperLibrary references
            properties = Regex.Replace(properties, @"^android.library.reference.(\d+)=../" + Constants.kULiveWallpaperLibraryName, "", RegexOptions.Multiline);
            properties = Regex.Replace(properties, @"\n{2,}", "\n");

            IOUtilities.WriteAllTextUtf8NoBom(projectPropertiesPath, properties);
        }

        /// <summary>
        /// Removes references to Unity Activities from AndroidManifest.xml.
        /// </summary>
        private void RemoveUnityActivitiesFromManifest() {
            IEnumerable<XmlElement> activityNodes = GetAndroidManifestActivityNodes(_androidManifestXmlDocument);

            // Filter out Unity activities
            IEnumerable<XmlElement> unityActivityNodes =
                activityNodes
                .Where(element => {
                    XmlElement metaDataElement = element["meta-data"];
                    if (metaDataElement == null)
                        return false;

                    XmlAttribute nameAttribute = metaDataElement.Attributes["android:name"];
                    if (nameAttribute == null)
                        return false;

                    return nameAttribute.Value == "unityplayer.UnityActivity";
                });

            // Remove Unity activities
            foreach (XmlElement childNode in unityActivityNodes.ToArray()) {
                childNode.ParentNode.RemoveChild(childNode);
            }
        }

        private Dictionary<string, object> GetGenericVariables() {
            string settingsActivityType;
            switch (SettingsActivityGenerationMode) {
                case LiveWallpaperSettingsActivityGenerationMode.None:
                    settingsActivityType = "none";
                    break;
                case LiveWallpaperSettingsActivityGenerationMode.Basic:
                    settingsActivityType = "basic";
                    break;
                case LiveWallpaperSettingsActivityGenerationMode.FakeInvisible:
                    settingsActivityType = "invisible";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string launcherShortcutType;
            switch (LauncherShortcutGenerationMode) {
                case LiveWallpaperLauncherShortcutGenerationMode.None:
                    launcherShortcutType = "none";
                    break;
                case LiveWallpaperLauncherShortcutGenerationMode.OpenSettings:
                    launcherShortcutType = "openSettings";
                    break;
                case LiveWallpaperLauncherShortcutGenerationMode.OpenPreview:
                    launcherShortcutType = "openPreview";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Dictionary<string, object>
            {
                { "productName", PlayerSettings.productName },
                { "companyName", PlayerSettings.companyName },
                { "bundleIdentifier", PlayerSettings.applicationIdentifier },
                { "wallpaperResXml", kWallpaperResXmlName },
                { "wallpaperServiceClassName", kLiveWallpaperServiceClassName },
                { "wallpaperSettingsActivityType", settingsActivityType },
                { "wallpaperLauncherShortcutType", launcherShortcutType },
                { "wallpaperSettingsActivityFullName", GetFullSettingsActivityClassName() },
                { "wallpaperSettingsActivityName", _settingsActivityName },
            };
        }

        /// <summary>
        /// Applies some fixes to the Unity-generated buidl.gradle file.
        /// </summary>
        private void PostProcessBuildGradleFile() {
            // Remove Unity comment from build.gradle
            string buildGradlePath = Path.Combine(_unityProjectPath, "build.gradle");
            string buildGradle = File.ReadAllText(buildGradlePath);
            buildGradle =
                buildGradle
                    .Replace("// GENERATED BY UNITY. REMOVE THIS COMMENT TO PREVENT OVERWRITING WHEN EXPORTING AGAIN\n", "")
                    .Replace(
@"allprojects {
   repositories {
      flatDir {
        dirs 'libs'
      }
   }
}".Replace("\r\n", "\n"),
@"allprojects {
   repositories {
      jcenter()

      flatDir {
        dirs 'libs'
      }
   }
}".Replace("\r\n", "\n"));

            IOUtilities.AttemptPotentiallyFailingOperation(() => File.WriteAllText(buildGradlePath, buildGradle));
        }

        /// <summary>
        /// Removes Java source files of Unity Activities.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        private void RemoveUnityActivitiesSource(string projectPath) {
            // Remove Unity activites Java source
            string javaSourcePath = GetJavaSourcePathInProject();
            foreach (string unityActivityName in kUnityActivityNames) {
                string unityActivityFilePath = Path.Combine(javaSourcePath, unityActivityName);
                IOUtilities.DeleteFileIfExists(unityActivityFilePath);
            }
        }

        /// <summary>
        /// Gets path to the Java source files location for current package.
        /// </summary>
        private string GetJavaSourcePathInProject() {
            string bundleIdentifier = PlayerSettings.applicationIdentifier;
            switch (_buildSystem) {
                case AndroidBuildSystem.Gradle:
                    return PathUtilities.Combine(GetGradleProjectSrcRoot(), "java", bundleIdentifier.Replace('.', Path.DirectorySeparatorChar));
                case AndroidBuildSystem.Adt:
                    return PathUtilities.Combine(_unityProjectPath, "src", bundleIdentifier.Replace('.', Path.DirectorySeparatorChar));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Inserts raw XML fragment to <see cref="XmlDocument"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="xmlFragment">XML fragment to insert.</param>
        /// <param name="parentNodeName">Name of the node to which the <paramref name="xmlFragment"/> will be inserted.</param>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        private static void InsertXmlToDocument(XmlDocument document, string xmlFragment, string parentNodeName) {
            // Get manifest <application> node
            XmlElement parentNode = GetFirstChildElementWithName(document.DocumentElement, parentNodeName);

            if (parentNode == null)
                throw new InvalidDataException(string.Format("No <{0}> node found in XmlDocument.", parentNodeName));

            XmlDocument templateDocument = new XmlDocument();
            templateDocument.PreserveWhitespace = true;

            // Read, ignoring namespaces
            using (StringReader stringReader = new StringReader(xmlFragment)) {
                using (XmlTextReader xmlTextReader = new XmlTextReader(stringReader) { Namespaces = false }) {
                    templateDocument.Load(xmlTextReader);
                }
            }
            XmlElement insertedNode = templateDocument.DocumentElement;
            XmlNode insertedNodeImported = document.ImportNode(insertedNode, true);
            parentNode.AppendChild(insertedNodeImported);
        }

        private static string GetRawTemplate(string templatePath) {
            TextAsset wallpaperXmlTemplateAsset = EditorResourcesManager.Get<TextAsset>(templatePath + ".txt");
            if (wallpaperXmlTemplateAsset == null)
                throw new FileNotFoundException(templatePath);

            string wallpaperXml = wallpaperXmlTemplateAsset.text;
            return wallpaperXml;
        }

        private static Template GetTemplate(string templatePath) {
            string rawTemplate = GetRawTemplate(templatePath);
            Template template = Template.Parse(rawTemplate);
            return template;
        }

        private static string RenderTemplate(string templatePath, Dictionary<string, object> dictionary) {
            Template template = GetTemplate(templatePath);
            string rendered = template.Render(new RenderParameters
            {
                LocalVariables = Hash.FromDictionary(dictionary),
                RethrowErrors = true
            });

            return rendered;
        }

        /// <summary>
        /// Live Wallpaper project build options. Options can be combined together.
        /// </summary>
        [Flags]
        public enum LiveWallpaperBuildOptionsFlags {
            None = 0,
            CreateUnityActivity = 1 << 1
        }

        /// <summary>
        /// Settings Activity generation mode.
        /// </summary>
        public enum LiveWallpaperSettingsActivityGenerationMode {
            None,
            Basic,
            FakeInvisible
        }

        /// <summary>
        /// Launcher shortcut generation mode.
        /// </summary>
        public enum LiveWallpaperLauncherShortcutGenerationMode {
            None,
            OpenSettings,
            OpenPreview
        }
    }
}
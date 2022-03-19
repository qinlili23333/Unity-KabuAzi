using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Contains <see cref="LiveWallpaperBuildWindow"/> state and current project settings.
    /// </summary>
    [Serializable]
    internal class ProjectSettingsContainer : ScriptableObject {
        [NonSerialized]
        private static ProjectSettingsContainer _instance;

        [SerializeField]
        [HideInInspector]
        private MainWindowState _mainWindowCurrentTab;

        [SerializeField]
        [HideInInspector]
        private bool _mainWindowShowQuickGuide;

        [SerializeField]
        [HideInInspector]
        private string _projectCreatePath;

        [SerializeField]
        [HideInInspector]
        private string _projectUpdatePath;

        [SerializeField]
        [HideInInspector]
        private LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags _liveWallpaperBuildOptions
            = LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags.None;

        [SerializeField]
        [HideInInspector]
        private LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode _liveWallpaperSettingsActivityGenerationMode
            = LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode.Basic;

        [SerializeField]
        [HideInInspector]
        private LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode _liveWallpaperLauncherShortcutGenerationMode
            = LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode.OpenPreview;

        [SerializeField]
        [HideInInspector]
        private bool _isVirtualRealitySupportDisabledOnce;

        public static ProjectSettingsContainer Instance {
            get {
                if (_instance == null) {
                    _instance =
                        UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(SerializedPath)
                        .OfType<ProjectSettingsContainer>()
                        .FirstOrDefault();

                    if (_instance != null)
                        return _instance;

                    _instance = CreateInstance<ProjectSettingsContainer>();
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                    _instance.SaveSerialized();
                }

                return _instance;
            }
        }

        private static string SerializedPath {
            get {
                const string assetName = "uLiveWallpaperProjectSettings.asset";
                string path = PathUtilities.Combine("Library", assetName);
                return path;
            }
        }

        public string ProjectCreatePath {
            get { return _projectCreatePath; }
            set { _projectCreatePath = value; }
        }

        public string ProjectUpdatePath {
            get { return _projectUpdatePath; }
            set { _projectUpdatePath = value; }
        }

        public LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags LiveWallpaperBuildOptions {
            get { return _liveWallpaperBuildOptions; }
            set { _liveWallpaperBuildOptions = value; }
        }

        public LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode LiveWallpaperSettingsActivityGenerationMode {
            get { return _liveWallpaperSettingsActivityGenerationMode; }
            set { _liveWallpaperSettingsActivityGenerationMode = value; }
        }

        public LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode LiveWallpaperLauncherShortcutGenerationMode {
            get { return _liveWallpaperLauncherShortcutGenerationMode; }
            set { _liveWallpaperLauncherShortcutGenerationMode = value; }
        }

        internal bool IsVirtualRealitySupportDisabledOnce {
            get { return _isVirtualRealitySupportDisabledOnce; }
            set { _isVirtualRealitySupportDisabledOnce = value; }
        }

        internal MainWindowState MainWindowCurrentTab {
            get { return _mainWindowCurrentTab; }
            set { _mainWindowCurrentTab = value; }
        }

        internal bool MainWindowShowQuickGuide {
            get { return _mainWindowShowQuickGuide; }
            set { _mainWindowShowQuickGuide = value; }
        }

        private ProjectSettingsContainer() {
            if (_instance != null)
                throw new InvalidOperationException("ProjectSettingsContainer._instance != null");

            _instance = this;
        }

        private void OnDisable() {
            SaveSerialized();
        }

        private void SaveSerialized() {
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _instance }, SerializedPath, true);
        }

        internal enum MainWindowState {
            CreateProject,
            UpdateProject,
            About
        }
    }
}
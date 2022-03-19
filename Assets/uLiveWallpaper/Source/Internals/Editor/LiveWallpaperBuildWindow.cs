using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    /// <summary>
    /// Main uLiveWallpaper window.
    /// </summary>
    public sealed class LiveWallpaperBuildWindow : EditorWindow {
        private const int kContentSidePadding = 22;
        private ProjectSettingsContainer _projectSettings;
        private ProjectSettingsContainer.MainWindowState _lastWindowMode;
        private Vector2 _scrollPosition;
        private AnimBool _quickGuideVisible;

        private void OnEnable() {
            titleContent = new GUIContent("uLiveWallpaper");
            minSize = new Vector2(445f, 317f);

            _projectSettings = ProjectSettingsContainer.Instance;
            _lastWindowMode = _projectSettings.MainWindowCurrentTab;

            _quickGuideVisible = new AnimBool(_projectSettings.MainWindowShowQuickGuide);
            _quickGuideVisible.valueChanged.AddListener(Repaint);
        }

        private void OnDisable() {
            _quickGuideVisible.valueChanged.RemoveListener(Repaint);
        }

        private void OnGUI() {
            _projectSettings = ProjectSettingsContainer.Instance;
            DrawGUI();
        }

        private void DrawGUI() {
            EditorGUIStyles.Cache();
            Undo.RecordObject(_projectSettings, null);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            {
                GUIStyle offsetStyle = new GUIStyle();
                offsetStyle.padding = new RectOffset(kContentSidePadding, kContentSidePadding, 0, 0);
                EditorGUILayout.BeginVertical(offsetStyle);
                DrawContent();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (GUI.changed) {
                EditorUtility.SetDirty(_projectSettings);
            }
        }

        private void DrawContent() {
            DrawHeader();

            switch (_lastWindowMode) {
                case ProjectSettingsContainer.MainWindowState.CreateProject:
                    DrawCreateProject();
                    break;
                case ProjectSettingsContainer.MainWindowState.UpdateProject:
                    DrawUpdateProject();
                    break;
                case ProjectSettingsContainer.MainWindowState.About:
                    DrawAbout();
                    break;
            }

            // Draw copyrights
            GUILayout.Space(25f);
            GUILayout.Label(GUIContent.none, GUIStyle.none, GUILayout.MaxHeight(0f), GUILayout.ExpandWidth(true));

            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect copyrightRect = new Rect(0f, 0f, lastRect.width, lastRect.yMin);
            copyrightRect.width += kContentSidePadding * 2;
            DrawCopyright(copyrightRect);
        }

        private void DrawHeader() {
            EditorGUILayoutExtensions.BeginHorizontalCenter();
            GUILayout.Space(12f);
            GUILayout.Label(EditorResourcesManager.Get<Texture2D>("Textures/Logo.png"));
            EditorGUILayoutExtensions.EndHorizontalCenter();

            EditorGUILayoutExtensions.BeginHorizontalCenter();
            _projectSettings.MainWindowCurrentTab =
                (ProjectSettingsContainer.MainWindowState)
                GUILayout.Toolbar(
                    (int) _projectSettings.MainWindowCurrentTab,
                    new[] {
                        new GUIContent("Create Project"),
                        new GUIContent("Update Project"),
                        new GUIContent("About & Support")
                    },
                    EditorGUIStyles.LargeButton,
                    GUILayout.MinWidth(250f),
                    GUILayout.MaxWidth(500f)
                );
            if (_projectSettings.MainWindowCurrentTab != _lastWindowMode) {
                GUIExtensions.ResetFocus();
            }
            _lastWindowMode = _projectSettings.MainWindowCurrentTab;
            EditorGUILayoutExtensions.EndHorizontalCenter();

            GUILayout.Space(7f);
        }

        private void DrawUpdateProject() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawSettings(() => {
                string projectPath = _projectSettings.ProjectUpdatePath;
                DrawProjectBrowseField("Project Folder: ", "Updated project folder", ref projectPath);
                _projectSettings.ProjectUpdatePath = projectPath;
            });

            bool isError = true;
            LiveWallpaperBuildGuiUtility.ProjectUpdateState projectUpdateState =
                LiveWallpaperBuildGuiUtility.GetProjectUpdateState(_projectSettings.ProjectUpdatePath);
            switch (projectUpdateState) {
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DetectedAndroidStudioProject:
                    EditorGUILayoutExtensions.HelpBox(
                        "Detected <i>Android Studio</i> project, ready to update.",
                        MessageType.Info,
                        EditorGUIStyles.RichTextLabel);
                    isError = false;
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DetectedEclipseAdtProject:
                    EditorGUILayoutExtensions.HelpBox(
                        "Detected <i>Eclipse ADT</i> project. Please import the project to <i>Android Studio</i> first.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.NoProjectDetected:
                    EditorGUILayoutExtensions.HelpBox(
                        "No project detected at the given path. Please select a valid directory with <i>Android Studio</i> project.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.ProjectPathEmpty:
                    EditorGUILayoutExtensions.HelpBox(
                        "Please select a project to upgrade.",
                        MessageType.Info,
                        EditorGUIStyles.RichTextLabel);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.DirectoryNotExists:
                    EditorGUILayoutExtensions.HelpBox(
                        "Directory does not exists. Please select a valid directory with <i>Android Studio</i> project.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectUpdateState.ProjectPathIsNotDirectory:
                    EditorGUILayoutExtensions.HelpBox(
                        "Selected path is not a directory. Please select a valid empty directory.",
                        MessageType.Error,
                        GUI.skin.label);
                    break;
                default:
                    throw
                        new InvalidEnumArgumentException(
                            "projectUpdateState",
                            (int) projectUpdateState,
                            typeof(LiveWallpaperBuildGuiUtility.ProjectUpdateState));
            }

            GUILayout.Space(5f);

            EditorGUI.BeginDisabledGroup(isError);
            EditorGUILayoutExtensions.BeginHorizontalCenter();
            {
                GUIContent buttonGuiContent = new GUIContent(" Update Project", EditorResourcesManager.Get<Texture2D>("Textures/UI-UpdateProject.png"));
                if (GUILayout.Button(buttonGuiContent, EditorGUIStyles.LargeButton, GUILayout.MinWidth(200f), GUILayout.MaxWidth(250f))) {
                    EditorApplication.delayCall += UpdateLiveWallpaperProject;
                }
            }
            EditorGUILayoutExtensions.EndHorizontalCenter();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(7f);

            EditorGUILayout.EndVertical();
        }

        private void DrawCreateProject() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawSettings(() => {
                string projectPath = _projectSettings.ProjectCreatePath;
                DrawProjectBrowseField("Project Folder: ", "Destination project folder", ref projectPath);
                _projectSettings.ProjectCreatePath = projectPath;

                EditorGUIUtility.labelWidth = 160f;
                _projectSettings.LiveWallpaperSettingsActivityGenerationMode =
                    (LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode)
                    EditorGUILayoutExtensions.EnumPopup(
                        "Settings Activity",
                        _projectSettings.LiveWallpaperSettingsActivityGenerationMode,
                        new[]
                        {
                            "None",
                            "Basic (Quality Settings, Frames Per Second)",
                            "Invisible Self-Closing (Only sends the event, use to handle preferences from within Unity)"
                        }
                        );

                if (_projectSettings.LiveWallpaperLauncherShortcutGenerationMode == LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode.OpenSettings &&
                    _projectSettings.LiveWallpaperSettingsActivityGenerationMode != LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode.Basic
                    ) {
                    _projectSettings.LiveWallpaperLauncherShortcutGenerationMode = LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode.None;
                }

                _projectSettings.LiveWallpaperLauncherShortcutGenerationMode =
                    (LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode)
                    EditorGUILayoutExtensions.EnumPopup(
                        "Home Screen Shortcut",
                        _projectSettings.LiveWallpaperLauncherShortcutGenerationMode,
                        new[]
                        {
                            "None",
                            "Open Settings Activity (Basic only)",
                            "Open Preview Screen"
                        }
                        );

                if (_projectSettings.LiveWallpaperLauncherShortcutGenerationMode == LiveWallpaperProjectBuilder.LiveWallpaperLauncherShortcutGenerationMode.OpenSettings &&
                    _projectSettings.LiveWallpaperSettingsActivityGenerationMode != LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode.Basic
                    ) {
                    _projectSettings.LiveWallpaperSettingsActivityGenerationMode = LiveWallpaperProjectBuilder.LiveWallpaperSettingsActivityGenerationMode.Basic;
                }

                _projectSettings.LiveWallpaperBuildOptions =
                    (LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags) _projectSettings.LiveWallpaperBuildOptions.SetFlag(
                        LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags.CreateUnityActivity,
                        EditorGUILayout.Toggle(
                            "Create Unity Activity",
                            _projectSettings.LiveWallpaperBuildOptions.HasFlag(LiveWallpaperProjectBuilder.LiveWallpaperBuildOptionsFlags.CreateUnityActivity)
                            )
                        );

                EditorGUIUtility.labelWidth = 0f;
            });

            bool isError = true;
            LiveWallpaperBuildGuiUtility.ProjectCreateState projectCreateState =
                LiveWallpaperBuildGuiUtility.GetProjectCreateState(_projectSettings.ProjectCreatePath);
            switch (projectCreateState) {
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.CanCreateProject:
                    isError = false;
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.DetectedAndroidStudioProject:
                    EditorGUILayoutExtensions.HelpBox(
                        "Detected <i>Android Studio</i> project.\nTo update the project, switch to <i>Update Project</i> tab.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.DetectedEclipseAdtProject:
                    EditorGUILayoutExtensions.HelpBox(
                        "Detected <i>Eclipse ADT</i> project.\nYou must import it to <i>Android Studio</i> " +
                        "and use the <i>Update Project</i> tab afterwards.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.DirectoryNotEmpty:
                    EditorGUILayoutExtensions.HelpBox(
                        "Selected directory is not empty. Please select an empty directory.\n" +
                        "To update an existing project, switch to <i>Update Project</i> tab.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.ProjectPathIsNotDirectory:
                    EditorGUILayoutExtensions.HelpBox(
                        "Selected path is not a directory. Please select a valid empty directory.",
                        MessageType.Error,
                        GUI.skin.label);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.ParentDirectoryNotExists:
                    EditorGUILayoutExtensions.HelpBox(
                        "Parent directory does not exists. Please select a valid directory.",
                        MessageType.Error,
                        EditorGUIStyles.RichTextLabelWordWrap);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.ProjectPathIsInvalid:
                    EditorGUILayoutExtensions.HelpBox(
                        "Project path is invalid. Please select a valid directory.",
                        MessageType.Error,
                        GUI.skin.label);
                    break;
                case LiveWallpaperBuildGuiUtility.ProjectCreateState.ProjectPathEmpty:
                    break;
                default:
                    throw
                        new InvalidEnumArgumentException(
                            "projectCreateState",
                            (int) projectCreateState,
                            typeof(LiveWallpaperBuildGuiUtility.ProjectCreateState));
            }

            if (!isError && PlayerSettings.Android.preferredInstallLocation != AndroidPreferredInstallLocation.ForceInternal) {
                Action drawButton = () => {
                    GUILayout.Space(2f);
                    EditorGUILayoutExtensions.BeginHorizontalCenter();
                    if (GUILayout.Button("Set to Force Internal", GUILayout.Width(200f), GUILayout.Height(20f))) {
                        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.ForceInternal;
                    }
                    EditorGUILayoutExtensions.EndHorizontalCenter();
                    GUILayout.Space(5f);
                };

                EditorGUILayoutExtensions.HelpBox(
                    "Preferred install location is not set to <i>Force Internal</i>, " +
                    "which is required for live wallpapers to load after reboot. " +
                    "It will be forced when creating the project.",
                    MessageType.Warning,
                    EditorGUIStyles.RichTextLabelWordWrap,
                    drawButton);
            }

            GUILayout.Space(5f);

            EditorGUI.BeginDisabledGroup(isError);
            EditorGUILayoutExtensions.BeginHorizontalCenter();
            {
                GUIContent buttonGuiContent = new GUIContent(" Create Project", EditorResourcesManager.Get<Texture2D>("Textures/UI-CreateProject.png"));
                if (GUILayout.Button(buttonGuiContent, EditorGUIStyles.LargeButton, GUILayout.MinWidth(200f), GUILayout.MaxWidth(250f))) {
                    EditorApplication.delayCall += CreateLiveWallpaperProject;
                }
            }
            EditorGUILayoutExtensions.EndHorizontalCenter();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(7f);

            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            DrawQuickGuide();

            UpdateUpdatedProjectPath(projectCreateState);
        }

        private void DrawAbout() {
            GUILayout.Label("<b>Version Info</b>", EditorGUIStyles.RichTextLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.LabelField(
                    string.Format(
                        "uLiveWallpaper <i>{0}</i>, built on <i>{1:MMMM d, yyyy}</i>.",
                        Constants.kVersionFull,
                        Constants.kReleaseDate
                        ),
                    EditorGUIStyles.RichTextLabel);
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(5f);

            const float linkEllipsisMargin = 5f;
            const float linkRightMargin = 55f;

            GUILayout.Label("<b>Documentation</b>", EditorGUIStyles.RichTextLabel);
            EditorGUI.indentLevel++;
            {
                DrawAboutLink("Manual:", "http://cdn.lostpolygon.com/unity-assets/ulivewallpaper/files/uLiveWallpaper - User Manual.pdf", null, linkEllipsisMargin, linkRightMargin);
                DrawAboutLink("API Reference:", "http://cdn.lostpolygon.com/unity-assets/ulivewallpaper/api-reference", null, linkEllipsisMargin, linkRightMargin);
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(5f);

            GUILayout.Label("<b>Support</b>", EditorGUIStyles.RichTextLabel);
            EditorGUI.indentLevel++;
            {
                DrawAboutLink("Unity Forum Thread:", "http://forum.unity3d.com/threads/ulivewallpaper-develop-android-live-wallpaper-with-unity-5.375255/", null, linkEllipsisMargin, linkRightMargin);
                DrawAboutLink("E-mail:", "mailto:contact@lostpolygon.com", "contact@lostpolygon.com");
                DrawAboutLink("Skype:", "skype:serhii.yolkin?chat", "serhii.yolkin");
                DrawAboutLink("Website:", "http://lostpolygon.com", "http://lostpolygon.com");
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(15f);

            EditorGUILayoutExtensions.BeginHorizontalCenter(null, GUILayout.Width(position.width - 65f));

            GUIStyle helpBoxBigPadding = new GUIStyle(EditorStyles.helpBox);
            helpBoxBigPadding.padding = new RectOffset(11, 11, 7, 9);
            GUILayout.BeginVertical(helpBoxBigPadding, GUILayout.Width(200f));

            EditorGUILayoutExtensions.BeginHorizontalCenter();
            GUILayout.Label("<i>Developed by</i>", EditorGUIStyles.RichTextLabel);
            EditorGUILayoutExtensions.EndHorizontalCenter();

            EditorGUILayoutExtensions.BeginHorizontalCenter();
            GUILayout.Label(EditorResourcesManager.Get<Texture2D>("Textures/LostPolygon-Logo.png"));
            EditorGUIExtensions.SetLinkRect(GUILayoutUtility.GetLastRect(), "http://lostpolygon.com");
            EditorGUILayoutExtensions.EndHorizontalCenter();

            EditorGUILayoutExtensions.BeginHorizontalCenter();
            DrawLinkLabel("See more products on Asset Store", "https://www.assetstore.unity3d.com/en/#!/publisher/3668");
            EditorGUILayoutExtensions.EndHorizontalCenter();
            GUILayout.EndVertical();

            EditorGUILayoutExtensions.EndHorizontalCenter();

            GUILayout.Space(10f);
        }

        private void UpdateUpdatedProjectPath(LiveWallpaperBuildGuiUtility.ProjectCreateState projectCreateState) {
            if (!UnityVersionUtility.IsGradleBuildSystemSupported)
                return;

            if (projectCreateState != LiveWallpaperBuildGuiUtility.ProjectCreateState.DetectedAndroidStudioProject)
                return;

            LiveWallpaperBuildGuiUtility.ProjectUpdateState projectUpdateState = LiveWallpaperBuildGuiUtility.GetProjectUpdateState(_projectSettings.ProjectUpdatePath);
            if (projectUpdateState != LiveWallpaperBuildGuiUtility.ProjectUpdateState.ProjectPathEmpty)
                return;

            _projectSettings.ProjectUpdatePath = _projectSettings.ProjectCreatePath;
        }

        private void DrawAboutLink(string title, string urlAddress, string urlText = null, float ellipsisPadding = 15f, float rightMargin = 0f) {
            if (urlText == null) {
                urlText = urlAddress;
            }

            EditorGUILayout.BeginHorizontal();
            {
                float indentSize = EditorGUI.indentLevel * 18f;
                GUILayout.Space(indentSize);

                Vector2 titleSize = GUI.skin.label.CalcSize(new GUIContent(title));
                GUILayout.Label(title, GUILayout.Width(titleSize.x));

                Vector2 linkSize = EditorGUIStyles.LinkLabel.CalcSize(new GUIContent(urlText));
                Vector2 linkSizeFit = linkSize;
                bool mustDrawEllipsis = false;
                float totalWidth = titleSize.x + indentSize * 1.5f + rightMargin + linkSize.x;
                if (totalWidth > position.width - ellipsisPadding) {
                    linkSizeFit.x = position.width - totalWidth + linkSize.x - ellipsisPadding;
                    mustDrawEllipsis = true;
                }
                GUILayout.Label(urlText, EditorGUIStyles.LinkLabel, GUILayout.Width(linkSizeFit.x));
                Rect linkRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type == EventType.Repaint) {
                    GUI.Label(linkRect, new string('_', urlText.Length), EditorGUIStyles.LinkLabel);

                    if (mustDrawEllipsis) {
                        Texture2D gradient = EditorResourcesManager.Get<Texture2D>("Textures/AlphaGradientLeft.png");

                        Rect gradientRect = linkRect;
                        gradientRect.xMin = gradientRect.xMax - 25f;

                        Color oldColor = GUI.color;
                        GUI.color = EditorGUIStyles.EditorWindowBackgroundColor;
                        GUI.DrawTexture(gradientRect, gradient, ScaleMode.StretchToFill, true);
                        GUI.color = oldColor;
                    }
                }

                EditorGUIExtensions.SetLinkRect(linkRect, urlAddress);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuickGuide() {
            EditorGUILayoutExtensions.BeginHorizontalCenter();
            _quickGuideVisible.target = GUILayout.Toggle(_quickGuideVisible.target, "Show Quick Guide", GUI.skin.button, GUILayout.Width(150f), GUILayout.Height(25f));
            _projectSettings.MainWindowShowQuickGuide = _quickGuideVisible.target;
            EditorGUILayoutExtensions.EndHorizontalCenter();
            if (EditorGUILayout.BeginFadeGroup(_quickGuideVisible.faded)) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                QuickGuideDrawer.DrawQuickGuide();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFadeGroup();
        }



        private static void DrawLinkLabel(string text, string address, params GUILayoutOption[] options) {
            GUILayout.Label(text, EditorGUIStyles.LinkLabel, options);
            Rect linkRect = GUILayoutUtility.GetLastRect();

            if (Event.current.type == EventType.Repaint) {
                GUI.Label(linkRect, new string('_', text.Length), EditorGUIStyles.LinkLabel);
            }

            EditorGUIExtensions.SetLinkRect(linkRect, address);
        }

        private static void DrawSettings(Action contentsFunc) {
            GUILayout.Label("<b>Settings</b>", EditorGUIStyles.RichTextLabel);
            EditorGUI.indentLevel++;
            contentsFunc();
            EditorGUI.indentLevel--;
        }

        private static void DrawProjectBrowseField(string labelText, string pathFieldPlaceholderText, ref string path) {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                GUILayout.Label(labelText, GUILayout.ExpandWidth(false));

                const float browseButtonHeight = 22f;
                const float browseButtonWidth = 75f;
                const float browseButtonLeftPadding = 5f;
                const float browseButtonRightPadding = 0f;
                Rect fieldAndButtonRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.label);
                Rect fieldRect = fieldAndButtonRect;
                fieldRect.width -= browseButtonWidth + browseButtonLeftPadding + browseButtonRightPadding;
                Rect buttonRect = fieldAndButtonRect;
                buttonRect.xMin = fieldRect.xMax + browseButtonLeftPadding;
                buttonRect.width = browseButtonWidth;
                buttonRect.y = buttonRect.yMin + (buttonRect.yMax - buttonRect.yMin) * 0.5f - browseButtonHeight * 0.5f;
                buttonRect.height = browseButtonHeight;
                if (path == null)
                    path = "";

                string pathFieldControlName = "Control_" + labelText;

                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                GUI.SetNextControlName(pathFieldControlName);
                path = EditorGUI.TextField(fieldRect, path);
                EditorGUI.indentLevel = indentLevel;
                if (GUI.Button(buttonRect, "Browse...")) {
                    GUIExtensions.ResetFocus();
                    string dialogStartPath = Directory.Exists(path) ? path : Path.GetFullPath(".");
                    string newPath = EditorUtility.SaveFolderPanel("Select destination Live Wallpaper project folder", dialogStartPath, PlayerSettings.productName);
                    if (!string.IsNullOrEmpty(newPath)) {
                        path = newPath;
                    }
                }

                if (String.IsNullOrEmpty(path) && GUI.GetNameOfFocusedControl() != pathFieldControlName) {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        Rect placeholdeRect = fieldRect;
                        placeholdeRect.xMin += 1f;
                        GUI.Label(placeholdeRect, pathFieldPlaceholderText);
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }

        private void DrawCopyright(Rect windowRect) {
            const float copyrightHeight = 20f;
            const string copyrightText = "Lost Polygon © 2013-2016, ";
            Func<Rect, Rect> calcRect = inRect => new Rect(5f, inRect.height - copyrightHeight, GUI.skin.label.CalcSize(new GUIContent(copyrightText)).x, copyrightHeight);
            Rect rect = calcRect(windowRect);

            if (position.height >= windowRect.height) {
                windowRect = position;
                rect = calcRect(windowRect);
            }

            GUI.Label(rect, copyrightText);

            rect.x += rect.width;
            rect.width = GUI.skin.label.CalcSize(new GUIContent("LostPolygon.com")).x + 2f;
            Rect underlineRect = rect;
            //underlineRect.width -= 1f;
            GUI.Label(underlineRect, "______________________", EditorGUIStyles.LinkLabel);
            GUI.Label(rect, "LostPolygon.com", EditorGUIStyles.LinkLabel);
            EditorGUIExtensions.SetLinkRect(rect, "http://lostpolygon.com");

            const string versionString = Constants.kVersionFull;
            rect.x = windowRect.width - GUI.skin.label.CalcSize(new GUIContent(versionString)).x - 3f;
            GUI.Label(rect, versionString);
        }

        private void UpdateLiveWallpaperProject() {
            try {
                LiveWallpaperBuildGuiUtility.UpdateLiveWallpaperProject(_projectSettings);
            } catch (OperationCanceledException) {
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private  void CreateLiveWallpaperProject() {
            try {
                LiveWallpaperBuildGuiUtility.CreateLiveWallpaperProject(_projectSettings);
            } catch (OperationCanceledException) {
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private static class QuickGuideDrawer {
            const float kParagraphMargin = 7f;

            private const string kLastStepContent =
                "Test if your wallpaper project builds fine by doing <i>'Build → Rebuild Project'</i>. " +
                "If no errors will occur, you're all set! Otherwise, recheck previous steps.\n\n" +
                "To run the live wallpaper on your device, first connect your device to the computer, " +
                "then click the <i>'Run'</i> button (green triangle), or use <i>'Run → Run'</i> main menu item.\n\n" +
                "To update your <i>Android Studio</i> project later on, use the <i>Update Project</i> tab.\n\n" +
                "<i>Note:</i> Android Studio might show a <i>'Error launching <project-name>: Default Activity not found'</i> error message " +
                "adter you try to run your live wallpaper on a device. To fix this, select menu <i>'Run → Edit Configurations'</i>. " +
                "There, in the <i>'General'</i> tab for <i>'Android App'</i>, look for <i>'Launch Options'</i> " +
                "and select Nothing instead of Default Activity.";

            public static void DrawQuickGuide() {
                if (UnityVersionUtility.IsGradleBuildSystemSupported) {
                    DrawQuickGuideGradle();
                } else {
                    DrawQuickGuideAdt();
                }

                GUILayout.Label("Have fun developing Live Wallpapers!", EditorGUIStyles.QuickGuideTitle);
                GUILayout.Space(kParagraphMargin);
            }

            private static void DrawGuideStep(string title, string content) {
                GUILayout.Label(title, EditorGUIStyles.QuickGuideTitle);
                GUILayout.Label(content, EditorGUIStyles.RichTextLabelWordWrap);
                GUILayout.Space(kParagraphMargin);
            }

            private static void DrawQuickGuideGradle() {
                DrawGuideStep(
                    "Step 1",
                    "Set the project generation settings and build the Android Studio project using the <i>Create Project</i> button above."
                );

                DrawGuideStep(
                    "Step 2",
                    "Start Android Studio. " +
                    "If <i>'Welcome to Android Studio'</i> splash screen appears, " +
                    "click <i>'Open an existing Android Studio project'</i>, otherwise, click <i>'File → Open...'</i> in the main menu. " +
                    "Locate and select the project you’ve just created. Click <i>OK</i>.\n\n" +
                    "If a <i>'Gradle Sync'</i> window asking to use the Gradle wrapper shows up, click <i>OK</i>. " +
                    "Wait for project to prepare. If an <i>'Android Gradle Plugin Update Recommended'</i> window shows up, click <i>Update</i>.\n\n" +
                    "Wait for the project to build."
                    );

                DrawGuideStep(
                    "Step 3",
                    kLastStepContent
                );
            }

            private static void DrawQuickGuideAdt() {
                DrawGuideStep(
                    "Step 1",
                    "Set the project generation settings and build the Android project using the <i>Create Project</i> button above."
                    );

                DrawGuideStep(
                    "Step 2",
                    "Open <i>Android Studio</i>. " +
                    "In case a <i>'Welcome to Android Studio'</i> splash screen appears, " +
                    "click <i>'Import project (Eclipse ADT, Gradle, etc.)'</i>, otherwise, " +
                    "click <i>'File... → New → Import Project...'</i>. Select the project you've just created. Click <i>OK</i>.\n\n" +
                    "<i>'Import Project from ADT'</i> window should open. Select the destination directory for imported project. " +
                    "Click <i>Next</i>, leave import settings as-is, click <i>Finish</i>. Wait for import to succeed."
                    );

                DrawGuideStep(
                    "Step 3",
                    "Click <i>'File... → New → New Module... → Import .JAR/.AAR Package'</i> and import <i>'" +
                    Constants.kULiveWallpaperLibraryName + ".aar'</i> library, " +
                    "located in <i>'Assets/uLiveWallpaper/Libraries/'</i> directory inside your Unity project. " +
                    "Leave all settings by default."
                );

                DrawGuideStep(
                    "Step 4",
                    "Click <i>'File → Project Structure...'</i> to open the <i>'Project Structure'</i> window. " +
                    "In the <i>'Modules'</i> list (on the left), select the <i>'app'</i> module.\n\n" +
                    "Switch to <i>'Dependencies'</i> tab. " +
                    "Click the green <i>'+'</i> button at the right side and add Module dependency <i>'" +
                    Constants.kULiveWallpaperLibraryName + "'</i>.\n\n" +
                    "Finally, click <i>OK</i> to close the <i>'Project Structure'</i> window."
                );

                DrawGuideStep(
                    "Step 5",
                    kLastStepContent
                );
            }
        }
    }
}
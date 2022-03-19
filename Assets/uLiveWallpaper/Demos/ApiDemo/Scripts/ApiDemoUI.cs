using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using System.Globalization;
using System.Text;
#endif

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// API demo. Uses all uLiveWallpaper APIs in order to demonstrate their usage.
    /// </summary>
    public class ApiDemoUI : MonoBehaviour {
#pragma warning disable 0414
        private const int kMaxLogItems = 50;
        private readonly List<string> _logItems = new List<string>();

        [SerializeField]
        private Text _isVisibleText;

        [SerializeField]
        private Text _isPreviewText;

        [SerializeField]
        private Text _wallpaperOffsetText;

        [SerializeField]
        private Text _desiredSizeText;

        [SerializeField]
        private Text _logText;

        private string _logTextString = "";
        private bool _logOffsetsChanges;
        private const string kTestPreferenceKey = "test_preference";

#pragma warning restore 0414

        public void OpenSettingsActivityClicked() {
#if UNITY_ANDROID
            LiveWallpaper.StartDefaultSettingsActivity();
#endif
        }

        public void OpenPreviewScreenClicked() {
#if UNITY_ANDROID
            LiveWallpaper.OpenPreviewScreen();
#endif
        }

        public void SetTestPreferenceClicked() {
#if UNITY_ANDROID
            // All changes to preferences must be wrapped in StartEditing()/FinishEditing()
            LiveWallpaper.Preferences.StartEditing();
            // Write some random value into preferences
            int randomValue = UnityEngine.Random.Range(1000, 999999);
            if (LiveWallpaper.Preferences.SetInt(kTestPreferenceKey, randomValue)) {
                AddLogItem(string.Format("Preference '{0}' value set to {1}", kTestPreferenceKey, randomValue));
            }
            LiveWallpaper.Preferences.FinishEditing();
#endif
        }

        public void GetTestPreferenceClicked() {
#if UNITY_ANDROID
            int testPreferenceValue = LiveWallpaper.Preferences.GetInt(kTestPreferenceKey, -1);
            AddLogItem(string.Format("Preference '{0}' current value is {1}", kTestPreferenceKey, testPreferenceValue));
#endif
        }

        public void SetLogOffsetsChangeChanged(bool value) {
            _logOffsetsChanges = value;
        }

        public void ClearLog() {
            _logItems.Clear();
            _logTextString = "";
        }

#if UNITY_ANDROID
        private void OnEnable() {
            // Register the event listeners
            LiveWallpaper.VisibilityChanged += LiveWallpaperOnVisibilityChanged;
            LiveWallpaper.OffsetsChanged += LiveWallpaperOnOffsetsChanged;
            LiveWallpaper.IsPreviewChanged += LiveWallpaperOnIsPreviewChanged;
            LiveWallpaper.DesiredSizeChanged += LiveWallpaperOnDesiredSizeChanged;
            LiveWallpaper.PreferenceChanged += LiveWallpaperOnPreferenceChanged;
            LiveWallpaper.PreferenceActivityTriggered += LiveWallpaperOnPreferenceActivityTriggered;
            LiveWallpaper.MultiTapDetected += LiveWallpaperOnMultiTapDetected;
            LiveWallpaper.CustomEventReceived += LiveWallpaperOnCustomEventReceived;
        }

        private void OnDisable() {
            // Unregister the event listeners.
            // It is important to do this, otherwise you'll get memory leaks!
            LiveWallpaper.VisibilityChanged -= LiveWallpaperOnVisibilityChanged;
            LiveWallpaper.OffsetsChanged -= LiveWallpaperOnOffsetsChanged;
            LiveWallpaper.IsPreviewChanged -= LiveWallpaperOnIsPreviewChanged;
            LiveWallpaper.DesiredSizeChanged -= LiveWallpaperOnDesiredSizeChanged;
            LiveWallpaper.PreferenceChanged -= LiveWallpaperOnPreferenceChanged;
            LiveWallpaper.PreferenceActivityTriggered -= LiveWallpaperOnPreferenceActivityTriggered;
            LiveWallpaper.MultiTapDetected -= LiveWallpaperOnMultiTapDetected;
            LiveWallpaper.CustomEventReceived -= LiveWallpaperOnCustomEventReceived;
        }

        private void Update() {
            // Update the UI with current values
            _isVisibleText.text = BoolToString(LiveWallpaper.IsVisible);
            _isPreviewText.text = BoolToString(LiveWallpaper.IsPreview);
            _wallpaperOffsetText.text =
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Offset.X: {0:0.####}\n" +
                    "Offset.Y: {1:0.####}\n" +
                    "OffsetStep.X: {2:0.####}\n" +
                    "OffsetStep.Y: {3:0.####}\n" +
                    "PixelOffset.X: {4}\n" +
                    "PixelOffset.Y: {5}\n" +
                    "Home screen: {6}/{7}",
                    LiveWallpaper.WallpaperOffset.Offset.x,
                    LiveWallpaper.WallpaperOffset.Offset.y,
                    LiveWallpaper.WallpaperOffset.OffsetStep.x,
                    LiveWallpaper.WallpaperOffset.OffsetStep.y,
                    LiveWallpaper.WallpaperOffset.PixelOffset.x,
                    LiveWallpaper.WallpaperOffset.PixelOffset.y,
                    LiveWallpaper.WallpaperOffset.CurrentHomeScreen + 1,
                    LiveWallpaper.WallpaperOffset.HomeScreenCount
                    );

            _desiredSizeText.text =
                string.Format(
                    "Width: {0}\nHeight: {1}",
                    LiveWallpaper.WallpaperDesiredSize.Width,
                    LiveWallpaper.WallpaperDesiredSize.Height
                    );

            _logText.text = _logTextString;
        }

        private void LiveWallpaperOnOffsetsChanged(Vector2 offset, Vector2 offsetStep, LiveWallpaper.Point pixelOffset) {
            if (!_logOffsetsChanges)
                return;

            AddLogItem(
                string.Format(
                    "Offsets changed:\n" +
                    "\tOffset.X: {0:0.####}\n" +
                    "\tOffset.Y: {1:0.####}\n" +
                    "\tOffsetStep.X: {2:0.####}\n" +
                    "\tOffsetStep.Y: {3:0.####}\n" +
                    "\tPixelOffset.X: {4}\n" +
                    "\tPixelOffset.Y: {5}",
                    offset.x,
                    offset.y,
                    offsetStep.x,
                    offsetStep.y,
                    pixelOffset.x,
                    pixelOffset.y));
        }

        private void LiveWallpaperOnVisibilityChanged(bool isVisible) {
            AddLogItem((LiveWallpaper.IsPreview ? "Preview - " : "Home screen - ") + "Is visible: " + BoolToString(isVisible));
        }

        private void LiveWallpaperOnIsPreviewChanged(bool isPreview) {
            AddLogItem("Is preview: " + BoolToString(isPreview));
        }

        private void LiveWallpaperOnDesiredSizeChanged(int desiredWidth, int desiredHeight) {
            AddLogItem(
                string.Format(
                    "Desired size changed:\n" +
                    "\tWidth: {0}\n" +
                    "\tHeight: {1}",
                    desiredWidth,
                    desiredHeight
                    )
                );
        }

        private void LiveWallpaperOnPreferenceChanged(string key) {
            AddLogItem("Preference changed: " + key);
        }

        private void LiveWallpaperOnPreferenceActivityTriggered() {
            AddLogItem("Preference Activity was opened");
        }

        private void LiveWallpaperOnMultiTapDetected(Vector2 lastTapPosition) {
            AddLogItem("Multiple tap sequence detected");
        }

        private void LiveWallpaperOnCustomEventReceived(string eventName, string eventData) {
            AddLogItem(
                string.Format(
                    "Custom event received:\n" +
                    "\tName: {0}\n" +
                    "\tData: {1}",
                    eventName,
                    eventData
                    )
                );
        }

        private void AddLogItem(string text) {
            _logItems.Add(text);
            if (_logItems.Count > kMaxLogItems) {
                string[] items = new string[kMaxLogItems];
                _logItems.CopyTo(_logItems.Count - kMaxLogItems, items, 0, kMaxLogItems);
                _logItems.Clear();
                _logItems.AddRange(items);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _logItems.Count; i++) {
                sb.Append(_logItems[_logItems.Count - i - 1]);
                if (i < _logItems.Count - 1) {
                    sb.Append('\n');
                }
            }

            _logTextString = sb.ToString();
        }

        private static string BoolToString(bool value) {
            return value ? "True" : "False";
        }
#endif
    }
}
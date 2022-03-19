using System;

using UnityEngine;
using UnityEditor;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Extensions of EditorGUILayout.
    /// </summary>
    internal static class EditorGUILayoutExtensions {
        /// <summary>
        /// A versions of <see cref="M:EditorGUILayout.EnumPopup"/>
        /// that allows to provide custom text for enum values.
        /// </summary>
        /// <param name="label">
        /// Field lable.
        /// </param>
        /// <param name="selected">
        /// Current value of the enum.
        /// </param>
        /// <param name="valuesText">
        /// Corresponding text for the values, to be shown in the popup.
        /// </param>
        /// <returns>
        /// The <see cref="Enum"/>.
        /// </returns>
        public static Enum EnumPopup(string label, Enum selected, string[] valuesText) {
            int enumValue = Convert.ToInt32(selected);
            int[] enumValues = (int[]) Enum.GetValues(selected.GetType());
            enumValue = EditorGUILayout.IntPopup(label, enumValue, valuesText, enumValues);

            return (Enum) Enum.ToObject(selected.GetType(), enumValue);
        }

        /// <summary>
        /// Begins horizontal centering group.
        /// </summary>
        public static Rect BeginHorizontalCenter(GUIStyle style = null, params GUILayoutOption[] options) {
            if (style == null) {
                style = GUIStyle.none;
            }

            Rect rect = EditorGUILayout.BeginHorizontal(style, options);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            return rect;
        }

        /// <summary>
        /// Ends horizontal centering group.
        /// </summary>
        public static void EndHorizontalCenter() {
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// A more customizable version of <see cref="M:EditorGUILayout.HelpBox" />.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The message type.</param>
        /// <param name="messageStyle">The <see cref="GUIStyle" /> to use for the <paramref name="message" />.</param>
        /// <param name="options">The options.</param>
        public static void HelpBox(string message, MessageType type, GUIStyle messageStyle, params GUILayoutOption[] options) {
            HelpBox(message, type, messageStyle, null, options);
        }

        /// <summary>
        /// A more customizable version of <see cref="M:EditorGUILayout.HelpBox" />.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The message type.</param>
        /// <param name="messageStyle">The <see cref="GUIStyle" /> to use for the <paramref name="message" />.</param>
        /// <param name="userDraw">Delegate called after drawing the text.</param>
        /// <param name="options">The options.</param>
        public static void HelpBox(string message, MessageType type, GUIStyle messageStyle, Action userDraw,  params GUILayoutOption[] options) {
            Texture2D icon = null;
            switch (type) {
                case MessageType.Info:
                    icon = EditorGUIStyles.InfoIconSmall;
                    break;
                case MessageType.Warning:
                    icon = EditorGUIStyles.WarningIconSmall;
                    break;
                case MessageType.Error:
                    icon = EditorGUIStyles.ErrorIconSmall;
                    break;
            }

            if (messageStyle == null) {
                messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.font = EditorStyles.helpBox.font;
                messageStyle.fontSize = EditorStyles.helpBox.fontSize;
                messageStyle.fontSize = EditorStyles.helpBox.fontSize;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox, options);
            {
                GUILayout.BeginHorizontal();
                {
                    Rect iconRect = new Rect();
                    if (icon != null) {
                        GUIContent iconContent = new GUIContent(icon);
                        iconRect = GUILayoutUtility.GetRect(iconContent, GUI.skin.label, GUILayout.ExpandWidth(false));
                        GUI.Label(iconRect, iconContent, GUI.skin.label);
                    }

                    GUIContent messageContent = new GUIContent(message);
                    Rect messageRect = GUILayoutUtility.GetRect(messageContent, messageStyle);
                    bool isMultiLine = messageStyle.wordWrap && messageRect.height > messageStyle.lineHeight;
                    bool isCenteredToIcon = icon != null && !isMultiLine;
                    if (isCenteredToIcon) {
                        messageRect.y = iconRect.yMin + (iconRect.yMax - iconRect.yMin) * 0.5f - messageStyle.lineHeight * 0.6f;
                    }

                    GUI.Label(messageRect, messageContent, messageStyle);
                }
                GUILayout.EndHorizontal();

                if (userDraw != null) {
                    userDraw();
                }
            }
            GUILayout.EndVertical();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using LostPolygon.uLiveWallpaper.Editor.Internal;

namespace LostPolygon.uLiveWallpaper.Editor {
    public abstract class LiveWallpaperProjectManipulatorBase {
        protected const string kAndroidManifestXmlName = "AndroidManifest.xml";

        private static readonly string[] kULiveWallpaperLibraryActivities =
        {
            "com.lostpolygon.unity.livewallpaper.StartWallpaperPreviewActivity",
            "com.lostpolygon.unity.androidintegration.UnityPlayerInstantiatorActivity"
        };

        protected string[] _buildScenes;
        protected BuildOptions _buildOptions;
        protected readonly string _projectRootPath;

        /// <summary>
        /// Called when staging project build has finished.
        /// </summary>
        public event Action StagingProjectBuildFinished;

        /// <summary>
        /// <see cref="UnityEditor.BuildOptions"/> for Unity player.
        /// </summary>
        public BuildOptions BuildOptions {
            get { return _buildOptions; }
            set { _buildOptions = value; }
        }

        /// <summary>
        /// Scenes to include into build. Scene list from Build Settings will be used if this iss et to null.
        /// </summary>
        public string[] BuildScenes {
            get { return _buildScenes; }
            set { _buildScenes = value; }
        }

        /// <summary>
        /// Destination project root path.
        /// </summary>
        public string ProjectRootPath {
            get { return _projectRootPath; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveWallpaperProjectManipulatorBase"/> class.
        /// </summary>
        /// <param name="projectRootPath">Project root path.</param>
        protected LiveWallpaperProjectManipulatorBase(string projectRootPath) {
            _projectRootPath = projectRootPath;
        }

        /// <summary>
        /// Invokes the <see cref="StagingProjectBuildFinished"/> event.
        /// </summary>
        protected void InvokeStagingProjectBuildFinished() {
            if (StagingProjectBuildFinished != null) {
                StagingProjectBuildFinished();
            }
        }

        protected static XmlElement GetFirstChildElementWithName(XmlElement element, string parentNodeName) {
            XmlElement parentNode =
                element
                .ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault(childElement => childElement.LocalName == parentNodeName);

            return parentNode;
        }

        protected static void UpdateUnityVersionMetaDataTag(XmlDocument androidManifestXmlDocument) {
            const string metaDataName = "uLiveWallpaper.UnityVersion";
            string unityVersion =
                String.Format(
                    "{0}.{1}.{2}",
                    UnityVersionUtility.UnityVersion.VersionMajor,
                    UnityVersionUtility.UnityVersion.VersionMinor,
                    UnityVersionUtility.UnityVersion.VersionPatch);

            XmlElement applicationElement = GetFirstChildElementWithName(androidManifestXmlDocument.DocumentElement, "application");
            XmlElement unityVersionMetaDataElement =
                applicationElement
                    .ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault(element => element.LocalName == "meta-data" && element.HasAttribute("android:name") && element.Attributes["android:name"].Value == metaDataName);

            const string androidXmlNamespace = "http://schemas.android.com/apk/res/android";
            if (unityVersionMetaDataElement == null) {
                unityVersionMetaDataElement = androidManifestXmlDocument.CreateElement("meta-data");
                unityVersionMetaDataElement.SetAttribute("android:name", androidXmlNamespace, metaDataName);
                applicationElement.AppendChild(unityVersionMetaDataElement);
            }

            unityVersionMetaDataElement.SetAttribute("android:value", androidXmlNamespace, unityVersion);
        }

        protected static IEnumerable<XmlElement> GetAndroidManifestActivityNodes(XmlDocument androidManifestXmlDocument) {
            XmlElement root = androidManifestXmlDocument.DocumentElement;

            IEnumerable<XmlElement> activityNodes =
                root
                    .ChildNodes
                    .OfType<XmlElement>()
                    .Where(element => element.LocalName == "application")
                    .SelectMany(element => element.ChildNodes.OfType<XmlElement>())
                    .Where(element => element.LocalName == "activity");
            return activityNodes;
        }

        protected static void RemoveLibraryAndroidActivities(XmlDocument androidManifestXmlDocument) {
            IEnumerable<XmlElement> activityNodes = GetAndroidManifestActivityNodes(androidManifestXmlDocument);
            XmlElement[] removedActivities =
                activityNodes
                .Where(activityNode => {
                    XmlAttribute nameAttribute = activityNode.Attributes["android:name"];
                    if (nameAttribute == null)
                        return false;

                    if (kULiveWallpaperLibraryActivities.Contains(nameAttribute.Value))
                        return true;

                    return false;
                })
                .ToArray();

            foreach (XmlElement activity in removedActivities) {
                activity.ParentNode.RemoveChild(activity);
            }
        }
    }
}
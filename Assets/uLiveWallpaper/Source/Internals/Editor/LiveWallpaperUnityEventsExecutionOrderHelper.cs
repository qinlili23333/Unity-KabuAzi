using System.Linq;
using UnityEditor;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// Makes sure <seealso cref="LiveWallpaperUnityMessagesForwarder"/> is executed as early as possible.
    /// </summary>
    internal static class LiveWallpaperUnityEventsExecutionOrderHelper {
        [InitializeOnLoadMethod]
        private static void UpdateExecutionOrder() {
            const string typeName = "LiveWallpaperUnityMessagesForwarder";
            const int executionOrder = 20000;

            // Find all script with name
            MonoScript[] monoScripts =
                AssetDatabase
                .FindAssets("t:MonoScript " + typeName)
                .Select(guid => (MonoScript) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonoScript)))
                .Where(monoScript => monoScript.name == typeName)
                .ToArray();

            foreach (MonoScript monoScript in monoScripts) {
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

                // Allow custom order
                if (currentExecutionOrder != 0)
                    return;

                MonoImporter.SetExecutionOrder(monoScript, executionOrder);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(monoScript), ImportAssetOptions.ForceUpdate);
            }
        }
    }
}

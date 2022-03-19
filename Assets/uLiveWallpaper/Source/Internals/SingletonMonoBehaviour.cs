using System;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Internal {
    /// <summary>
    /// A reliable singleton implementation for <seealso cref="MonoBehaviour" />. Makes sure only one instance ever exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
        protected static T _instance;

        protected bool _isDestroyed;

        /// <summary>
        /// A reference to singleton instance.
        /// </summary>
        public static T Instance {
            get {
                UpdateInstance();
                return _instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in process of being destroyed.
        /// </summary>
        protected bool IsDestroyed {
            get { return _isDestroyed; }
        }

        protected static void TryUpdateInstance() {
            try {
                UpdateInstance();
            } catch {
                // Happens when this static constructor is called from a GameObject being created.
                // Just ignoring, as this is intended and can't be avoided.
            }
        }

        /// <summary>
        /// Tries to find an existing instance in the scene,
        /// and creates one if there were none.
        /// </summary>
        private static void UpdateInstance() {
            if (_instance != null)
                return;

            // Trying to find an existing instance in the scene
            _instance = (T) FindObjectOfType(typeof(T));

            // Creating a new instance in case there are no instances present in the scene
            if (_instance != null)
                return;

            GameObject gameObject = new GameObject();
            _instance = gameObject.AddComponent<T>();
        }

        protected virtual void Awake() {
            // Kill this instance, if other instances exist
            if (FindObjectsOfType(typeof(T)).Length > 1) {
                Debug.LogError("Multiple " + typeof(T).Name + " instances found, destroying...");
                _isDestroyed = true;

                Component[] components = gameObject.GetComponents<Component>();
                if (components.Length > 1) {
                    DestroyImmediate(this);
                } else {
                    DestroyImmediate(gameObject);
                }

                return;
            }

            _instance = GetComponent<T>();

            //gameObject.name = "_" + typeof(T).Name;

            // We want this object to persist across scenes
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(this);
        }

        protected virtual void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}
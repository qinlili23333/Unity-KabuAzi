using System.Collections.Generic;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Manages spawning and despawning a number of identical objects.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class ObjectSpawner : MonoBehaviour {
        [SerializeField]
        private bool _spawnOnStart = true;

        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private float _resetPositionRandomDistance = 3f;

        [SerializeField]
        private int _numberOfObjects = 15;

        private readonly List<GameObject> _spawnedObjects = new List<GameObject>();

        public int NumberOfObjects {
            get { return _numberOfObjects; }
            set { _numberOfObjects = value; }
        }

        public void Respawn() {
            Despawn();

            for (int i = 0; i < _numberOfObjects; i++) {
                // The position is calculated as point within a random sphere to avoid physics "exploding" 
                // due to a ton of objects in the same place
                Vector3 position = transform.position + Random.insideUnitSphere * _resetPositionRandomDistance;
                Quaternion rotation = Random.rotationUniform;

                GameObject go = (GameObject) Instantiate(_prefab, position, rotation);
                go.transform.parent = transform;
                _spawnedObjects.Add(go);
            }
        }

        public void Despawn() {
            for (int i = 0; i < _spawnedObjects.Count; i++) {
                Destroy(_spawnedObjects[i]);
            }

            _spawnedObjects.Clear();
        }

        private void Start() {
            if (_spawnOnStart) {
                Respawn();
            }
        }

        private void OnDestroy() {
            Despawn();
        }
    }
}
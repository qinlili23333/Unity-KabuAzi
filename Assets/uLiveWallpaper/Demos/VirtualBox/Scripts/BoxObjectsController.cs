using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Makes sure objects with <see cref="Rigidbody"/> stay within the camera range.
    /// </summary>
    public class BoxObjectsController : MonoBehaviour {
        [SerializeField]
        private Transform _objectsRoot;

        [SerializeField]
        private Vector3 _resetPosition;

        [SerializeField]
        private float _resetPositionRandomDistance = 3f;

        private Vector2 _prevCameraSize;

        private void OnEnable() {
            _prevCameraSize = CameraUtilities.GetFrustum(Camera.main, 0f, -10f);
        }

        private void Update() {
            Vector2 cameraSize = CameraUtilities.GetFrustum(Camera.main, 0f, -10f);

            // Orientation change
            if (_prevCameraSize != cameraSize) {
                Rigidbody[] childRigidbodies = _objectsRoot.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody childRigidbody in childRigidbodies) {
                    if (childRigidbody.position.x < -cameraSize.x * 0.3f ||
                        childRigidbody.position.x > cameraSize.x * 0.3f ||
                        childRigidbody.position.y < -cameraSize.y * 0.3f ||
                        childRigidbody.position.y > cameraSize.y * 0.3f
                        ) {
                        childRigidbody.position = _resetPosition + Random.insideUnitSphere * _resetPositionRandomDistance;
                    }
                }
            }

            _prevCameraSize = cameraSize;
        }
    }
}
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// An example of moving an object using the wallpaper offset.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class WallpaperOffsetToObjectTransform : MonoBehaviour {
#pragma warning disable 0414
        [SerializeField]
        private Transform _controlledTransform;

        [SerializeField]
        private bool _applyRotation = true;

        [SerializeField]
        private Vector3 _minRotation = new Vector3(0f, -15f, 0f);

        [SerializeField]
        private Vector3 _maxRotation = new Vector3(0f, 15f, 0f);

        [SerializeField]
        private bool _applyPosition = true;

        [SerializeField]
        private Vector3 _minPosition = new Vector3(-3f, 0f, 0f);

        [SerializeField]
        private Vector3 _maxPosition = new Vector3(3f, 0f, 0f);

        [SerializeField]
        private float _offsetLerp = 10f;

        private float _currentOffset;
#pragma warning restore 0414

#if UNITY_ANDROID
        private void OnEnable() {
            if (_controlledTransform == null) {
                _controlledTransform = transform;
            }

            _currentOffset = LiveWallpaper.WallpaperOffset.Offset.x;
        }

        private void Update() {
            float newOffset = LiveWallpaper.WallpaperOffset.Offset.x;

            // Calculated the smoothed rotation
            _currentOffset = Mathf.Lerp(_currentOffset, newOffset, _offsetLerp * Time.deltaTime);

            // Apply the transformation
            if (_applyRotation) {
                _controlledTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(_minRotation), Quaternion.Euler(_maxRotation), _currentOffset);
            }

            if (_applyPosition) {
                _controlledTransform.localPosition = Vector3.Lerp(_minPosition, _maxPosition, _currentOffset);
            }
        }
#endif
    }
}
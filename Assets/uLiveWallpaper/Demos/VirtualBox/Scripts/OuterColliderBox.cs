using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Positions the box walls to fit the screen.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    [ExecuteInEditMode]
    public class OuterColliderBox : MonoBehaviour {
        [SerializeField]
        private BoxCollider _leftCollider;

        [SerializeField]
        private BoxCollider _rightCollider;

        [SerializeField]
        private BoxCollider _topCollider;

        [SerializeField]
        private BoxCollider _bottomCollider;

        [SerializeField]
        private BoxCollider _backCollider;

        [SerializeField]
        private float _colliderSize = 1f;

        private void Update() {
            Vector2 cameraSize = CameraUtilities.GetFrustum(Camera.main, 0f, -10f);

            if (_leftCollider != null)
                SetTransformBounds(_leftCollider.transform, new Vector3(-cameraSize.x - _colliderSize * 0.5f, 0f, 0f), new Vector3(_colliderSize, cameraSize.y * 2f, 0f));

            if (_rightCollider != null)
                SetTransformBounds(_rightCollider.transform, new Vector3(cameraSize.x + _colliderSize * 0.5f, 0f, 0f), new Vector3(_colliderSize, cameraSize.y * 2f, 0f));

            if (_topCollider != null)
                SetTransformBounds(_topCollider.transform, new Vector3(0f, cameraSize.y + _colliderSize * 0.5f, 0f), new Vector3(cameraSize.x * 2f, _colliderSize, 0f));

            if (_bottomCollider != null)
                SetTransformBounds(_bottomCollider.transform, new Vector3(0f, -cameraSize.y - _colliderSize * 0.5f, 0f), new Vector3(cameraSize.x * 2f, _colliderSize, 0f));

            if (_backCollider != null)
                SetTransformBounds(_backCollider.transform, _backCollider.transform.position, new Vector3(cameraSize.x * 2f, cameraSize.y * 2f, _colliderSize));
        }

        private void SetTransformBounds(Transform trans, Vector3 center, Vector3 size) {
            trans.position = new Vector3(center.x, center.y, trans.position.z);
            trans.localScale = new Vector3(size.x, size.y, trans.localScale.z);
        }
    }
}
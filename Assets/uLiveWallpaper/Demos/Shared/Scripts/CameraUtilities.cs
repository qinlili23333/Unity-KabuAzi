using UnityEngine;

namespace LostPolygon.uLiveWallpaper {
    /// <summary>
    /// Utility methods for working with <see cref="Camera"/>.
    /// </summary>
    public static class CameraUtilities {
        /// <summary>
        /// Calculates the camera frustum at a given Z position.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="objectZPos">
        /// Distance for which the frustum will be calculated.
        /// </param>
        /// <param name="cameraZPos">
        /// Camera Z position. If set to float.NaN, will be replaced with actual camera Z position
        /// </param>
        /// <returns>
        /// The width and height of the frustum.
        /// </returns>
        public static Vector2 GetFrustum(Camera camera, float objectZPos = 0f, float cameraZPos = float.NaN) {
            if (camera.orthographic)
                return new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);

            if (float.IsNaN(cameraZPos)) {
                cameraZPos = camera.transform.position.z;
            }
            float frustumHeight = Mathf.Abs(objectZPos - cameraZPos) * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * camera.aspect, frustumHeight);
        }

        /// <summary>
        /// Copies some <see cref="Camera"/> properties from one camera to another.
        /// </summary>
        /// <param name="targetCamera">
        /// The target camera.
        /// </param>
        /// <param name="sourceCamera">
        /// The source camera.
        /// </param>
        public static void CopyCamera(Camera targetCamera, Camera sourceCamera) {
           targetCamera.aspect = sourceCamera.aspect;
           targetCamera.orthographic = sourceCamera.orthographic;
           targetCamera.orthographicSize = sourceCamera.orthographicSize;
           targetCamera.farClipPlane = sourceCamera.farClipPlane;
        }
    }
}

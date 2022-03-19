using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Rotates the transform to point the direction of gravity.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class RotateTowardsGravity : MonoBehaviour {
        private void Update() {
            if (Physics.gravity.sqrMagnitude > Vector3.kEpsilon) {
                transform.rotation = Quaternion.LookRotation(Physics.gravity.normalized, Vector3.forward);
            }
        }
    }
}
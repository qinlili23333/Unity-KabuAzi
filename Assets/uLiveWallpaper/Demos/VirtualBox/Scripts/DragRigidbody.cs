using System;
using System.Collections;
using UnityEngine;

namespace LostPolygon.uLiveWallpaper.Demos {
    public class DragRigidbody : MonoBehaviour {
        public event Action<Rigidbody> Dragging;
        public event Action<Rigidbody> Released;

        public float AngularDrag = 5f;
        public bool AttachToCenterOfMass = false;
        public float Damper = 50f;
        public float Distance = 0.2f;
        public float Drag = 15f;
        public LayerMask Layers;
        public float Spring = 500f;
        private SpringJoint _springJoint;

        public Rigidbody DraggedRigidbody {
            get {
                if (_springJoint == null)
                    return null;

                return _springJoint.connectedBody;
            }
        }

        private void Update() {
            // Make sure the user pressed the mouse down
            if (!Input.GetMouseButtonDown(0))
                return;

            Camera mainCamera = FindCamera();

            // We need to actually hit an object
            RaycastHit hit;
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000, Layers.value))
                return;

            // We need to hit a rigidbody that is not kinematic
            if (!hit.rigidbody || hit.rigidbody.isKinematic)
                return;

            if (!_springJoint) {
                GameObject go = new GameObject("Rigidbody dragger");
                go.hideFlags = HideFlags.HideInHierarchy;
                Rigidbody body = go.AddComponent<Rigidbody>();
                _springJoint = go.AddComponent<SpringJoint>();
                body.isKinematic = true;
            }

            _springJoint.transform.position = hit.point;
            if (AttachToCenterOfMass) {
                Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
                anchor = _springJoint.transform.InverseTransformPoint(anchor);
                _springJoint.anchor = anchor;
            } else {
                _springJoint.anchor = Vector3.zero;
            }

            _springJoint.spring = Spring;
            _springJoint.damper = Damper;
            _springJoint.maxDistance = Distance;
            _springJoint.connectedBody = hit.rigidbody;

            StartCoroutine("DragObject", hit.distance);
        }

        private IEnumerator DragObject(float distance) {
            if (Dragging != null)
                Dragging(_springJoint.connectedBody);

            float oldDrag = _springJoint.connectedBody.drag;
            float oldAngularDrag = _springJoint.connectedBody.angularDrag;
            _springJoint.connectedBody.drag = Drag;
            _springJoint.connectedBody.angularDrag = AngularDrag;
            Camera mainCamera = FindCamera();
            while (Input.GetMouseButton(0)) {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                _springJoint.transform.position = ray.GetPoint(distance);
                yield return 0;
            }

            if (Released != null)
                Released(_springJoint.connectedBody);

            if (_springJoint.connectedBody) {
                _springJoint.connectedBody.drag = oldDrag;
                _springJoint.connectedBody.angularDrag = oldAngularDrag;
                _springJoint.connectedBody = null;
            }
        }

        private Camera FindCamera() {
            Camera camera = GetComponent<Camera>();
            if (camera != null)
                return camera;

            return Camera.main;
        }
    }
}
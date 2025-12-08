using UnityEngine;

namespace Shared.Foundation
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 6f;
        public float yaw = 0f;
        public float pitch = 15f;
        public float yawSpeed = 120f;
        public float pitchSpeed = 80f;
        public float minPitch = -10f;
        public float maxPitch = 60f;
        public float zoom = 6f;
        public float zoomSpeed = 5f;
        public float minZoom = 3f;
        public float maxZoom = 12f;

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        void LateUpdate()
        {
            if (!target) return;
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
            zoom = Mathf.Clamp(zoom - Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            var rot = Quaternion.Euler(pitch, yaw, 0);
            var pos = target.position - rot * Vector3.forward * zoom + Vector3.up * 1.5f;
            transform.SetPositionAndRotation(pos, rot);
        }
    }

}

using UnityEngine;

namespace HyperloopDash.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 5, -10);
        public float smoothSpeed = 10f;
        public bool lookAtTarget = true;

        private void LateUpdate()
        {
            if (target == null) return;

            // Calculate desired position
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly interpolate
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Optionally look at target
            if (lookAtTarget)
            {
                transform.LookAt(target);
            }
        }
    }
}

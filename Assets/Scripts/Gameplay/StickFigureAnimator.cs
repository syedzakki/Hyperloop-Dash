using UnityEngine;

namespace HyperloopDash.Gameplay
{
    public class StickFigureAnimator : MonoBehaviour
    {
        [Header("Body Parts")]
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform leftArm;
        public Transform rightArm;

        [Header("Animation Settings")]
        public float runSpeed = 5f;
        public float legSwingAngle = 45f;
        public float armSwingAngle = 30f;

        private float _animationTime = 0f;

        private void Update()
        {
            // Only animate when game is playing
            if (Time.timeScale > 0)
            {
                _animationTime += Time.deltaTime * runSpeed;

                // Animate legs (opposite swing)
                if (leftLeg != null)
                {
                    float leftAngle = Mathf.Sin(_animationTime) * legSwingAngle;
                    leftLeg.localRotation = Quaternion.Euler(leftAngle, 0, 0);
                }

                if (rightLeg != null)
                {
                    float rightAngle = Mathf.Sin(_animationTime + Mathf.PI) * legSwingAngle;
                    rightLeg.localRotation = Quaternion.Euler(rightAngle, 0, 0);
                }

                // Animate arms (opposite to legs)
                if (leftArm != null)
                {
                    float leftArmAngle = Mathf.Sin(_animationTime + Mathf.PI) * armSwingAngle;
                    leftArm.localRotation = Quaternion.Euler(leftArmAngle, 0, 0);
                }

                if (rightArm != null)
                {
                    float rightArmAngle = Mathf.Sin(_animationTime) * armSwingAngle;
                    rightArm.localRotation = Quaternion.Euler(rightArmAngle, 0, 0);
                }
            }
        }
    }
}

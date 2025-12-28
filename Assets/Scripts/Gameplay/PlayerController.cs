using UnityEngine;
using System.Collections;
using HyperloopDash.Managers;
using HyperloopDash.Helpers;

namespace HyperloopDash.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float laneDistance = 3.0f;
        public float laneChangeSpeed = 10f;
        public float jumpHeight = 2f;
        public float jumpDuration = 0.5f;
        public float crouchHeight = 0.5f;
        public float crouchDuration = 0.4f;
        
        [Header("Brake Settings")]
        public float brakeTimeScale = 0.3f;
        public float brakeDuration = 0.6f;
        public float brakeCooldown = 2.0f;
        
        [Header("References")]
        public ParticleSystem crashParticles;
        public TrailRenderer trail;

        public int CurrentLane => _currentLane; // Exposed for testing
        private int _currentLane = 1; // 0=Left, 1=Center, 2=Right
        private float _targetX = 0f;
        private bool _canBrake = true;
        private bool _isInvulnerable = false;
        
        private CharacterController _characterController;
        private Vector3 _verticalVelocity;
        private bool _isJumping = false;
        private bool _isCrouching = false;
        private float _normalHeight;
        private Vector3 _normalCenter;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _normalHeight = _characterController.height;
            _normalCenter = _characterController.center;
            
            // Subscribe to Inputs
            SwipeInput.OnSwipeLeft += MoveLeft;
            SwipeInput.OnSwipeRight += MoveRight;
            SwipeInput.OnSwipeUp += Jump;
            SwipeInput.OnSwipeDown += Crouch;
        }

        private void OnDestroy()
        {
            SwipeInput.OnSwipeLeft -= MoveLeft;
            SwipeInput.OnSwipeRight -= MoveRight;
            SwipeInput.OnSwipeUp -= Jump;
            SwipeInput.OnSwipeDown -= Crouch;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleLaneMovement();
        }

        private void MoveLeft()
        {
            if (_currentLane > 0)
            {
                _currentLane--;
                UpdateTargetPosition();
            }
        }

        private void MoveRight()
        {
            if (_currentLane < 2)
            {
                _currentLane++;
                UpdateTargetPosition();
            }
        }

        private void UpdateTargetPosition()
        {
            // Center is 0. Left is -laneDistance. Right is +laneDistance.
            _targetX = (_currentLane - 1) * laneDistance;
        }

        private void HandleLaneMovement()
        {
            if (GameManager.Instance == null) return;
            
            // Smoothly lerp towards target X (lane position)
            Vector3 currentPos = transform.position;
            float newX = Mathf.Lerp(currentPos.x, _targetX, Time.deltaTime * laneChangeSpeed);
            
            // CRITICAL FIX: Add forward movement using game speed
            float forwardSpeed = GameManager.Instance.CurrentSpeed;
            Vector3 moveVector = new Vector3(
                newX - currentPos.x,  // Lateral movement
                0,                     // No vertical
                forwardSpeed * Time.deltaTime  // Forward movement
            );
            
            _characterController.Move(moveVector);
        }

        public bool IsBraking { get; private set; }

        private void Brake()
        {
            if (_canBrake)
            {
                StartCoroutine(BrakeRoutine());
            }
        }

        private IEnumerator BrakeRoutine()
        {
            _canBrake = false;
            IsBraking = true; // Set the flag
            Time.timeScale = brakeTimeScale;
            
            yield return new WaitForSecondsRealtime(brakeDuration);
            
            Time.timeScale = 1f;
            IsBraking = false; // Reset the flag
            
            yield return new WaitForSeconds(brakeCooldown);
            _canBrake = true;
        }

        private void Jump()
        {
            if (!_isJumping && !_isCrouching && _characterController.isGrounded)
            {
                StartCoroutine(JumpRoutine());
            }
        }

        private IEnumerator JumpRoutine()
        {
            _isJumping = true;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            
            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / jumpDuration;
                float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                
                Vector3 newPos = transform.position;
                newPos.y = startPos.y + height;
                transform.position = newPos;
                
                yield return null;
            }
            
            // Return to ground
            Vector3 finalPos = transform.position;
            finalPos.y = startPos.y;
            transform.position = finalPos;
            
            _isJumping = false;
        }

        private void Crouch()
        {
            if (!_isJumping && !_isCrouching)
            {
                StartCoroutine(CrouchRoutine());
            }
        }

        private IEnumerator CrouchRoutine()
        {
            _isCrouching = true;
            
            // Shrink character controller
            _characterController.height = crouchHeight;
            _characterController.center = new Vector3(0, crouchHeight / 2, 0);
            
            yield return new WaitForSeconds(crouchDuration);
            
            // Return to normal
            _characterController.height = _normalHeight;
            _characterController.center = _normalCenter;
            
            _isCrouching = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isInvulnerable) return;

            if (other.CompareTag("Obstacle"))
            {
                Crash();
            }
            else if (other.CompareTag("SignalBar"))
            {
                if (!IsBraking)
                {
                    Crash();
                }
            }
            else if (other.CompareTag("Collectible"))
            {
                CollectOrb(other.gameObject);
            }
        }

        public void ActivateShield(float duration)
        {
            StartCoroutine(ShieldRoutine(duration));
        }

        private IEnumerator ShieldRoutine(float duration)
        {
            _isInvulnerable = true;
            // Visual feedback (e.g. change color)
            yield return new WaitForSeconds(duration);
            _isInvulnerable = false;
        }

        private void CollectOrb(GameObject orb)
        {
            GameManager.Instance.AddEnergy(1);
            // In a real pool, we return it. In this prototype, we might just disable it.
            // Assuming the orb script handles its own visual "pop" or we do it here.
            ObjectPooler.Instance.ReturnToPool(orb); 
            // Also trigger sound/particles
        }

        private void Crash()
        {
            if (crashParticles != null) crashParticles.Play();
            GameManager.Instance.ResetCombo();
            GameManager.Instance.GameOver();
        }
    }
}

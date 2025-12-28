using UnityEngine;
using System.Collections;
using HyperloopDash.Managers;
using HyperloopDash.Helpers;

namespace HyperloopDash.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float laneDistance = 3.0f; // Distance between lanes
        public float laneChangeSpeed = 10f;
        public float jumpForce = 5f; // Not used in this design but good to have stub
        
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

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            // Subscribe to Inputs
            SwipeInput.OnSwipeLeft += MoveLeft;
            SwipeInput.OnSwipeRight += MoveRight;
            SwipeInput.OnSwipeDown += Brake;
        }

        private void OnDestroy()
        {
            SwipeInput.OnSwipeLeft -= MoveLeft;
            SwipeInput.OnSwipeRight -= MoveRight;
            SwipeInput.OnSwipeDown -= Brake;
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
            // Smoothly lerp towards target X
            Vector3 currentPos = transform.position;
            float newX = Mathf.Lerp(currentPos.x, _targetX, Time.deltaTime * laneChangeSpeed);
            
            Vector3 moveVector = new Vector3(newX - currentPos.x, 0, 0);
            
            // Apply simple gravity if needed, though we are sliding on ground
            // For now, keep Y locked to 0 unless we add Jump
            
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
            IsBraking = true;
            Time.timeScale = brakeTimeScale;
            // Also notify UI of cooldown start if needed
            
            yield return new WaitForSecondsRealtime(brakeDuration);
            
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                Time.timeScale = 1.0f;
            }
            IsBraking = false;

            yield return new WaitForSeconds(brakeCooldown);
            _canBrake = true;
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

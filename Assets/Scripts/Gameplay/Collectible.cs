using UnityEngine;
using HyperloopDash.Managers;

namespace HyperloopDash.Gameplay
{
    public class Collectible : MonoBehaviour
    {
        public float rotateSpeed = 90f;
        private Transform _playerTransform;

        private void Start()
        {
            var p = FindObjectOfType<PlayerController>();
            if (p) _playerTransform = p.transform;
        }

        private void Update()
        {
            // Rotate for visual effect
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            // Recycle if missed
            if (_playerTransform != null && transform.position.z < _playerTransform.position.z - 10f)
            {
                ObjectPooler.Instance.ReturnToPool(gameObject);
            }
        }
    }
}

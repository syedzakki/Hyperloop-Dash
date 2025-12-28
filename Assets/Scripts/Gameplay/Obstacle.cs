using UnityEngine;
using HyperloopDash.Managers;

namespace HyperloopDash.Gameplay
{
    public class Obstacle : MonoBehaviour
    {
        // Simple distinct logic if needed, but mostly just a collider holder.
        // We might want to disable it once it's far behind the player.
        
        private Transform _playerTransform;
        
        private void Start()
        {
             _playerTransform = FindObjectOfType<PlayerController>().transform; // Cache this
        }

        private void Update()
        {
            if (_playerTransform != null && transform.position.z < _playerTransform.position.z - 20f)
            {
                ObjectPooler.Instance.ReturnToPool(gameObject);
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using HyperloopDash.Managers;

namespace HyperloopDash.Gameplay
{
    public class TrackManager : MonoBehaviour
    {
        public GameObject[] tunnelPrefabs;
        public float segmentLength = 20f;
        public int initialSegments = 10;
        public Transform playerTransform;

        private float _spawnZ = 0f;
        private float _safeZone = 30f;
        
        // Just keeping track of active segments implies we need a list if we want to recycle them customly,
        // OR we just use the ObjectPooler. Let's use ObjectPooler.
        // We assume "TunnelSegment" is a tag in the pooler.
        
        private void Start()
        {
            for (int i = 0; i < initialSegments; i++)
            {
                SpawnSegment();
            }
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (playerTransform.position.z - _safeZone > (_spawnZ - (initialSegments * segmentLength)))
                {
                    SpawnSegment();
                    // We rely on segments to disable themselves when far behind, 
                    // OR we handle it here. 
                    // For simplicity, let's let segments handle their own return or stick to simple spawning.
                    // Actually, for a tunnel, it's better to manage the queue here to ensure continuity.
                }
            }
        }

        private void SpawnSegment()
        {
            // Simple round robin or random segment
            GameObject go = ObjectPooler.Instance.SpawnFromPool("TunnelSegment", Vector3.forward * _spawnZ, Quaternion.identity);
            if(go != null) 
            {
                // In case it was pooled, we ensure it's at the right z
                go.transform.position = Vector3.forward * _spawnZ;
            }
            _spawnZ += segmentLength;
        }
    }
}

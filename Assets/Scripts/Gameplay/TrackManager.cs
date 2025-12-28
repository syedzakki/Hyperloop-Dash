using UnityEngine;
using System.Collections.Generic;
using HyperloopDash.Managers;

namespace HyperloopDash.Gameplay
{
    public class TrackManager : MonoBehaviour
    {
        public GameObject[] tunnelPrefabs;
        public float segmentLength = 20f;
        public int activeSegments = 5;
        public Transform playerTransform;

        private float _spawnZ = 0f;
        private List<GameObject> _activeSegments = new List<GameObject>();
        
        private void Start()
        {
            // Spawn initial segments
            for (int i = 0; i < activeSegments; i++)
            {
                SpawnSegment();
            }
            Debug.Log($"TrackManager: Spawned {activeSegments} initial segments");
        }

        private void Update()
        {
            if (GameManager.Instance == null || playerTransform == null) return;
            
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                // Spawn new segment when player moves forward
                float playerZ = playerTransform.position.z;
                float spawnThreshold = _spawnZ - (activeSegments * segmentLength);
                
                if (playerZ > spawnThreshold)
                {
                    SpawnSegment();
                    RecycleOldSegments();
                }
            }
        }

        private void SpawnSegment()
        {
            GameObject segment = ObjectPooler.Instance.SpawnFromPool("TunnelSegment", Vector3.forward * _spawnZ, Quaternion.identity);
            if (segment != null)
            {
                segment.transform.position = Vector3.forward * _spawnZ;
                _activeSegments.Add(segment);
                _spawnZ += segmentLength;
            }
        }

        private void RecycleOldSegments()
        {
            if (playerTransform == null) return;
            
            // Remove segments that are far behind the player
            float recycleDistance = segmentLength * 2;
            _activeSegments.RemoveAll(segment =>
            {
                if (segment != null && segment.transform.position.z < playerTransform.position.z - recycleDistance)
                {
                    ObjectPooler.Instance.ReturnToPool(segment);
                    return true;
                }
                return false;
            });
        }
    }
}

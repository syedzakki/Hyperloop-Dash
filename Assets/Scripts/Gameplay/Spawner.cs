using UnityEngine;
using HyperloopDash.Managers;

namespace HyperloopDash.Gameplay
{
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public float spawnDistance = 100f; // Distance from player to spawn
        public float minSpawnInterval = 1.0f; // Seconds
        public float maxSpawnInterval = 2.0f;

        [Header("Weights")]
        public float obstacleChance = 0.7f;
        public float coinChance = 0.3f;

        public Transform playerTransform;

        private float _nextSpawnZ;
        private float _lastSpawnZ;
        
        // We will spawn based on Z distance intervals rather than time to prevent bunching up when speed increases
        private float _spawnIntervalDistance = 20f; // Base distance between things

        private void Start()
        {
            _nextSpawnZ = 30f; // First spawn
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            // Check if we need to spawn something ahead
            if (playerTransform.position.z + spawnDistance > _nextSpawnZ)
            {
                SpawnRandomElement();
                
                // Calculate next spawn Z
                // As speed increases, we might want to increase distance slightly to give reaction time,
                // OR we keep it constant so it gets harder. Let's keep it constant constant for "Harder".
                _nextSpawnZ += Random.Range(20f, 40f); 
            }
        }

        private void SpawnRandomElement()
        {
            float roll = Random.value;

            if (roll < obstacleChance)
            {
                SpawnObstacle();
            }
            else
            {
                SpawnCoin();
            }
        }

        private void SpawnObstacle()
        {
            // Pick a random lane: -3, 0, 3 (Assuming lane width is 3)
            // But some obstacles take multiple lanes.
            // Let's assume tags: "Obstacle_Single", "Obstacle_Blocker" (2 lanes), "Obstacle_Bar"
            
            string[] obstacleTags = { "Obstacle_Blocker", "Obstacle_Bar", "Obstacle_Debris" };
            string tag = obstacleTags[Random.Range(0, obstacleTags.Length)];

            // Position calculation
            // For Blocker/Bar, we usually center them or pick a random lane based on logic.
            // For simplicity in this generated code, we'll spawn at center Z and let them have internal offset logic 
            // OR pick a random lane for single block debris.
            
            float xPos = 0;
            if (tag == "Obstacle_Debris")
            {
                int lane = Random.Range(0, 3); // 0, 1, 2
                xPos = (lane - 1) * 3.0f;
            }

            ObjectPooler.Instance.SpawnFromPool(tag, new Vector3(xPos, 0.5f, _nextSpawnZ), Quaternion.identity);
        }

        private void SpawnCoin()
        {
            // Spawn 3 coins in a row or just 1
            int lane = Random.Range(0, 3);
            float xPos = (lane - 1) * 3.0f;
            
            for (int i = 0; i < 3; i++)
            {
                ObjectPooler.Instance.SpawnFromPool("EnergyOrb", new Vector3(xPos, 1f, _nextSpawnZ + (i * 2f)), Quaternion.identity);
            }
            // Advance next spawn Z a bit more because we spawned a chain
            _nextSpawnZ += 5f; 
        }
    }
}

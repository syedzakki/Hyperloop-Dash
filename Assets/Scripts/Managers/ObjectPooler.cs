using UnityEngine;
using System.Collections.Generic;

namespace HyperloopDash.Managers
{
    [System.Serializable]
    public class PoolItem
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;

        public List<PoolItem> items;
        public Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (PoolItem item in items)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < item.size; i++)
                {
                    GameObject obj = Instantiate(item.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform); // Clean hierarchy
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(item.tag, objectPool);
            }
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }
        
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            // No need to manually enqueue as the Queue rotates. 
            // However, typical simple pools often Re-enqueue on return and Dequeue on spawn.
            // Our SpawnFromPool simply rotates the queue (Dequeue then Enqueue). 
            // So "ReturnToPool" is just visual disabling in this specific implementation style 
            // to keep it O(1) and avoid searching.
            // If we needed dynamic resizing, we'd need a different approach.
        }
    }
}

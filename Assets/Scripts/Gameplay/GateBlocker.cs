using UnityEngine;

namespace HyperloopDash.Gameplay
{
    public class GateBlocker : MonoBehaviour
    {
        // Expects child objects named "Left", "Center", "Right" 
        // OR simply 3 children at indices 0, 1, 2 representing lanes.
        
        private void OnEnable()
        {
            // Randomly block 1 or 2 lanes
            // We assume the prefab has 3 children blocks, initially all active or inactive.
            
            int blockedCount = Random.Range(1, 3); // 1 or 2
            
            // First disable all
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            // Random shuffle indices
            int[] lanes = { 0, 1, 2 };
            Shuffle(lanes);

            // Enable first N
            for (int i = 0; i < blockedCount; i++)
            {
                 int laneIndex = lanes[i];
                 if (laneIndex < transform.childCount)
                 {
                     transform.GetChild(laneIndex).gameObject.SetActive(true);
                 }
            }
        }

        private void Shuffle(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int temp = array[i];
                int r = Random.Range(i, array.Length);
                array[i] = array[r];
                array[r] = temp;
            }
        }
    }
}

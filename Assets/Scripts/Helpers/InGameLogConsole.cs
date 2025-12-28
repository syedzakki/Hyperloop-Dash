using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HyperloopDash.Helpers
{
    public class InGameLogConsole : MonoBehaviour
    {
        public Text logText;
        public int maxLines = 15;
        private Queue<string> queue = new Queue<string>();

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            Debug.Log("--- DEBUG CONSOLE STARTED ---");
            Debug.Log($"Screen: {Screen.width}x{Screen.height}, DPI: {Screen.dpi}");
            Debug.Log($"Touch Support: {Input.touchSupported}");
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            string color = type == LogType.Error || type == LogType.Exception ? "red" : "white";
            string entry = $"<color={color}>[{type}] {logString}</color>";
            
            queue.Enqueue(entry);
            if (queue.Count > maxLines) queue.Dequeue();

            logText.text = string.Join("\n", queue);
        }
        
        void Update()
        {
            // Retry with R key
            if (Input.GetKeyDown(KeyCode.R) || (Input.touchCount >= 3))
            {
                Debug.Log("RETRY - Reloading scene...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            
            // Also visualize input touches
            if (Input.touchCount > 0)
            {
               // Debug.Log($"Touch: {Input.GetTouch(0).phase} at {Input.GetTouch(0).position}");
            }
        }
    }
}

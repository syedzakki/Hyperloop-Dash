using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro is used, fallback to standard Text if needed but TMPro is standard now.
using HyperloopDash.Managers;

namespace HyperloopDash.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject hudPanel;
        public GameObject gameOverPanel;

        [Header("HUD Elements")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI energyText;
        public TextMeshProUGUI comboText;

        [Header("Game Over Elements")]
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI bestScoreText;
        public Button reviveButton;
        public Button doubleEnergyButton;

        private void Start()
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreUpdated += UpdateScore;
            GameManager.Instance.OnEnergyUpdated += UpdateEnergy;
            GameManager.Instance.OnComboUpdated += UpdateCombo;

            // Initialize Button Listeners
            // Note: These should be linked in Inspector or here. 
            // For code completeness, we assign them if not null.
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                GameManager.Instance.OnScoreUpdated -= UpdateScore;
                GameManager.Instance.OnEnergyUpdated -= UpdateEnergy;
                GameManager.Instance.OnComboUpdated -= UpdateCombo;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            mainMenuPanel.SetActive(state == GameState.MainMenu);
            hudPanel.SetActive(state == GameState.Playing);
            gameOverPanel.SetActive(state == GameState.GameOver);

            if (state == GameState.GameOver)
            {
                SetupGameOver();
            }
        }

        private void UpdateScore(int score)
        {
            scoreText.text = $"SCORE: {score}";
        }

        private void UpdateEnergy(int energy)
        {
            energyText.text = $"{energy}";
        }

        private void UpdateCombo(int combo)
        {
            comboText.text = $"x{combo}";
            comboText.gameObject.SetActive(combo > 1);
        }

        private void SetupGameOver()
        {
            finalScoreText.text = $"SCORE: {GameManager.Instance.Score:F0}";
            bestScoreText.text = $"BEST: {PlayerPrefs.GetInt("BestScore", 0)}";
            
            // Logic to enable/disable Revive button if already used could go here
            reviveButton.interactable = true; 
        }

        // Button Callbacks (Link these in Inspector)

        public void OnPlayClicked()
        {
            GameManager.Instance.StartGame();
        }

        public void OnRestartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void OnReviveClicked()
        {
            // Call AdManager -> On Success -> GameManager.Revive
            // For prototype:
            Debug.Log("Show Revive Ad...");
            // Mock success:
            GameManager.Instance.RevivePlayer();
            reviveButton.interactable = false; // Only once
        }

        public void OnDoubleEnergyClicked()
        {
            // Call AdManager -> On Success -> GameManager.RewardDouble
            Debug.Log("Show Double Energy Ad...");
            GameManager.Instance.RewardDoubleEnergy();
            doubleEnergyButton.interactable = false;
        }
    }
}

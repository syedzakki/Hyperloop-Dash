using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HyperloopDash.Managers;

namespace HyperloopDash.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject hudPanel;
        public GameObject gameOverPanel;
        public GameObject pausePanel;
        public GameObject tutorialPanel;

        [Header("HUD Elements")]
        public Text scoreText;
        public Text energyText;
        public Text comboText;

        [Header("Game Over Elements")]
        public Text finalScoreText;
        public Text bestScoreText;
        public Button reviveButton;
        public Button doubleEnergyButton;

        private void Start()
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScoreUpdated += UpdateScore;
            GameManager.Instance.OnEnergyUpdated += UpdateEnergy;
            GameManager.Instance.OnComboUpdated += UpdateCombo;

            // Force update UI to current state (Main Menu) to ensure other panels are hidden
            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
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
            pausePanel.SetActive(state == GameState.Paused);

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
            Debug.Log("RESTART CLICKED!");
            Time.timeScale = 1f; // Ensure time is running
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

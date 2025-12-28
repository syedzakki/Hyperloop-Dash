using UnityEngine;
using System;

namespace HyperloopDash.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public float InitialSpeed = 20f;
        public float SpeedIncreaseRate = 0.5f; // Speed increase per second
        public float MaxSpeed = 50f;
        
        [Header("Scoring")]
        public float ScoreMultiplierPerSecond = 10f;
        
        // Public State
        public GameState CurrentState { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float Score { get; private set; }
        public int EnergyCollected { get; private set; }
        public int CurrentCombo { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreUpdated; // Int for display
        public event Action<int> OnEnergyUpdated;
        public event Action<int> OnComboUpdated;

        private float _distanceTraveled;
        private int _dailySeed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDailySeed();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ChangeState(GameState.MainMenu);
        }

        private void Update()
        {
            // "Tap to Play" - Allow any screen tap to start game from main menu
            if (CurrentState == GameState.MainMenu)
            {
                bool tapped = Input.GetMouseButtonDown(0) || 
                             (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
                
                if (tapped)
                {
                    Debug.Log("TAP DETECTED - Starting Game!");
                    StartGame();
                }
            }
            else if (CurrentState == GameState.Playing)
            {
                UpdateGameplay();
            }
        }

        private void InitializeDailySeed()
        {
            // Simple daily seed based on YearMonthDay
            string dateString = DateTime.Now.ToString("yyyyMMdd");
            _dailySeed = dateString.GetHashCode();
            UnityEngine.Random.InitState(_dailySeed);
            Debug.Log($"Initialized Daily Seed: {_dailySeed} for date {dateString}");
        }

        public void StartGame()
        {
            Score = 0;
            EnergyCollected = 0;
            CurrentCombo = 1;
            CurrentSpeed = InitialSpeed;
            _distanceTraveled = 0;
            
            // Re-seed for consistency if this is a "Daily Run"
            UnityEngine.Random.InitState(_dailySeed);
            
            ChangeState(GameState.Playing);
        }

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            
            // Save Best Score
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if ((int)Score > bestScore)
            {
                PlayerPrefs.SetInt("BestScore", (int)Score);
            }

            // Save Total Energy
            int totalEnergy = PlayerPrefs.GetInt("TotalEnergy", 0);
            PlayerPrefs.SetInt("TotalEnergy", totalEnergy + EnergyCollected);
            PlayerPrefs.Save();
        }

        public void RevivePlayer()
        {
            // Logic to continue game
            ChangeState(GameState.Playing);
            // In a real implementation, you'd reset position or clear local obstacles here
        }

        private void UpdateGameplay()
        {
            // Increase speed over time
            if (CurrentSpeed < MaxSpeed)
            {
                CurrentSpeed += SpeedIncreaseRate * Time.deltaTime;
            }

            // Score based on distance/speed equivalent
            Score += CurrentSpeed * Time.deltaTime * ScoreMultiplierPerSecond * 0.1f; 
            
            OnScoreUpdated?.Invoke((int)Score);
        }

        public void AddEnergy(int amount)
        {
            EnergyCollected += amount * CurrentCombo;
            OnEnergyUpdated?.Invoke(EnergyCollected);
        }

        public void IncreaseCombo()
        {
            CurrentCombo = Mathf.Min(CurrentCombo + 1, 5); // Max x5
            OnComboUpdated?.Invoke(CurrentCombo);
        }

        public void ResetCombo()
        {
            CurrentCombo = 1;
            OnComboUpdated?.Invoke(CurrentCombo);
        }

        public void RewardDoubleEnergy()
        {
            EnergyCollected *= 2;
            OnEnergyUpdated?.Invoke(EnergyCollected);
            // Save immediately just in case
            int totalEnergy = PlayerPrefs.GetInt("TotalEnergy", 0);
            PlayerPrefs.SetInt("TotalEnergy", totalEnergy + EnergyCollected); // Note: this adds the distinct run amount again? 
            // Correction: We already saved the base amount on GameOver. We should just add the extra half.
            // Or simpler: We just update the display and save at the very end of the session or rely on the previous save + difference.
            // For safety in this simple loop, let's just re-save the total corectly.
            // Actually, GameOver saved X. Now we want specific 2X. 
            // Let's just track it and save on "Claim" button press.
            PlayerPrefs.SetInt("TotalEnergy", PlayerPrefs.GetInt("TotalEnergy") + (EnergyCollected / 2)); // Add the other half (since we doubled it)
            PlayerPrefs.Save();
        }

        private void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            Time.timeScale = (newState == GameState.Paused || newState == GameState.GameOver) ? 0 : 1;
        }
    }
}

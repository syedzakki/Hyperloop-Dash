using UnityEngine;
using System;

// Wrapper for Google Mobile Ads logic
// If SDK is present, uncomment appropriate lines or wrap in #if GOOGLE_MOBILE_ADS

namespace HyperloopDash.Ads
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance;

        [Header("Ad Unit IDs (Android)")]
        public string appId = "ca-app-pub-3940256099942544~3347511713"; // Test App ID
        public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test Rewarded ID

        private Action _onUserEarnedReward;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAds();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAds()
        {
            Debug.Log("Initializing AdMob...");
            // MobileAds.Initialize(initStatus => { });
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            // Clean up old ad before loading new one
            /*
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            AdRequest adRequest = new AdRequest();
            
            RewardedAd.Load(rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load: " + error);
                        return;
                    }

                    _rewardedAd = ad;
                    RegisterEventHandlers(ad);
                });
            */
            Debug.Log("AdMob: Loading Rewarded Ad Mock...");
        }

        public void ShowRewardedAd(Action onRewardHelper)
        {
            _onUserEarnedReward = onRewardHelper;

            // if (_rewardedAd != null && _rewardedAd.CanShowAd())
            // {
            //    _rewardedAd.Show((Reward reward) =>
            //    {
            //        Debug.Log("User Earned Reward!");
            //        _onUserEarnedReward?.Invoke();
            //    });
            // }
            // else
            // {
            //    Debug.Log("Rewarded ad not ready.");
            //    LoadRewardedAd(); // Try loading again
            // }
            
            // Mock for now:
            Debug.Log("AdMob: Showing Ad Mock...");
            _onUserEarnedReward?.Invoke();
        }

        // Setup UMP (Stubs)
        // public void ShowConsentForm() { ... }
    }
}

using UnityEngine;
using System;

namespace HyperloopDash.Helpers
{
    public class SwipeInput : MonoBehaviour
    {
        public static event Action OnSwipeLeft;
        public static event Action OnSwipeRight;
        public static event Action OnSwipeDown;
        public static event Action OnSwipeUp; // Not used but good to have

        [SerializeField] private float minSwipeDistance = 50f;

        private Vector2 _fingerDown;
        private Vector2 _fingerUp;

        private void Update()
        {
            // Unity Editor / PC Inputs
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) OnSwipeLeft?.Invoke();
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) OnSwipeRight?.Invoke();
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) OnSwipeDown?.Invoke();
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) OnSwipeUp?.Invoke();
#endif

            // Touch Inputs
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _fingerDown = touch.position;
                    _fingerUp = touch.position;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    _fingerUp = touch.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    _fingerUp = touch.position;
                    DetectSwipe();
                }
            }
        }

        private void DetectSwipe()
        {
            if (Vector2.Distance(_fingerDown, _fingerUp) > minSwipeDistance)
            {
                float xDiff = _fingerUp.x - _fingerDown.x;
                float yDiff = _fingerUp.y - _fingerDown.y;

                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    // Horizontal Swipe
                    if (xDiff > 0) OnSwipeRight?.Invoke();
                    else OnSwipeLeft?.Invoke();
                }
                else
                {
                    // Vertical Swipe
                    if (yDiff > 0) OnSwipeUp?.Invoke();
                    else OnSwipeDown?.Invoke();
                }
            }
        }
    }
}

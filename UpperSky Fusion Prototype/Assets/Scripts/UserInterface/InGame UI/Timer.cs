using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Custom_UI.InGame_UI
{
    public class Timer : MonoBehaviour
    {
        private GameManager _gameManager;
        
        [Required()] public TextMeshProUGUI timerTMP;

        private float _currentTime;

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (!_gameManager.gameIsStarted || _gameManager.gameIsFinished) return;
            
            _currentTime += Time.deltaTime;
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            int minutes = Mathf.FloorToInt(_currentTime / 60f);
            int seconds = Mathf.FloorToInt(_currentTime % 60f);
            timerTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
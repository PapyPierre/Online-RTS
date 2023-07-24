using UnityEngine;
using UnityEngine.UI;

namespace Custom_UI.InGame_UI
{
    public class ProductionBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        
        private float _time;
        private float _speed;

        private bool _init;

        public void Init(float productionTime)
        {
            _time = productionTime;       
            _speed = slider.maxValue / _time;
            _init = true;
        }
        
        private void Update()
        {
            if (slider.value >= slider.maxValue)
            {
                _init = false;
                // Building finished
            }
            
            if (_init) slider.value += Time.deltaTime * _speed;
        }
    }
}
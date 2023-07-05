using UnityEngine;
using UnityEngine.UI;

namespace Custom_UI.InGame_UI
{
    public class StatBar : MonoBehaviour
    {
          private Slider _slider;
         [SerializeField] private float updateSpeed = 2;
         private float _targetValue;

         public void Init(float maxValue)
         {
             _slider = GetComponent<Slider>();
             _slider.maxValue = maxValue;
             _slider.value = maxValue;
             UpdateBar(maxValue);
         }
         
         public void UpdateBar(float currentValue)
         {
             _targetValue = currentValue;
         }

         private void Update()
         {
             _slider.value = Mathf.MoveTowards(_slider.value, _targetValue, Time.deltaTime * updateSpeed);
         }
    }
}

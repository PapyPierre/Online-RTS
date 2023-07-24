using UnityEngine;
using UnityEngine.UI;

namespace Custom_UI.InGame_UI
{
    public class StatBar : MonoBehaviour
    {
         public Slider Slider { get; private set; }
         [SerializeField] private float updateSpeed = 2;
         private float _targetValue;

         public void Init(float maxValue)
         {
             Slider = GetComponent<Slider>();
             Slider.maxValue = maxValue;
             Slider.value = maxValue;
             UpdateBar(maxValue);
         }
         
         public void UpdateBar(float currentValue)
         {
             _targetValue = currentValue;
         }

         private void Update()
         {
             Slider.value = Mathf.MoveTowards(Slider.value, _targetValue, Time.deltaTime * updateSpeed);
         }
    }
}

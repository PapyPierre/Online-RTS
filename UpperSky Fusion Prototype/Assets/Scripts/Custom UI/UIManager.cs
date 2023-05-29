using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        [Required()] public TextMeshProUGUI connectionInfoTMP;
        [Required()] public TextMeshProUGUI materialsTMP;
        [Required()] public TextMeshProUGUI orichalcTMP;
        [Required()] public TextMeshProUGUI supplyTMP;
        
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(name);
                return;
            }

            instance = this;
        }

        public void UpdateMaterialsTMP(int newValue)
        {
            materialsTMP.text = newValue.ToString();
        }
        
        public void UpdateOrichalcTMP(int newValue)
        {
            orichalcTMP.text = newValue.ToString();
        }
        
        public void UpdateSupplyTMP(int newCurrentValue, int newMaxValue)
        {
            supplyTMP.text = newCurrentValue + "/" + newMaxValue;
        }
    }
}

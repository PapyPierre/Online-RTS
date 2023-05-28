using TMPro;
using UnityEngine;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        public TextMeshProUGUI connectionInfoTMP;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(name);
                return;
            }

            instance = this;
        }
    }
}

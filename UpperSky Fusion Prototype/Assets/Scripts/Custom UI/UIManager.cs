using Fusion;
using NaughtyAttributes;
using Network_Logic;
using TMPro;
using UnityEngine;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private NetworkManager _networkManager;
        
        [Header("Main Menu Variables"), Required()] public GameObject mainMenu;
        [SerializeField, Required()] private TMP_InputField inputFieldRoomName;
        [Required()] public TextMeshProUGUI connectionInfoTMP;

        [Header("In Game Variables"), Required()] public GameObject ressourcesLayout;
        [Required()] public TextMeshProUGUI materialsTMP;
        [Required()] public TextMeshProUGUI orichalcTMP;
        [Required()] public TextMeshProUGUI supplyTMP;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _networkManager = NetworkManager.instance;
        }

        #region MainMenu Functions
        public void StartGameAsHost()
        {
            if (inputFieldRoomName.text.Length > 0)
            {
                _networkManager.StartGame(GameMode.Host, inputFieldRoomName.text);
            }
            else
            {
                Debug.Log("Need a room name");
            }
        }

        public void StartGameAsClient()
        { 
            if (inputFieldRoomName.text.Length > 0)
            {
                _networkManager.StartGame(GameMode.Client, inputFieldRoomName.text);
            }
            else
            {
                Debug.Log("Need a room name");
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
        #endregion

        #region InGame Functions
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
        #endregion
    }
}

using Fusion;
using NaughtyAttributes;
using Network;
using TMPro;
using UnityEngine;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private NetworkManager _networkManager;
        
        [Header("Main Menu Variables"), Required()] 
        public GameObject mainMenu;
        [SerializeField, Required()] private TMP_InputField inputFieldRoomName;
        [Required()] public TextMeshProUGUI connectionInfoTMP;

        [Header("In Game Variables"), Required()]
        public GameObject inGameUI;
        [Required()] public TextMeshProUGUI materialsTMP;
        [Required()] public TextMeshProUGUI orichalcTMP;
        [Required()] public TextMeshProUGUI supplyTMP;

        [SerializeField, Required()] private GameObject buildMenu;
        [SerializeField, Required()] private GameObject techMenu;

        [SerializeField, Required()] private GameObject infoboxBuilding;
        [SerializeField, Required()] private TextMeshProUGUI infoboxBuildingName;
        [SerializeField, Required()] private TextMeshProUGUI infoboxBuildingDescription;

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
            _networkManager = NetworkManager.Instance;
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
        public void ShowOrHideBuildMenu() => buildMenu.SetActive(!buildMenu.activeSelf);
        public void ShowOrHideTechMenu() => techMenu.SetActive(!techMenu.activeSelf);

        public void ShowInfoboxBuilding(string buildingName, string buildingDescription)
        {
            infoboxBuilding.SetActive(true);
            infoboxBuildingName.text = buildingName;
            infoboxBuildingDescription.text = buildingDescription;
        }
        
        public void HideInfoboxBuilding() => infoboxBuilding.SetActive(false);

        public void Build(int buildingIndex) // See EntityManager enum "AllBuildings" to know all buildings index
        {
            switch (buildingIndex)
            {
                case 0 :  
                    Debug.Log("Exploitation d'orichalque");
                    break;
                case 1 : 
                    Debug.Log("Habitation");
                    break;
                case 2 : 
                    Debug.Log("Foreuse");
                    break;
                case 4 : 
                    Debug.Log("Menuiserie");
                    break;
                case 8 : 
                    Debug.Log("Baliste");
                    break;
            }
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
        #endregion
    }
}

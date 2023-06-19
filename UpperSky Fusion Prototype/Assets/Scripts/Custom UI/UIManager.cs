using Buildings;
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

        [SerializeField, Required()] private GameObject buildingInfobox;
        [SerializeField, Required()] private TextMeshProUGUI infoboxName;
        [SerializeField, Required()] private TextMeshProUGUI infoboxDescr;
        [SerializeField, Required()] private TextMeshProUGUI infoboxMatCost;
        [SerializeField, Required()] private TextMeshProUGUI infoboxOriCost;

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

        public void ShowInfoboxBuilding(BuildingData buildingData, bool isLocked)
        {
            buildingInfobox.SetActive(true);
            infoboxName.text = buildingData.Name;
            infoboxDescr.text = isLocked ? buildingData.LockedDescription : buildingData.Description;
            infoboxMatCost.text = buildingData.MaterialCost.ToString();
            infoboxOriCost.text = buildingData.OrichalqueCost.ToString();

            infoboxMatCost.color = 
                _networkManager.thisPlayer.ressources.CurrentMaterials
                >= buildingData.MaterialCost ? Color.white : Color.red;

            infoboxOriCost.color = 
                _networkManager.thisPlayer.ressources.CurrentOrichalque 
                >= buildingData.OrichalqueCost ? Color.white : Color.red;
        }
        
        public void HideInfoboxBuilding() => buildingInfobox.SetActive(false);

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

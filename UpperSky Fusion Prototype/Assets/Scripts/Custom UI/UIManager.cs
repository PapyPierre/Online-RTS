using System;
using Buildings;
using Custom_UI.InGame_UI;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Network;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private NetworkManager _networkManager;
        private UnitsManager _unitsManager;
        private BuildingsManager _buildingsManager;
        
        [Header("Main Menu Variables"), Required()] 
        public GameObject mainMenu;
        [SerializeField, Required()] private TMP_InputField inputFieldRoomName;
        [Required()] public TextMeshProUGUI connectionInfoTMP;

        [Header("In Game Variables"), Required()]
        public GameObject inGameUI;
        
        [Required(), Space] public TextMeshProUGUI materialsTMP;
        [Required()] public TextMeshProUGUI orichalqueTMP;
        [Required()] public TextMeshProUGUI supplyTMP;

        [SerializeField, Required(), Space] private GameObject buildMenu;
        [SerializeField, Required()] private GameObject techMenu;
        [SerializeField, Required()] private GameObject formationMenu;
        [SerializeField, Required()] private GameObject formationQueue;

        [SerializeField, Space] private UnitsIcon[] unitsIconsInMenu;

        [SerializeField, Required(), Space] private GameObject infobox;
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
            _unitsManager = UnitsManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
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
        public void ShowOrHideFormationMenu(BuildingsManager.AllBuildingsEnum formationBuiling)
        {
            formationMenu.SetActive(!formationMenu.activeSelf);
            
            foreach (var unitsIcon in unitsIconsInMenu) unitsIcon.gameObject.SetActive(false);
            
            for (var i = 0; i < _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits.Length; i++)
            {          
                unitsIconsInMenu[i].gameObject.SetActive(true);

                var unit = _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits[i];
                        
                unitsIconsInMenu[i].data = _unitsManager.allUnitsData[(int) unit];
                unitsIconsInMenu[i].GetComponent<Image>().sprite = _unitsManager.allUnitsData[(int) unit].Icon;
            }
        }
        
        public void ShowOrHideFormationQueue() => formationQueue.SetActive(!formationQueue.activeSelf);

        public void ShowInfobox(EntityData entityData, bool isLocked)
        {
            infobox.SetActive(true);
            infoboxName.text = entityData.Name;
            infoboxDescr.text = isLocked ? entityData.LockedDescription : entityData.Description;
            
            infoboxMatCost.text = entityData.MaterialCost.ToString();
            infoboxOriCost.text = entityData.OrichalqueCost.ToString();

            infoboxMatCost.color = 
                _networkManager.thisPlayer.ressources.CurrentMaterials
                >= entityData.MaterialCost ? Color.white : Color.red;

            infoboxOriCost.color = 
                _networkManager.thisPlayer.ressources.CurrentOrichalque 
                >= entityData.OrichalqueCost ? Color.white : Color.red;
        }

        public void HideInfobox() => infobox.SetActive(false);

        public void UpdateMaterialsTMP(int newValue)
        {
            materialsTMP.text = newValue.ToString();
        }
        
        public void UpdateOrichalqueTMP(int newValue)
        {
            orichalqueTMP.text = newValue.ToString();
        }
        
        public void UpdateSupplyTMP(int newCurrentValue, int newMaxValue)
        {
            supplyTMP.text = newCurrentValue + "/" + newMaxValue;
        }
        #endregion
    }
}

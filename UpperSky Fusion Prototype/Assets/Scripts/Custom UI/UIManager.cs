using Custom_UI.InGame_UI;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Network;
using TMPro;
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
        [SerializeField] private Image[] unitsQueueImages;
        private BaseBuilding _currentlyOpenFormationBuilding;

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

        public void HideOpenedUI()
        {
           ShowOrHideBuildMenu(false);   
           ShowOrHideTechMenu(false);   
           ShowOrHideFormationMenu(false);
           ShowOrHideFormationQueue(false);
           HideInfobox();
        }
        
        public void ShowOrHideBuildMenu(bool active) => buildMenu.SetActive(active);
        
        public void ShowOrHideTechMenu(bool active) => techMenu.SetActive(active);

        public void OpenFormationBuilding(BuildingsManager.AllBuildingsEnum formationBuiling,
            BaseBuilding buildingInstance)
        {
            _currentlyOpenFormationBuilding = buildingInstance;
            ShowOrHideFormationMenu(true, formationBuiling);
            ShowOrHideFormationQueue(true);
        }
        
        private void ShowOrHideFormationMenu(bool active, BuildingsManager.AllBuildingsEnum formationBuiling = 0)
        {
            formationMenu.SetActive(active);

            if (!active)
            {
                _currentlyOpenFormationBuilding = null;
                return;
            }

            foreach (var unitsIcon in unitsIconsInMenu) unitsIcon.gameObject.SetActive(false);
            
            for (var i = 0; i < _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits.Length; i++)
            {          
                unitsIconsInMenu[i].gameObject.SetActive(true);

                var unit = _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits[i];
                        
                unitsIconsInMenu[i].data = _unitsManager.allUnitsData[(int) unit];
                unitsIconsInMenu[i].GetComponent<Image>().sprite = _unitsManager.allUnitsData[(int) unit].Icon;
            }
        }
        
        private void ShowOrHideFormationQueue(bool active)
        { 
            formationQueue.SetActive(active);
            if (active) UpdateFormationQueueDisplay();
        } 
        
        // Call from inspector
        public void AddUnitToFormationQueue(int buttonIndex)
        {
            if (_currentlyOpenFormationBuilding is null)
            {
                Debug.LogError("didn't find building to form unit");
                return;
            }

            int formationQueueCurrentCount = _currentlyOpenFormationBuilding.FormationQueue.Count;

            if (formationQueueCurrentCount < 5) // 5 because there is 5 slots in a formation queue
            {
                var unit = _currentlyOpenFormationBuilding.Data.FormableUnits[buttonIndex];
                _currentlyOpenFormationBuilding.FormationQueue.Enqueue(unit);
                UpdateFormationQueueDisplay();
            }
        }

        private void UpdateFormationQueueDisplay()
        {
            if (_currentlyOpenFormationBuilding is null)
            {
                Debug.LogError("didn't find building to update formation queue display");
                return;
            }
            
            for (int i = 0; i < _currentlyOpenFormationBuilding.FormationQueue.Count; i++)
            {
                var queueCopy = _currentlyOpenFormationBuilding.FormationQueue.ToArray();
                
                unitsQueueImages[i].sprite = _unitsManager.allUnitsData[(int) queueCopy[i]].Icon;
                
                //TODO faire une class custom "FormationQueue" pour éviter de devoir faire une array temporaire ici (conseil de jacques)
            }
        }

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
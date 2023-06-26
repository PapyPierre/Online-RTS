using Custom_UI.InGame_UI;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Network;
using Player;
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
        [SerializeField, Required()] private GameObject formationMenu;
        [SerializeField, Required()] private GameObject formationQueue;

        [SerializeField, Space] private UnitsIcon[] unitsIconsInMenu;
        [SerializeField] private Image[] unitsQueueImages;
        [SerializeField] private Slider formationQueueSlider;
        public BaseBuilding CurrentlyOpenFormationBuilding { get; private set; }

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
           ShowOrHideBuildMenu();
           ShowOrHideFormationMenu(false);
           ShowOrHideFormationQueue(false);
           HideInfobox();
        }

        public void ShowOrHideBuildMenu()
        {
            buildMenu.SetActive(!buildMenu.activeSelf);

            if (buildMenu.activeSelf)
            {
                CurrentlyOpenFormationBuilding = null;
                ShowOrHideFormationMenu(false);
                ShowOrHideFormationQueue(false);
                HideInfobox();
            }
        }

        public void OpenFormationBuilding(BuildingsManager.AllBuildingsEnum formationBuiling,
            BaseBuilding buildingInstance)
        {
            CurrentlyOpenFormationBuilding = buildingInstance;
            ShowOrHideFormationMenu(true, formationBuiling);
            ShowOrHideFormationQueue(true);
        }
        
        private void ShowOrHideFormationMenu(bool active, BuildingsManager.AllBuildingsEnum formationBuiling = 0)
        {
            formationMenu.SetActive(active);

            if (!active)
            {
                CurrentlyOpenFormationBuilding = null;
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
            if (CurrentlyOpenFormationBuilding is null)
            {
                Debug.LogError("didn't find building to form unit");
                return;
            }
            
            PlayerController player = _networkManager.thisPlayer;
                
            var playerCurrentMat = player.ressources.CurrentMaterials;
            var playerCurrentOri = player.ressources.CurrentOrichalque;
            var playerCurrentSupply = player.ressources.CurrentSupply;
            var playerCurrentMaxSupply = player.ressources.CurrentMaxSupply;
            
            var unit = CurrentlyOpenFormationBuilding.Data.FormableUnits[buttonIndex];

            var unitMatCost = _unitsManager.allUnitsData[(int) unit].MaterialCost;
            var unitOriCost =_unitsManager.allUnitsData[(int) unit].OrichalqueCost;
            var unitSupplyCost = _unitsManager.allUnitsData[(int) unit].SupplyCost;

            if (unitSupplyCost + playerCurrentSupply > playerCurrentMaxSupply)
            {
                Debug.Log("not enough available supplies");
                return;
            }
            
            // Check if player have enough ressources to build this building
            if (playerCurrentMat >= unitMatCost && playerCurrentOri >= unitOriCost)
            {
                int formationQueueCurrentCount = CurrentlyOpenFormationBuilding.FormationQueue.Count;

                if (formationQueueCurrentCount < 5) // 5 because there is 5 slots in a formation queue
                {
                    player.ressources.CurrentMaterials -= unitMatCost;
                    player.ressources.CurrentOrichalque -= unitOriCost;
                    player.ressources.CurrentSupply += unitSupplyCost;
                    
                    CurrentlyOpenFormationBuilding.FormationQueue.Enqueue(unit);
                    if (CurrentlyOpenFormationBuilding.FormationQueue.Count is 1)
                    {
                        CurrentlyOpenFormationBuilding.timeLeftToForm =
                            _unitsManager.allUnitsData[(int) unit].ProductionTime;

                    }
                    UpdateFormationQueueDisplay();
                }
                else Debug.Log("Queue is full");
            }
            else Debug.Log("not enough ressources");
        }

        public void UpdateFormationQueueDisplay()
        {
            if (CurrentlyOpenFormationBuilding is null) return;
            
            foreach (var image in unitsQueueImages)
            {
                image.sprite = null; //TODO mettre un sprite par default
            }
            
            for (int i = 0; i < CurrentlyOpenFormationBuilding.FormationQueue.Count; i++)
            {
                var queueCopy = CurrentlyOpenFormationBuilding.FormationQueue.ToArray();
                
                unitsQueueImages[i].sprite = _unitsManager.allUnitsData[(int) queueCopy[i]].Icon;
                
                //TODO faire une class custom "FormationQueue" pour éviter de devoir faire une array temporaire ici (conseil de jacques)
            }
            
            if (CurrentlyOpenFormationBuilding.FormationQueue.Count > 0)
            {
                CurrentlyOpenFormationBuilding.UpdateFormationQueueSliderWithNewValue();
            }
            else
            {
                UpdateFormationQueueSlider(0);
            }
        }

        public void UpdateFormationQueueSlider(float newValue)
        {
            switch (newValue)
            {
                case < 0 or > 1:
                    Debug.LogError(newValue + "is not in range of " + formationQueueSlider.name);
                    return;
                case 0:
                    formationQueueSlider.value = 0;
                    break;
                default:
                {
                    var currentValue = formationQueueSlider.value;
                    formationQueueSlider.value = Mathf.Lerp(currentValue, newValue, 0.5f);
                    break;
                }
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
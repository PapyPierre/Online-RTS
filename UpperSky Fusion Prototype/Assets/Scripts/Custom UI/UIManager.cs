using System.Collections;
using Custom_UI.InGame_UI;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using NaughtyAttributes;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Custom_UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private UnitsManager _unitsManager;
        private BuildingsManager _buildingsManager;
        private GameManager _gameManager;
        
        [Header("Main Menu Variables"), Required()] 
        public GameObject mainMenu;
        [Required()] public GameObject menuCamera;
        [SerializeField, Required()] private TMP_InputField inputFieldRoomName;
        [Required()] public TextMeshProUGUI connectionInfoTMP;
        private int _fps;
        private double _ping;
        [Required()] public TextMeshProUGUI fpsTMP;
        [Required()] public TextMeshProUGUI pingTMP;
        [SerializeField, Required()] private PlayerSpawner playerSpawner;

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
        public BaseBuilding CurrentlyOpenBuilding { get; private set; }

        [SerializeField, Required(), Space] private GameObject infobox;
        [SerializeField, Required()] private TextMeshProUGUI infoboxName;
        [SerializeField, Required()] private TextMeshProUGUI infoboxDescr;
        [SerializeField, Required()] private TextMeshProUGUI infoboxMatCost;
        [SerializeField, Required()] private TextMeshProUGUI infoboxOriCost;

        [SerializeField, Required(), Space] private GameObject endGamePanel;
        [SerializeField, Required()] private TextMeshProUGUI winTMP;
        [SerializeField, Required()] private TextMeshProUGUI loseTMP;

        [Header("Loading Screen")]
        [Required()] public GameObject loadingScreen;
        [SerializeField, Required()] private TextMeshProUGUI loadingText;
        private int _loadingTextDots;
        
        [Header("Other Variables"), Required()]
        public GameObject floatingText;
        
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
            _unitsManager = UnitsManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
            _gameManager = GameManager.Instance;

            StartCoroutine(UpdateInfoDisplay());
        }

        private void Update()
        {
            ComputeFps();
            if (_gameManager.gameIsStarted) ComputePlayerPing();
        }
        
        private IEnumerator UpdateInfoDisplay()
        {
            while (true)
            {
                DisplayFps();
                if (_gameManager.gameIsStarted) DisplayPlayerPing();
                
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private void ComputeFps() => _fps = Mathf.RoundToInt(1.0f / Time.deltaTime);
        
        private void ComputePlayerPing()
        { 
            _ping = _gameManager.thisPlayer.Runner.GetPlayerRtt(_gameManager.thisPlayer.Object.StateAuthority);
        }

        private void DisplayFps()
        {
            fpsTMP.text = _fps + " FPS";

            switch (_fps)
            {
                case < 15:
                    fpsTMP.color = Color.red;
                    break;
                case >= 15 and < 24:
                    fpsTMP.color = Color.red + Color.yellow;
                    break;   
                case >= 24 and < 30:
                    fpsTMP.color = Color.yellow;
                    break;
                case >= 30 and < 60:
                    fpsTMP.color = Color.yellow + Color.green;
                    break;
                case >= 60:
                    fpsTMP.color = Color.green;
                    break;
            }
        }
        
        private void DisplayPlayerPing()
        {
            pingTMP.text = Mathf.RoundToInt((float) _ping) + " ms";

            switch (_ping)
            {
                case < 15:
                    pingTMP.color = Color.green;
                    break;
                case >= 15 and < 40:
                    pingTMP.color = Color.green + Color.yellow;
                    break;   
                case >= 40 and < 80:
                    pingTMP.color = Color.yellow;
                    break;
                case >= 80 and < 130:
                    pingTMP.color = Color.yellow + Color.red;
                    break;
                case >= 130:
                    pingTMP.color = Color.red;
                    break;
            }
        }

        #region MainMenu Functions
        public void JoinGame()
        {
            if (inputFieldRoomName.text.Length > 0)
            {
                playerSpawner.SpawnPlayers(inputFieldRoomName.text);
                loadingScreen.SetActive(true);
                UpdateLoadingText(false);
                mainMenu.SetActive(false);
                inGameUI.SetActive(true);
            }
            else
            {
                Debug.Log("Need a room name");
            }
        }

        public void QuitGame() => Application.Quit();
        #endregion

        #region InGame Functions

        public void HideOpenedUI()
        { 
            if (buildMenu.activeSelf) ShowOrHideBuildMenu();
           ShowOrHideFormationMenu(false);
           ShowOrHideFormationQueue(false);
           HideInfobox();
        }

        public void ShowOrHideBuildMenu()
        {
            buildMenu.SetActive(!buildMenu.activeSelf);

            if (buildMenu.activeSelf)
            {
                CloseCurrentlyOpenBuilding();
                ShowOrHideFormationMenu(false);
                ShowOrHideFormationQueue(false);
                HideInfobox();
            }
        }

        public void OpenFormationBuilding(BuildingsManager.AllBuildingsEnum formationBuiling,
            BaseBuilding buildingInstance)
        {
            CurrentlyOpenBuilding = buildingInstance;
            ShowOrHideFormationMenu(true, formationBuiling);
            ShowOrHideFormationQueue(true);
        }
        
        private void ShowOrHideFormationMenu(bool active, BuildingsManager.AllBuildingsEnum formationBuiling = 0)
        {
            formationMenu.SetActive(active);

            if (!active)
            {
                CloseCurrentlyOpenBuilding();
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

        private void CloseCurrentlyOpenBuilding()
        {
            if (CurrentlyOpenBuilding is null) return;
            
            CurrentlyOpenBuilding.isOpen = false;
            CurrentlyOpenBuilding.SetActiveSelectionCircle(false);
            CurrentlyOpenBuilding = null;
        }
        
        // Call from inspector
        public void AddUnitToFormationQueue(int buttonIndex)
        {
            if (CurrentlyOpenBuilding is null)
            {
                Debug.LogError("didn't find building to form unit");
                return;
            }
            
            PlayerController player = _gameManager.thisPlayer;
                
            var playerCurrentMat = player.ressources.CurrentMaterials;
            var playerCurrentOri = player.ressources.CurrentOrichalque;
            var playerCurrentSupply = player.ressources.CurrentSupply;
            var playerCurrentMaxSupply = player.ressources.CurrentMaxSupply;
            
            var unit = CurrentlyOpenBuilding.Data.FormableUnits[buttonIndex];

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
                int formationQueueCurrentCount = CurrentlyOpenBuilding.FormationQueue.Count;

                if (formationQueueCurrentCount < 5) // 5 because there is 5 slots in a formation queue
                {
                    player.ressources.CurrentMaterials -= unitMatCost;
                    player.ressources.CurrentOrichalque -= unitOriCost;
                    player.ressources.CurrentSupply += unitSupplyCost;
                    
                    CurrentlyOpenBuilding.FormationQueue.Enqueue(unit);
                    if (CurrentlyOpenBuilding.FormationQueue.Count is 1)
                    {
                        CurrentlyOpenBuilding.timeLeftToForm =
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
            if (CurrentlyOpenBuilding is null) return;
            
            foreach (var image in unitsQueueImages)
            {
                image.sprite = null; //TODO mettre un sprite par default
            }
            
            for (int i = 0; i < CurrentlyOpenBuilding.FormationQueue.Count; i++)
            {
                var queueCopy = CurrentlyOpenBuilding.FormationQueue.ToArray();
                
                unitsQueueImages[i].sprite = _unitsManager.allUnitsData[(int) queueCopy[i]].Icon;
                
                //TODO faire une class custom "FormationQueue" pour éviter de devoir faire une array temporaire ici (conseil de jacques)
            }
            
            if (CurrentlyOpenBuilding.FormationQueue.Count > 0)
            {
                CurrentlyOpenBuilding.UpdateFormationQueueSliderWithNewValue();
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
                _gameManager.thisPlayer.ressources.CurrentMaterials
                >= entityData.MaterialCost ? Color.white : Color.red;

            infoboxOriCost.color = 
                _gameManager.thisPlayer.ressources.CurrentOrichalque 
                >= entityData.OrichalqueCost ? Color.white : Color.red;
        }

        public void HideInfobox() => infobox.SetActive(false);

        public void UpdateMaterialsTMP(int newValue) => materialsTMP.text = newValue.ToString();

        public void UpdateOrichalqueTMP(int newValue) => orichalqueTMP.text = newValue.ToString();
        
        public void UpdateSupplyTMP(int newCurrentValue, int newMaxValue)
        {
            supplyTMP.text = newCurrentValue + "/" + newMaxValue;
        }

        public void ShowWinText()
        {
            endGamePanel.SetActive(true);
            winTMP.gameObject.SetActive(true);
        }
        
        public void ShowLoseText()
        {
            endGamePanel.SetActive(true);
            loseTMP.gameObject.SetActive(true);
        }

        public void UpdateLoadingText(bool isLoading)
        {
            if (_gameManager.gameIsStarted) return;
           
            if (!isLoading)
            {
                loadingText.text =  "Connected players : " + _gameManager.connectedPlayers.Count + "/" 
                                    + _gameManager.expectedNumberOfPlayers;
            }
            else
            {
                loadingText.text = "                    Loading"; // Space is normal
                for (int i = 0; i < _loadingTextDots; i++) loadingText.text += ".";
                StartCoroutine(WaitToAddDot());
            }
        }

        private IEnumerator WaitToAddDot()
        {
            yield return new WaitForSeconds(0.5f);
            if (_loadingTextDots < 3) _loadingTextDots++;
            else _loadingTextDots = 0;
            UpdateLoadingText(true);
        }

        public void PopFloatingText(Transform parent, string text, Color color)
        {
            TextMeshPro infoText = Instantiate(floatingText, parent.position, Quaternion.identity, parent)
                .GetComponent<TextMeshPro>();
            
            infoText.text = text;
            infoText.color = color;
        }
        #endregion
    }
}
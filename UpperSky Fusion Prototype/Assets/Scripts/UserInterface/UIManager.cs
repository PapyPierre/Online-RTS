using System.Collections;
using System.Globalization;
using Custom_UI.InGame_UI;
using Element;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using NaughtyAttributes;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private UnitsManager _unitsManager;
        private BuildingsManager _buildingsManager;
        private GameManager _gameManager;
        [HideInInspector] public PlayerRessources playerRessources;
        
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
        
        public GameObject inGameUI;
        
        [SerializeField, Space] private TextMeshProUGUI[] ressourcesTMP;
         
        [Header("In Game Menu")]
        [SerializeField, Required()] private GameObject buildMenu;
        [SerializeField, Required()] private GameObject formationMenu;
        [SerializeField, Required()] private GameObject formationQueue;

        [Header("Units Formation")]
        [SerializeField] private UnitsIcon[] unitsIconsInMenu;
        [SerializeField] private Image[] unitsQueueImages;
        [SerializeField] private Slider formationQueueSlider;
        public BaseBuilding CurrentlyOpenBuilding { get; private set; }

        [Header("Production Infobox")]
        [SerializeField, Required()] private GameObject prodInfobox;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxName;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxDescr;
        [SerializeField] private GameObject[] prodInfoboxRessourcesObj;
        [SerializeField] private TextMeshProUGUI[] prodInfoboxRessourcesCostTMP;
        
        [Header("In Game Infobox")]
        [SerializeField, Required()] private GameObject inGameInfobox;
        [SerializeField, Required()] private TextMeshProUGUI inGameInfoboxName;
        [SerializeField, Required()] private TextMeshProUGUI inGameInfoboxDescr;
        [SerializeField] private GameObject[] inGameInfoboxStatsObj;
        [SerializeField] private TextMeshProUGUI[] inGameInfoboxStatsTMP;
        [SerializeField, Required()] private Image inGameInfoboxEntityIcon;
        [SerializeField, Required()] private Image inGameInfoboxEntityColor; // Color of owner
        [SerializeField, Required()] private Image inGameInfoboxEntitySkill;
        [HideInInspector] private BaseElement.ElementType _shownElementType;

        [Header("End Game")]
        [SerializeField, Required()] private GameObject endGamePanel;
        [SerializeField, Required()] private TextMeshProUGUI winTMP;
        [SerializeField, Required()] private TextMeshProUGUI loseTMP;

        [Header("Loading Screen")]
        [Required()] public GameObject loadingScreen;
        [SerializeField, Required()] private TextMeshProUGUI loadingText;
        private int _loadingTextDots;
        
        [Header("Pop Ups"), Required()]
        public GameObject floatingText;
        [SerializeField] private GameObject underAttackPopUp;
        [SerializeField] private float underAttackPopUpLivingTime;
        private float _underAttackPopUpCurrentTime;
        [SerializeField] private GameObject attackMinimapPopup;
        
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
            HideUnderAttackPopUp();
        }
        
        private IEnumerator UpdateInfoDisplay()
        {
            while (true)
            {
                DisplayFps();
                if (_gameManager.gameIsStarted)
                {
                    DisplayPlayerPing();
                    UpdateWoodTMP(playerRessources.CurrentWood);
                    UpdateMetalsTMP(playerRessources.CurrentMetals);
                    UpdateOrichalqueTMP(playerRessources.CurrentOrichalque);
                }
                
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
           HideProdInfobox();
           HideInGameInfoBox();
        }

        public void ShowOrHideBuildMenu()
        {
            buildMenu.SetActive(!buildMenu.activeSelf);

            if (buildMenu.activeSelf)
            {
                CloseCurrentlyOpenBuilding();
                ShowOrHideFormationMenu(false);
                ShowOrHideFormationQueue(false);
                HideProdInfobox();
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
            if (active)
            {
                HideInGameInfoBox();
                UpdateFormationQueueDisplay();
            }
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

            var unit = CurrentlyOpenBuilding.Data.FormableUnits[buttonIndex];

            var unitWoodCost = _unitsManager.allUnitsData[(int) unit].WoodCost;
            var unitMetalsCost = _unitsManager.allUnitsData[(int) unit].MetalsCost;
            var unitOriCost =_unitsManager.allUnitsData[(int) unit].OrichalqueCost;
            var unitSupplyCost = _unitsManager.allUnitsData[(int) unit].SupplyCost;

            if (unitSupplyCost + player.ressources.CurrentSupply > player.ressources.CurrentMaxSupply)
            {
                Debug.Log("not enough available supplies");
                return;
            }
            
            // Check if player have enough ressources to build this building
            if (player.ressources.CurrentWood >= unitWoodCost 
                && player.ressources.CurrentMetals >= unitMetalsCost 
                && player.ressources.CurrentOrichalque >= unitOriCost)
            {
                int formationQueueCurrentCount = CurrentlyOpenBuilding.FormationQueue.Count;

                if (formationQueueCurrentCount < 5) // 5 because there is 5 slots in a formation queue
                {
                    player.ressources.CurrentWood -= unitWoodCost;
                    player.ressources.CurrentMetals -= unitMetalsCost;
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

        public void ShowProdInfobox(EntityData entityData, bool isLocked)
        {
            prodInfobox.SetActive(true);
            prodInfoboxName.text = entityData.Name;
            prodInfoboxDescr.text = isLocked ? entityData.LockedDescription : entityData.Description;

            if (entityData.WoodCost > 0)
            {
                prodInfoboxRessourcesObj[0].SetActive(true);
                prodInfoboxRessourcesCostTMP[0].text = entityData.WoodCost.ToString();

                prodInfoboxRessourcesCostTMP[0].color = 
                    playerRessources.CurrentWood >=  entityData.WoodCost ? Color.white : Color.red;
            }
            else prodInfoboxRessourcesObj[0].SetActive(false);

            
            if (entityData.MetalsCost > 0)
            {
                prodInfoboxRessourcesObj[1].SetActive(true);
                prodInfoboxRessourcesCostTMP[1].text = entityData.MetalsCost.ToString();
                
                prodInfoboxRessourcesCostTMP[1].color = 
                    playerRessources.CurrentMetals >=  entityData.MetalsCost ? Color.white : Color.red;
            }
            else prodInfoboxRessourcesObj[1].SetActive(false);

            if (entityData.OrichalqueCost > 0)
            {
                prodInfoboxRessourcesObj[2].SetActive(true);
                prodInfoboxRessourcesCostTMP[2].text = entityData.OrichalqueCost.ToString();
                
                prodInfoboxRessourcesCostTMP[2].color = 
                    playerRessources.CurrentOrichalque >= entityData.OrichalqueCost ? Color.white : Color.red;
            }
            else prodInfoboxRessourcesObj[2].SetActive(false);

            if (entityData is UnitData unitData)
            {
                prodInfoboxRessourcesObj[3].SetActive(true);
                prodInfoboxRessourcesCostTMP[3].text = unitData.SupplyCost.ToString();
                
                prodInfoboxRessourcesCostTMP[3].color = 
                    playerRessources.CurrentSupply + unitData.SupplyCost <= playerRessources.CurrentMaxSupply ? 
                        Color.white : Color.red;
            }
            else prodInfoboxRessourcesObj[3].SetActive(false);
        }

        public void HideProdInfobox() => prodInfobox.SetActive(false);

        public void ShowInGameInfoBox(ElementData elementData, PlayerController owner, int numberOfBuildings = 0)
        {
            if (CurrentlyOpenBuilding is not null) return;
            
            inGameInfobox.SetActive(true);
            inGameInfoboxName.text = elementData.Name;
            inGameInfoboxDescr.text = elementData.Description;

            inGameInfoboxEntityIcon.sprite = elementData.Icon;

            inGameInfoboxEntityColor.color = owner is null ? Color.white : owner.myColor;
            
            inGameInfoboxEntitySkill.gameObject.SetActive(false);

            if (elementData is EntityData entityData)
            {
               foreach (var go in inGameInfoboxStatsObj)
               {
                   go.SetActive(true);
               }
               
               // Nombre de bâtiments, visible que pour les iles
               inGameInfoboxStatsObj[^1].SetActive(false);
            
               inGameInfoboxStatsTMP[0].text = entityData.MaxHealthPoints.ToString();
               inGameInfoboxStatsTMP[1].text = entityData.MaxArmorPoints.ToString();
               inGameInfoboxStatsTMP[2].text = entityData.SightRange.ToString();
            
               if (entityData is UnitData unitData)
               {
                   _shownElementType = BaseElement.ElementType.Unit;
                   
                   inGameInfoboxStatsTMP[3].text = unitData.MovementSpeed.ToString(CultureInfo.CurrentCulture);
                   inGameInfoboxStatsTMP[4].text = unitData.WeatherResistance.ToString(CultureInfo.CurrentCulture);

                   if (unitData.CanShoot)
                   {
                       inGameInfoboxStatsTMP[5].text = unitData.DamagePerShoot.ToString();
                       inGameInfoboxStatsTMP[6].text = unitData.ArmorPenetration + "%";
                   }
                   else
                   {
                       inGameInfoboxStatsObj[5].SetActive(false);
                       inGameInfoboxStatsObj[6].SetActive(false);
                   }

                   if (unitData.Skill is not UnitsManager.UnitSkillsEnum.None)
                   {
                       inGameInfoboxEntitySkill.gameObject.SetActive(true);
                       inGameInfoboxEntitySkill.sprite =
                           _unitsManager.allUnitSkillsData[(int) unitData.Skill].SkillIcon;
                   }
               }
               else if (entityData is BuildingData)
               {
                   _shownElementType = BaseElement.ElementType.Building;

                   for (var index = 3; index < inGameInfoboxStatsObj.Length; index++)
                   {
                       inGameInfoboxStatsObj[index].SetActive(false);
                   }
               }
           }
           else if (elementData is IslandData islandData)
           {
               _shownElementType = BaseElement.ElementType.Island;

               foreach (var go in inGameInfoboxStatsObj)
               {
                   go.SetActive(false);
               }
               
               // Nombre de bâtiments, visible que pour les iles
               inGameInfoboxStatsObj[^1].SetActive(true);
               inGameInfoboxStatsTMP[^1].text = numberOfBuildings + "/" + islandData.MaxBuildingsOnThisIsland;
           }
        }
     
        public void HideInGameInfoBox()
        { 
            _shownElementType = BaseElement.ElementType.None;
            
            foreach (var go in inGameInfoboxStatsObj)
            {
                go.SetActive(false);
            }
            
            inGameInfobox.SetActive(false);
        }

        // Call from inspector
        public void CallToUseUnitSkill(int skillIndex) => _unitsManager.UseUnitSkill(skillIndex);
        

        public void UpdateWoodTMP(int newValue) => ressourcesTMP[0].text = newValue.ToString();
        
        public void UpdateMetalsTMP(int newValue) =>  ressourcesTMP[1].text = newValue.ToString();

        public void UpdateOrichalqueTMP(int newValue) =>  ressourcesTMP[2].text = newValue.ToString();
        
        public void UpdateSupplyTMP(int newCurrentValue, int newMaxValue)
        {
            ressourcesTMP[3].text = newCurrentValue + "/" + newMaxValue;
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

        public void ShowUnderAttackPopUp(Vector3 position)
        {
            underAttackPopUp.SetActive(true);
            _underAttackPopUpCurrentTime = underAttackPopUpLivingTime;

            // Bug popup non visible
            var obj = Instantiate(attackMinimapPopup, position, Quaternion.identity);
           Destroy(obj, underAttackPopUpLivingTime);
        }

        private void HideUnderAttackPopUp()
        {
            if (_underAttackPopUpCurrentTime < 0)
            {
                underAttackPopUp.SetActive(false);
                return;
            }
            
            _underAttackPopUpCurrentTime -= Time.deltaTime;
        } 
        
        #endregion
    }
}
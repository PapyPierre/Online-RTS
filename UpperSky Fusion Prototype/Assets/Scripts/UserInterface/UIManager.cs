using System;
using System.Collections;
using System.Globalization;
using Custom_UI.InGame_UI;
using Element;
using Element.Entity;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Entity;
using Entity.Buildings;
using Fusion;
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
        [SerializeField] private UnitsIcon[] unitsIconsInMenu;
        
        [Header("Win Conditions Panel")]
        [SerializeField, Required()] private GameObject winConditionPanel;
        [SerializeField] private WinCondition[] winConditions;
        
        [Serializable]
        private class WinCondition
        {
            public Image[] playersIcons;
            public Slider[] playersAdvanceSlider;
            [HideInInspector] public int _sliderMaxValue;
        }

        [Header("Build Button")]
        [SerializeField, Required()] private Image buildButtonImage;
        [SerializeField, Required()] private Sprite _buildButtonDefaultSprite;
        [SerializeField, Required()] private GameObject buildButtonFilter;
        [SerializeField, Required()] private Image buildButtonProgressionBar;
        private float currentlyBuiltBuildingProductionTime;
        private float progressionInBuilding;

        [Header("Production Infobox")]
        [SerializeField, Required()] private GameObject prodInfobox;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxName;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxDescr;
        [SerializeField] private GameObject[] prodInfoboxRessourcesObj;
        [SerializeField] private TextMeshProUGUI[] prodInfoboxRessourcesCostTMP;
        
        [Header("Selection Infobox")]
        [SerializeField, Required(), Space] private GameObject selectionInfobox;
        [SerializeField, Required()] private TextMeshProUGUI selectionInfobox_Name;
        [SerializeField, Required()] private TextMeshProUGUI selectionInfobox_Descr;
        
        [SerializeField, Space] private GameObject[] selectionInfobox_StatusBandelettes;
        
        [SerializeField, Required(), Space] private Animator selectionInfobox_StatsPanelAnimator;
        private static readonly int Open = Animator.StringToHash("open");
        private bool _isStatsPanelOpen;
        private bool IsStatsPanelOpen
        {
            get => _isStatsPanelOpen;
            
            set
            {
                _isStatsPanelOpen = value;
                if (value)
                {
                    selectionInfobox_StatsPanelAnimator.gameObject.SetActive(true);
                }
                selectionInfobox_StatsPanelAnimator.SetBool(Open, value);
            }
        }
        [SerializeField] private GameObject[] selectionInfobox_StatsObj;
        [SerializeField] private TextMeshProUGUI[] selectionInfobox_StatsTMP;
        
        [SerializeField, Required(), Space] private Image selectionInfobox_EntityIcon;
        
        [SerializeField, Space] private Image[] selectionInfobox_OwnerColorElements;
        
        [SerializeField, Space] private GameObject[] selectionInfobox_UnitSkillsBG;
        [SerializeField] private Button[] selectionInfobox_UnitsSkillsBtn;
        
        [SerializeField, Space] private Image[] unitsQueueImages;
        [SerializeField] private Sprite defaultUnitQueueSprite;
        [SerializeField] private Slider formationQueueSlider;
        
        [SerializeField, Required(), Space] private GameObject selectionInfoboxDestroyBtn;

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
            if (buildButtonFilter.activeSelf) ManageDisplayOnBuildingButton();
        }

        private void ManageDisplayOnBuildingButton()
        {
            progressionInBuilding += Time.deltaTime;

            if (progressionInBuilding > currentlyBuiltBuildingProductionTime)
            {
                progressionInBuilding = currentlyBuiltBuildingProductionTime;
            }

            buildButtonProgressionBar.fillAmount = progressionInBuilding / currentlyBuiltBuildingProductionTime;
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
        
        private void ComputePlayerPing() => _ping = _gameManager.thisPlayer.Runner.GetPlayerRtt(PlayerRef.None) * 1000;

        private void DisplayFps()
        {
            fpsTMP.text = _fps + " FPS";

            switch (_fps)
            {
                case < 30:
                    fpsTMP.color = Color.red;
                    break;
                case >= 30 and < 60:
                    fpsTMP.color = Color.yellow;
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
                case < 60:
                    pingTMP.color = Color.green;
                    break;
                case >= 60 and < 120:
                    pingTMP.color = Color.yellow;
                    break;
                case >= 120:
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

        public void DisplayOnBuilding(BuildingData currentlyBuiltBuildingData)
        {
            buildButtonFilter.SetActive(true);
            buildButtonImage.sprite = currentlyBuiltBuildingData.Icon;
            buildButtonProgressionBar.fillAmount = 0;
            progressionInBuilding = 0;
            currentlyBuiltBuildingProductionTime = currentlyBuiltBuildingData.ProductionTime;
        }

        public void OnFinishBuilding()
        {
            buildButtonFilter.SetActive(false);
            buildButtonImage.sprite = _buildButtonDefaultSprite;
        }

        public void UpdateRessourcesDisplay()
        {
            UpdateRessourceTMP(0, playerRessources.CurrentWood, playerRessources.CurrentWoodGain);
            UpdateRessourceTMP(1, playerRessources.CurrentMetals, playerRessources.CurrentMetalsGain);
            UpdateRessourceTMP(2, playerRessources.CurrentOrichalque, playerRessources.CurrentOrichalqueGain);
        }

        public void HideOpenedUI()
        { 
            HideBuildMenu();
            CloseFormationBuilding();
            HideProdInfobox();
            HideSelectionInfoBox();
            HideWinConditionPanel();
        }

        // Call from inspector
        public void ShowWinConditionPanel()
        {
            if (winConditionPanel.activeSelf) HideWinConditionPanel();
            else
            {
                winConditionPanel.SetActive(true);
                
                foreach (var winCondition in winConditions)
                {
                    foreach (var image in winCondition.playersIcons)
                    {
                       image.gameObject.SetActive(false);
                    }

                    foreach (var slider in winCondition.playersAdvanceSlider)
                    {
                        slider.gameObject.SetActive(false);
                    }

                    for (var index = 0; index < _gameManager.connectedPlayers.Count; index++)
                    {
                        PlayerController player = _gameManager.connectedPlayers[index];
                        winCondition.playersIcons[index].gameObject.SetActive(true);
                        winCondition.playersIcons[index].color = player.myColor;
                        winCondition.playersAdvanceSlider[index].fillRect.GetComponent<Image>().color = player.myColor;
                    }
                }

                winConditions[0]._sliderMaxValue = _gameManager.requiredNumberOfIslandToWin;
                
                for (var index = 0; index < _gameManager.connectedPlayers.Count; index++)
                {
                    PlayerController player = _gameManager.connectedPlayers[index];
                    winConditions[0].playersAdvanceSlider[index].gameObject.SetActive(true);
                    winConditions[0].playersAdvanceSlider[index].value = player.NumberOfControlledIslands;
                }
            }
        }

        public void HideWinConditionPanel()
        {
            winConditionPanel.SetActive(false);
        }

        public void ShowBuildMenu()
        {
            CloseFormationBuilding();
            buildMenu.SetActive(true);
        }

        public void HideBuildMenu()
        {
            buildMenu.SetActive(false);
            
            ShowOrHideFormationMenu(false);
            HideFormationQueue();
            HideProdInfobox();
        }

        public void OpenFormationBuilding(FormationBuilding formationBuiling)
        {
            ShowOrHideFormationMenu(true, formationBuiling.Data.ThisBuilding);
            ShowFormationQueue(formationBuiling);
        }

        public void CloseFormationBuilding()
        {
            ShowOrHideFormationMenu(false);
            HideFormationQueue();
        }
        
        private void ShowOrHideFormationMenu(bool active, BuildingsManager.AllBuildingsEnum formationBuiling = 0)
        {
            formationMenu.SetActive(active);

            foreach (var unitsIcon in unitsIconsInMenu) unitsIcon.gameObject.SetActive(false);
            
            for (var i = 0; i < _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits.Length; i++)
            {          
                unitsIconsInMenu[i].gameObject.SetActive(true);

                var unit = _buildingsManager.allBuildingsDatas[(int) formationBuiling].FormableUnits[i];
                        
                unitsIconsInMenu[i].data = _unitsManager.allUnitsData[(int) unit];
                unitsIconsInMenu[i].GetComponent<Image>().sprite = _unitsManager.allUnitsData[(int) unit].Icon;
            }
        }
        
        private void ShowFormationQueue(FormationBuilding building = null)
        { 
            selectionInfobox_Descr.gameObject.SetActive(false);
            
            foreach (var image in unitsQueueImages)
            {
                image.gameObject.SetActive(true);
            }
            
            UpdateFormationQueueDisplay(building);
        }

        private void HideFormationQueue()
        {
            selectionInfobox_Descr.gameObject.SetActive(true);
            
            foreach (var image in unitsQueueImages)
            {
                image.gameObject.SetActive(false);
            }
        }

        // Call from inspector
        public void CallToAddUnitToFormationQueue(int buttonIndex)
        {
            if (_gameManager.thisPlayer.lastSelectedElement is FormationBuilding formationBuilding)
            {
                formationBuilding.AddUnitToFormationQueue(formationBuilding.Data.FormableUnits[buttonIndex]);
            }
        }
        
        // Call from inspector
        public void CallToRemoveUnitFromFormationQueue(int buttonIndex)
        {
            if (_gameManager.thisPlayer.lastSelectedElement is FormationBuilding formationBuilding)
            {
                formationBuilding.RemoveUnitFromFormationQueue(buttonIndex);
            }
        }

        public void UpdateFormationQueueDisplay(FormationBuilding formationBuilding)
        {
            foreach (var image in unitsQueueImages)
            {
                image.sprite = defaultUnitQueueSprite; 
            }
            
            for (int i = 0; i < formationBuilding.FormationQueue.Count(); i++)
            {
                unitsQueueImages[i].sprite = 
                    _unitsManager.allUnitsData[(int) formationBuilding.FormationQueue.PeekAtGivenIndex(i)].Icon;
            }
            
            if (formationBuilding.FormationQueue.IsNotEmpty())
            {
                formationBuilding.UpdateFormationQueueSliderWithNewValue();
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

        public void ShowSelectionInfoBox(BaseElement element, ElementData elementData, PlayerController owner)
        {
            selectionInfobox.SetActive(true);
            selectionInfobox_StatsPanelAnimator.gameObject.SetActive(false);

            selectionInfobox_Name.text = elementData.Name;
            selectionInfobox_Descr.text = elementData.Description;

            selectionInfobox_EntityIcon.sprite = elementData.Icon;

            foreach (var image in selectionInfobox_OwnerColorElements)
            {
               image.color = owner is null ? Color.white : owner.myColor;
            }

            foreach (var go in selectionInfobox_UnitSkillsBG)
            {
                go.SetActive(false);
            }
            
            selectionInfoboxDestroyBtn.SetActive(true);

            if (element is BaseUnit unit)
            {
                for (int i = 0; i < selectionInfobox_StatusBandelettes.Length; i++)
                {
                    var statusAtIndex = UnitsManager.Instance.allUnitsStatusData[i].Status;
                    selectionInfobox_StatusBandelettes[i].SetActive(unit.currentStatusOnThisUnit.Contains(statusAtIndex));
                }

                var skills = unit.skills;
                for (var index = 0; index < skills.Length; index++)
                {
                    var skill = skills[index];
                    selectionInfobox_UnitSkillsBG[index].SetActive(true); 
                    selectionInfobox_UnitSkillsBG[index].GetComponent<Image>().sprite =  skill.Data.SkillIcon;
                    var skillImage = selectionInfobox_UnitsSkillsBtn[index].GetComponent<Image>();
                    skillImage.sprite = skill.Data.SkillIcon;
                    if (skill.isReady)
                    {
                        selectionInfobox_UnitsSkillsBtn[index].interactable = true;
                        skillImage.fillAmount = 1;
                    }
                    else
                    {
                        selectionInfobox_UnitsSkillsBtn[index].interactable = false;
                        skillImage.fillAmount = skill.cdCompletion;
                    }
                }
            }
            else if (elementData is IslandData)
            {
                selectionInfoboxDestroyBtn.SetActive(false);
            }
        }

        public void UpdateSelectionInfobox(BaseElement element, ElementData elementData, PlayerController owner)
        {
            if (_gameManager.thisPlayer.lastSelectedElement == element) ShowSelectionInfoBox(element, elementData, owner);
        }

        public void HideSelectionInfoBox()
        {
            foreach (var go in selectionInfobox_StatsObj)
            {
                go.SetActive(false);
            }
            
            selectionInfobox.SetActive(false);
            
            HideStatsPanel();
        }

        // Call from inspector
        public void InteractWithStatsPanel()
        {
            if (IsStatsPanelOpen) HideStatsPanel();
            else ShowStatsPanel();
        }
        
        private void ShowStatsPanel()
        {
            IsStatsPanelOpen = true;
            
            switch (_gameManager.thisPlayer.lastSelectedElement)
            {
                case BaseEntity entity:
                {
                    foreach (var go in selectionInfobox_StatsObj)
                    {
                        go.SetActive(true);
                    }
               
                    // Nombre de bâtiments, visible que pour les iles
                    selectionInfobox_StatsObj[^1].SetActive(false);
               
                    switch (entity)
                    {
                        case BaseUnit unit:
                        {
                            selectionInfobox_StatsTMP[0].text = unit.Data.MaxHealthPoints.ToString();
                            selectionInfobox_StatsTMP[1].text = unit.Data.MaxArmorPoints.ToString();
                            selectionInfobox_StatsTMP[2].text = unit.Data.SightRange.ToString();
                            selectionInfobox_StatsTMP[3].text = unit.Data.MovementSpeed.ToString(CultureInfo.CurrentCulture);
                            selectionInfobox_StatsTMP[4].text = unit.Data.WeatherResistance.ToString(CultureInfo.CurrentCulture);

                            if (unit.Data.ShootingMode == UnitsManager.ShootingMode.ShotByShot)
                            {
                                selectionInfobox_StatsTMP[5].text = unit.Data.DamagePerShoot.ToString();
                                selectionInfobox_StatsTMP[6].text = unit.Data.ArmorPenetration + "%";
                                selectionInfobox_StatsTMP[7].text = Mathf.RoundToInt(unit.Data.ShootingRange).ToString();

                            }
                            else
                            {
                                selectionInfobox_StatsObj[5].SetActive(false);
                                selectionInfobox_StatsObj[6].SetActive(false);
                            }

                            break;
                        }
                        case BaseBuilding building:
                        {
                            selectionInfobox_StatsTMP[0].text = building.Data.MaxHealthPoints.ToString();
                            selectionInfobox_StatsTMP[1].text = building.Data.MaxArmorPoints.ToString();
                            selectionInfobox_StatsTMP[2].text = building.Data.SightRange.ToString();
                   
                            for (var index = 3; index < selectionInfobox_StatsObj.Length; index++)
                            {
                                selectionInfobox_StatsObj[index].SetActive(false);
                            }

                            break;
                        }
                    }

                    break;
                }
                case BaseIsland island:
                {
                    foreach (var go in selectionInfobox_StatsObj)
                    {
                        go.SetActive(false);
                    }
               
                    // Nombre de bâtiments, visible que pour les iles
                    selectionInfobox_StatsObj[^1].SetActive(true);
                    selectionInfobox_StatsTMP[^1].text = island.BuildingsCount + "/" + 
                                                        island.data.MaxBuildingsOnThisIsland;
                    break;
                }
            }
        }

        private void HideStatsPanel() => IsStatsPanelOpen = false;
        
        // Call from inspector
        public void DestroySelectionInfoboxEntity()
        {
            switch (_gameManager.thisPlayer.lastSelectedElement)
            {
                case BaseUnit unit:
                    unit.DestroyEntity();
                    break;
                case BaseBuilding building:
                    building.DestroyEntity();
                    break;
                case BaseIsland island:
                    Debug.LogError("try to destroy island using selection infobox destroy btn");
                    break;
            }
            
            HideSelectionInfoBox();
        }

        private void UpdateRessourceTMP(int ressourceIndex, int currentRessource, float currentRessourceGain)
        {
            ressourcesTMP[ressourceIndex].text =
                currentRessource 
                + "<color=#EAEAEA><size=65%><voffset=0.185em> +" 
                + currentRessourceGain;
        }

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

        public void ShowUnderAttackPopUp(Vector3 position, BaseEntity entity)
        {
            underAttackPopUp.SetActive(true);
            _underAttackPopUpCurrentTime = underAttackPopUpLivingTime;
            
            var obj = Instantiate(attackMinimapPopup, position, Quaternion.Euler(90,0,0), entity.minimapCanvas);
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
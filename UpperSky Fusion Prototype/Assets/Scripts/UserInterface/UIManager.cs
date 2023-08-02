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
        [SerializeField] private UnitsIcon[] unitsIconsInMenu;

        [Header("Production Infobox")]
        [SerializeField, Required()] private GameObject prodInfobox;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxName;
        [SerializeField, Required()] private TextMeshProUGUI prodInfoboxDescr;
        [SerializeField] private GameObject[] prodInfoboxRessourcesObj;
        [SerializeField] private TextMeshProUGUI[] prodInfoboxRessourcesCostTMP;
        
        [Header("Selection Infobox")]
        [SerializeField, Required()] private GameObject selectionInfobox;
        [SerializeField, Required()] private TextMeshProUGUI selectionInfoboxName;
        [SerializeField, Required()] private TextMeshProUGUI selectionInfoboxDescr;
        [SerializeField] private GameObject[] selectionInfoboxStatsObj;
        [SerializeField] private TextMeshProUGUI[] selectionInfoboxStatsTMP;
        [SerializeField, Required()] private Image selectionInfoboxEntityIcon;
        [SerializeField, Required()] private Image selectionInfoboxEntityColor; // Color of owner
        [SerializeField, Required()] private GameObject selectionInfoboxEntitySkill;
        [SerializeField] private Image[] unitsQueueImages;
        [SerializeField] private Slider formationQueueSlider;
        [SerializeField, Required()] private GameObject selectionInfoboxDestroyBtn;

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
                if (_gameManager.gameIsStarted) DisplayPlayerPing();
                
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        public void UpdateRessourcesDisplay()
        {
            UpdateRessourceTMP(0, playerRessources.CurrentWood, playerRessources.CurrentWoodGain);
            UpdateRessourceTMP(1, playerRessources.CurrentMetals, playerRessources.CurrentMetalsGain);
            UpdateRessourceTMP(2, playerRessources.CurrentOrichalque, playerRessources.CurrentOrichalqueGain);
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

        public void HideOpenedUI()
        { 
            HideBuildMenu();
            ShowOrHideFormationMenu(false);
            ShowOrHideFormationQueue(false);
            HideProdInfobox();
            HideSelectionInfoBox();
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
            ShowOrHideFormationQueue(false);
            HideProdInfobox();
        }

        public void OpenFormationBuilding(BaseBuilding formationBuiling)
        {
            ShowOrHideFormationMenu(true, formationBuiling.Data.ThisBuilding);
            ShowOrHideFormationQueue(true, formationBuiling);
        }

        public void CloseFormationBuilding()
        {
            ShowOrHideFormationMenu(false);
            ShowOrHideFormationQueue(false);
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
        
        private void ShowOrHideFormationQueue(bool active, BaseBuilding building = null)
        { 
            selectionInfoboxDescr.gameObject.SetActive(!active);
            
            foreach (var image in unitsQueueImages)
            {
                image.gameObject.SetActive(active);
            }
            
            if (active) UpdateFormationQueueDisplay(building);
        }

        // Call from inspector
        public void AddUnitToFormationQueue(int buttonIndex)
        {
            if (_gameManager.thisPlayer.lastSelectedElement is BaseBuilding building)
            { 
                if (!building.Data.IsFormationBuilding)
                {
                    Debug.LogError("Building is not formation building");
                    return;
                }
            }
            else
            {
                Debug.LogError("Element is not a building");
                return;
            }


            BaseBuilding formationBuilding = _gameManager.thisPlayer.lastSelectedElement.GetComponent<BaseBuilding>();
            PlayerController player = _gameManager.thisPlayer;

            var unit = formationBuilding.Data.FormableUnits[buttonIndex];

            var unitWoodCost = _unitsManager.allUnitsData[(int) unit].WoodCost;
            var unitMetalsCost = _unitsManager.allUnitsData[(int) unit].MetalsCost;
            var unitOriCost =_unitsManager.allUnitsData[(int) unit].OrichalqueCost;

            // Check if player have enough ressources to build this building
            if (player.ressources.CurrentWood >= unitWoodCost 
                && player.ressources.CurrentMetals >= unitMetalsCost 
                && player.ressources.CurrentOrichalque >= unitOriCost)
            {
                int formationQueueCurrentCount = formationBuilding.FormationQueue.Count;

                if (formationQueueCurrentCount < 5) // 5 because there is 5 slots in a formation queue
                {
                    player.ressources.CurrentWood -= unitWoodCost;
                    player.ressources.CurrentMetals -= unitMetalsCost;
                    player.ressources.CurrentOrichalque -= unitOriCost;

                    formationBuilding.FormationQueue.Enqueue(unit);
                    if (formationBuilding.FormationQueue.Count is 1)
                    {
                        formationBuilding.timeLeftToForm =
                            _unitsManager.allUnitsData[(int) unit].ProductionTime;

                    }
                    UpdateFormationQueueDisplay(formationBuilding);
                }
                else Debug.Log("Queue is full");
            }
            else Debug.Log("not enough ressources");
        }

        public void UpdateFormationQueueDisplay(BaseBuilding formationBuilding)
        {
            foreach (var image in unitsQueueImages)
            {
                image.sprite = null; //TODO mettre un sprite par default
            }
            
            for (int i = 0; i < formationBuilding.FormationQueue.Count; i++)
            {
                var queueCopy = formationBuilding.FormationQueue.ToArray();
                
                unitsQueueImages[i].sprite = _unitsManager.allUnitsData[(int) queueCopy[i]].Icon;
                
                //TODO faire une class custom "FormationQueue" pour éviter de devoir faire une array temporaire ici (conseil de jacques)
            }
            
            if (formationBuilding.FormationQueue.Count > 0)
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

        public void ShowSelectionInfoBox(BaseElement element, ElementData elementData, PlayerController owner)
        {
            selectionInfobox.SetActive(true);

            selectionInfoboxName.text = elementData.Name;
            selectionInfoboxDescr.text = elementData.Description;

            selectionInfoboxEntityIcon.sprite = elementData.Icon;

            selectionInfoboxEntityColor.color = owner is null ? Color.white : owner.myColor;
            
            selectionInfoboxEntitySkill.SetActive(false);
            selectionInfoboxDestroyBtn.SetActive(true);

            if (elementData is EntityData entityData)
            {
               foreach (var go in selectionInfoboxStatsObj)
               {
                   go.SetActive(true);
               }
               
               // Nombre de bâtiments, visible que pour les iles
               selectionInfoboxStatsObj[^1].SetActive(false);
            
               selectionInfoboxStatsTMP[0].text = entityData.MaxHealthPoints.ToString();
               selectionInfoboxStatsTMP[1].text = entityData.MaxArmorPoints.ToString();
               selectionInfoboxStatsTMP[2].text = entityData.SightRange.ToString();
            
               if (entityData is UnitData unitData)
               {
                   selectionInfoboxStatsTMP[3].text = unitData.MovementSpeed.ToString(CultureInfo.CurrentCulture);
                   selectionInfoboxStatsTMP[4].text = unitData.WeatherResistance.ToString(CultureInfo.CurrentCulture);

                   if (unitData.CanShoot)
                   {
                       selectionInfoboxStatsTMP[5].text = unitData.DamagePerShoot.ToString();
                       selectionInfoboxStatsTMP[6].text = unitData.ArmorPenetration + "%";
                   }
                   else
                   {
                       selectionInfoboxStatsObj[5].SetActive(false);
                       selectionInfoboxStatsObj[6].SetActive(false);
                   }

                   if (unitData.SkillData.Skill is not UnitsManager.UnitSkillsEnum.None)
                   {
                       selectionInfoboxEntitySkill.SetActive(true);
                       selectionInfoboxEntitySkill.GetComponent<Image>().sprite = unitData.SkillData.SkillIcon;
                        

                       selectionInfoboxEntitySkill.GetComponent<Button>().interactable  = 
                           element.GetComponent<BaseUnit>().isSkillReady;
                   }
               }
               else if (entityData is BuildingData)
               {
                   for (var index = 3; index < selectionInfoboxStatsObj.Length; index++)
                   {
                       selectionInfoboxStatsObj[index].SetActive(false);
                   }
               }
            }
            else if (elementData is IslandData islandData)
            {
                selectionInfoboxDestroyBtn.SetActive(false);

                foreach (var go in selectionInfoboxStatsObj)
                {
                    go.SetActive(false);
                }
               
                // Nombre de bâtiments, visible que pour les iles
                selectionInfoboxStatsObj[^1].SetActive(true);
                selectionInfoboxStatsTMP[^1].text = element.GetComponent<BaseIsland>().BuildingsCount + "/" + islandData.MaxBuildingsOnThisIsland;
            }
        }

        public void UpdateSelectionInfobox(BaseElement element, ElementData elementData, PlayerController owner)
        {
            if (_gameManager.thisPlayer.lastSelectedElement == element) ShowSelectionInfoBox(element, elementData, owner);
        }

        public void HideSelectionInfoBox()
        {
            foreach (var go in selectionInfoboxStatsObj)
            {
                go.SetActive(false);
            }
            
            selectionInfobox.SetActive(false);
        }

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
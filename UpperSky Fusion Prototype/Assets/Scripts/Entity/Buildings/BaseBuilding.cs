using Custom_UI;
using Fusion;
using NaughtyAttributes;
using Network;
using UnityEngine;
using World.Island;

namespace Entity.Buildings
{
    public class BaseBuilding : NetworkBehaviour
    {
        [SerializeField, Expandable] private BuildingData data;
        private BuildingsManager _buildingsManager;
        private NetworkManager _networkManager;
        private UIManager _uiManager;

        private Island _myIsland;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

        private float _tempMatToGenerate;
        private float _tempOrichalqueToGenerate;

       private bool _mouseOverThisBuilding;
        
        private void Awake()
        {
            _maxHealth = data.MaxHealthPoints;
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        public override void Spawned()
        {
            _buildingsManager = BuildingsManager.Instance;
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
            
            var pos = transform.position;
            Ray ray = new Ray(new Vector3(pos.x, pos.y + 3, pos.z), -transform.up); 
            RaycastHit hit;

            Physics.Raycast(ray, out hit, 5000, _buildingsManager.terrainLayer);
            if (hit.collider is null)
            {
                Debug.LogError("Building didn't detect his island");
                return;
            }
            
            _myIsland = hit.collider.GetComponentInParent<Island>();

            if (!PlayerIsOwner()) return;
            
            if (data.UnlockedBuildings.Length > 0)
            {
                UnlockBuildings();
            }
            
            if (data.DoesGenerateRessources)
            {
                InvokeRepeating("GenerateRessourcesEachSecond", 1,1);
                
                //TODO eventually add a check for game max supply if there's one
                _myIsland.Owner.ressources.CurrentMaxSupply += data.AditionnalMaxSupplies;
            }
        }

        private void Update()
        {
            if (_mouseOverThisBuilding && data.IsFormationBuilding && PlayerIsOwner())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _uiManager.ShowOrHideFormationMenu(data.ThisBuilding);
                    _uiManager.ShowOrHideFormationQueue();
                }
            }
        }

        private bool PlayerIsOwner()
        {
            return _myIsland.Owner == _networkManager.thisPlayer;
        }

        private void UnlockBuildings()
        {
            foreach (var building in data.UnlockedBuildings)
            {
                _buildingsManager.allBuildingsIcons[(int) building].Unlock();
            }
        }
        
        private void GenerateRessourcesEachSecond()
        {
            _tempMatToGenerate += data.GeneratedMaterialPerSeconds;
            _tempOrichalqueToGenerate += data.GeneratedOrichalquePerSeconds;

            if (_tempMatToGenerate >= 1 )
            {
                int x = Mathf.FloorToInt(_tempMatToGenerate);
                _myIsland.Owner.ressources.CurrentMaterials += x;
                _tempMatToGenerate -= x;
            }

            if (_tempOrichalqueToGenerate >= 1)
            {
                int y = Mathf.FloorToInt(_tempOrichalqueToGenerate);
                _myIsland.Owner.ressources.CurrentOrichalque += y;
                _tempOrichalqueToGenerate -= y;
            }
        }

        private void OnMouseEnter()
        {
            _mouseOverThisBuilding = true;
        }

        private void OnMouseExit()
        {
            _mouseOverThisBuilding = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, -transform.up * 10);
        }
    }
}

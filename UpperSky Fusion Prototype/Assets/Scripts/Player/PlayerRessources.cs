using Custom_UI;
using UnityEngine;

namespace Player
{
    public class PlayerRessources : MonoBehaviour
    {
        private UIManager _uiManager;

        [SerializeField] private int playerMaterialsAtStart;
        [SerializeField] private int playerOrichalcAtStart;
        [SerializeField] private int playerSupplyAtStart;
        [SerializeField] private int playerMaxSupplyAtStart;

        // Those vairables are properties define below
        private int _currentMaterials;
        private int _currentOrichalque;
        private int _currentSupply;
        private int _currentMaxSupply;

        public int CurrentMaterials   
        {
            get => _currentMaterials;

            set
            {
                _currentMaterials = value;
                _uiManager.UpdateMaterialsTMP(value);
            }
        }
        
        public int CurrentOrichalque
        {
            get => _currentOrichalque;

            set
            {
                _currentOrichalque = value;
                _uiManager.UpdateOrichalcTMP(value);
            }
        }
        
        public int CurrentSupply
        {
            get => _currentSupply;

            set
            {
                if (value > CurrentMaxSupply) Debug.LogError("not enough avaible supply");
                else
                {
                    _currentSupply = value;
                    _uiManager.UpdateSupplyTMP(value, CurrentMaxSupply);
                }
            }
        }

        public int CurrentMaxSupply
        {
            get => _currentMaxSupply;

            set
            {
                _currentMaxSupply = value;
                _uiManager.UpdateSupplyTMP(CurrentSupply, value);
            }
        }

        private void Start()
        {
            _uiManager = UIManager.Instance;
            CurrentMaterials = playerMaterialsAtStart;
            CurrentOrichalque = playerOrichalcAtStart;
            CurrentSupply = playerSupplyAtStart;
            CurrentMaxSupply = playerMaxSupplyAtStart;
        }
    }
}

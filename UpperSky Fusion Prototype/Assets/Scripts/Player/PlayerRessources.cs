using Custom_UI;
using UnityEngine;
using UserInterface;

namespace Player
{
    public class PlayerRessources : MonoBehaviour
    {
        private UIManager _uiManager;

        [SerializeField] private int playerWoodAtStart;
        [SerializeField] private int playerMetalsAtStart;
        [SerializeField] private int playerOrichalcAtStart;
        [SerializeField] private int playerSupplyAtStart;
        [SerializeField] private int playerMaxSupplyAtStart;

        public int CurrentWood { get; set; }
        
        public int CurrentMetals { get; set; }

        public int CurrentOrichalque { get; set; }
        
        private int _currentSupply;
        private int _currentMaxSupply;
        
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
            CurrentWood = playerWoodAtStart;
            CurrentMetals = playerMetalsAtStart;
            CurrentOrichalque = playerOrichalcAtStart;
            CurrentSupply = playerSupplyAtStart;
            CurrentMaxSupply = playerMaxSupplyAtStart;
        }
    }
}

using System.Collections;
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

        #region Wood
        public int CurrentWood { get; set; }
        public float CurrentWoodGain { get; set; }
        private float _tempWoodToGenerate;
        #endregion
        
        #region Metals
        public int CurrentMetals { get; set; }
        public float CurrentMetalsGain { get; set; }
        private float _tempMetalsToGenerate;
        #endregion
        
        #region Orichalque
        public int CurrentOrichalque { get; set; }
        public float CurrentOrichalqueGain { get; set; }
        private float _tempOrichalqueToGenerate;
        #endregion
        
        #region Supply
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
        #endregion

        private void Start()
        {
            _uiManager = UIManager.Instance;
            CurrentWood = playerWoodAtStart;
            CurrentMetals = playerMetalsAtStart;
            CurrentOrichalque = playerOrichalcAtStart;
            CurrentSupply = playerSupplyAtStart;
            CurrentMaxSupply = playerMaxSupplyAtStart;
            
            StartCoroutine(CallEveryRealTimeSeconds());
        }
        
        private IEnumerator CallEveryRealTimeSeconds()
        {
            yield return new WaitForSecondsRealtime(1);

            GainRessources();
            _uiManager.UpdateRessourcesDisplay();

            StartCoroutine(CallEveryRealTimeSeconds());
        }
        
        private void GainRessources()
        {
            _tempWoodToGenerate += CurrentWoodGain;
            _tempMetalsToGenerate += CurrentMetalsGain;
            _tempOrichalqueToGenerate += CurrentOrichalqueGain;

            if (_tempWoodToGenerate >= 1 )
            {
                int x = Mathf.FloorToInt(_tempWoodToGenerate);
                CurrentWood += x;
                _tempWoodToGenerate -= x;
            }
            
            if (_tempMetalsToGenerate >= 1 )
            {
                int x = Mathf.FloorToInt(_tempMetalsToGenerate);
                CurrentMetals += x;
                _tempMetalsToGenerate -= x;
            }

            if (_tempOrichalqueToGenerate >= 1)
            {
                int y = Mathf.FloorToInt(_tempOrichalqueToGenerate);
                CurrentOrichalque += y;
                _tempOrichalqueToGenerate -= y;
            }
        }
    }
}

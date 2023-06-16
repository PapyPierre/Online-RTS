using Custom_UI;
using UnityEngine;

namespace Gameplay
{
    public class PlayerRessources : MonoBehaviour
    {
        private UIManager _uiManager;

        [SerializeField] private int playerMaterialsAtStart;
        [SerializeField] private int playerOrichalcAtStart;
        [SerializeField] private int playerSupplyAtStart;
        [SerializeField] private int playerMaxSupplyAtStart;

        private int _playerCurrentMaterials;
        private int _playerCurrentOrichalc;
        private int _playerCurrentSupply;
        private int _playerCurrentMaxSupply;

        public int PlayerCurrentMaterials
        {
            get => _playerCurrentMaterials;

            set
            {
                _playerCurrentMaterials = value;
                _uiManager.UpdateMaterialsTMP(value);
            }
        }
        
        public int PlayerCurrentOrichalc
        {
            get => _playerCurrentOrichalc;

            set
            {
                _playerCurrentOrichalc = value;
                _uiManager.UpdateOrichalcTMP(value);
            }
        }
        
        public int PlayerCurrentSupply
        {
            get => _playerCurrentSupply;

            set
            {
                if (value > PlayerCurrentMaxSupply) Debug.LogError("not enough avaible supply");
                else
                {
                    _playerCurrentSupply = value;
                    _uiManager.UpdateSupplyTMP(value, PlayerCurrentMaxSupply);
                }
            }
        }

        public int PlayerCurrentMaxSupply
        {
            get => _playerCurrentMaxSupply;

            set
            {
                _playerCurrentMaxSupply = value;
                _uiManager.UpdateSupplyTMP(PlayerCurrentSupply, value);
            }
        }

        private void Start()
        {
            _uiManager = UIManager.Instance;
            PlayerCurrentMaterials = playerMaterialsAtStart;
            PlayerCurrentOrichalc = playerOrichalcAtStart;
            PlayerCurrentSupply = playerSupplyAtStart;
            PlayerCurrentMaxSupply = playerMaxSupplyAtStart;
        }
    }
}

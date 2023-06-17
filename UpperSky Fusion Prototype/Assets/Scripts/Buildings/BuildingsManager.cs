using Custom_UI.InGame_UI;
using UnityEngine;

namespace Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;

        public BuildingIcon[] allBuildingsIcons;

        public enum AllBuildingsEnum
        {
            Foreuse = 0,
            ExploitationOrichalque = 1, 
            Habitation = 2,
            Menuiserie = 3,
            Tisserand = 4,
            Fonderie = 5,
            Manufacture = 6,
            Baliste = 7,
            Canon = 8,
            MineOrichalque = 9,
            Obusier = 10,
            CentreMeteo = 11,
            Usine = 12,
            Academie = 13
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }
        
        
    }
}

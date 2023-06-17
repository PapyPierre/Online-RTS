using UnityEngine;

namespace Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;

        public enum AllBuildings
        {
            ExploitationOrichalque = 0, 
            Habitation = 1,
            Foreuse = 2,
            CentreMeteo = 3,
            Menuiserie = 4,
            Tisserand = 5,
            Fonderie = 6,
            Academie = 7,
            Baliste = 8,
            Manufacture = 9,
            Canon = 10,
            Usine = 11,
            MineOrichalque = 12,
            Obusier = 13
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

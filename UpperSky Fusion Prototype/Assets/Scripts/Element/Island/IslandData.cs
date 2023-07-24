using UnityEngine;
using World;

namespace Element.Island
{
    [CreateAssetMenu(fileName = "IslandData", menuName = "Data/IslandData", order = 1)]
    public class IslandData : ElementData
    { 
        [field: SerializeField] 
        public IslandTypesEnum Type { get; set; }

       [field: SerializeField]
       public int MaxBuildingsOnThisIsland { get; private set; }
    }
}

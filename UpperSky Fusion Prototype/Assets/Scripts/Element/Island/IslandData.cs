using NaughtyAttributes;
using UnityEngine;
using World;

namespace Element.Island
{
    [CreateAssetMenu(fileName = "IslandData", menuName = "Data/IslandData", order = 1)]
    public class IslandData : ElementData
    {
        [field: SerializeField, Header("Gameplay Info")] 
        public IslandTypesEnum Type { get; set; }

        [field: SerializeField]
        public int MaxBuildingsOnThisIsland { get; private set; }
        
        [field: SerializeField, Range(0f,100f)]
        public float Rarity { get; private set; }
        
        [field: SerializeField, Header("Colors")]
        public Color TopColor { get; private set; }
        
        [field: SerializeField]
        public Color GroundColor { get; private set; }
        
        [field: SerializeField]
        public Color RockColor { get; private set; }
        
        [field: SerializeField, Header("Prefabs")]
        public GameObject[] Rocks { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Stones { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Trees { get; private set; }

        [field: SerializeField]
        public GameObject[] MediumStuff { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Plants { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Grass { get; private set; }
        
        [field: SerializeField, Header("NumberOfEach"), MinMaxSlider(0,10)]
        public Vector2Int NumberOfRocks { get; private set; }
        
        [field: SerializeField, MinMaxSlider(0,30)]
        public Vector2Int NumberOfStones { get; private set; }
        
        [field: SerializeField, MinMaxSlider(0,20)]
        public Vector2Int NumberOfTrees { get; private set; }

        [field: SerializeField, MinMaxSlider(0,50)]
        public Vector2Int NumberOfMediumStuff { get; private set; }

        [field: SerializeField, MinMaxSlider(0,100)]
        public Vector2Int NumberOfPlants { get; private set; }

        [field: SerializeField]
        public bool SpawnGrass { get; private set; }
    }
}

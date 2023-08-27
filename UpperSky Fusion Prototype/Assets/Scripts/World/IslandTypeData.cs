using NaughtyAttributes;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "IslandTypeData", menuName = "Data/IslandTypeData", order = 1)]

    public class IslandTypeData : ScriptableObject
    {
        [field: SerializeField, MinMaxSlider(0,100)]
        public Vector2Int Rarity { get; private set; }
        
        [field: SerializeField, Header("Colors")]
        public Color TopColor { get; private set; }
        
        [field: SerializeField]
        public Color GroundColor { get; private set; }
        
        [field: SerializeField, Header("Prefabs")]
        public GameObject[] Rocks { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Trees { get; private set; }

        [field: SerializeField]
        public GameObject[] MediumStuff { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Plants { get; private set; }
        
        [field: SerializeField]
        public GameObject[] Grass { get; private set; }
        
        [field: SerializeField, Header("NumberOfEach"), MinMaxSlider(0,100)]
        public Vector2Int NumberOfRocks { get; private set; }
        
        [field: SerializeField, MinMaxSlider(0,100)]
        public Vector2Int NumberOfTrees { get; private set; }

        [field: SerializeField, MinMaxSlider(0,100)]
        public Vector2Int NumberOfMediumStuff { get; private set; }

        [field: SerializeField, MinMaxSlider(0,100)]
        public Vector2Int NumberOfPlants { get; private set; }

        [field: SerializeField, MinMaxSlider(0,200)]
        public Vector2Int NumberOfGrass { get; private set; }
    }
}

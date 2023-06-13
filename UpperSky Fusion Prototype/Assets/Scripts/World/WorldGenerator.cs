using NaughtyAttributes;
using Nekwork_Objects.Islands;
using UnityEngine;

// Sciprt resonsable de la génération procedural du monde

namespace World
{
    public class WorldGenerator : MonoBehaviour
    {
        public static WorldGenerator instance;

        public float innerBorderRadius;
        public float outerBorderRadius;

        public int numberOfPlayers;
        [SerializeField] private Island islandPrefab;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            instance = this;
        }
        
        [Button()]
        private void GenerateWorld()
        {
            SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting);
            CalculatePlayersPosOnInnerBorder();
        }

        // Le but de cette fonction est de calculé x positions equidistante sur la bordure du cercle
        private void CalculatePlayersPosOnInnerBorder()
        {
            // Calcule du périmètre
            float innerBorderPerimeter = 2 * Mathf.PI * innerBorderRadius;
            
            // Calcule de la distance entre chaque joueur raporté sur le périmètre
            float distancePerPlayer = innerBorderPerimeter / numberOfPlayers;
            
            // Calcule de l'angle en radian avec cette distance sur le périmètre du cercle
            float radianAngle = (distancePerPlayer / innerBorderPerimeter) * 2 * Mathf.PI;
            
            // Conversion en degré
            float degreeAngle = radianAngle * 180 / Mathf.PI;
            
            SpawnPlayerPosAtEachAngle(degreeAngle);
        }

        private void SpawnPlayerPosAtEachAngle(float angle)
        {
            for (int i = 0; i < numberOfPlayers -1; i++)
            {
                transform.Rotate(Vector3.up, angle);
                SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting);
            }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            transform.Rotate(Vector3.up, Random.Range(0,359));
        }

        private void SpawnIsland(Vector3 position, IslandTypesEnum type)
        {
            Island island = Instantiate(islandPrefab, position, Quaternion.identity, transform);
            island.type = type;
        }
    }
}

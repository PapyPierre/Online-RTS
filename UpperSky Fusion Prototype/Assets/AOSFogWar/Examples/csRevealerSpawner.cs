/*
 * Created :    Spring 2023
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    FogWar
 * Filename :   csRevealerSpawner.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 */

using AOSFogWar;
using AOSFogWar.Used_Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace FischlWorks_FogWar
{
    public class csRevealerSpawner : MonoBehaviour
    {
        [FormerlySerializedAs("fogWar")] [SerializeField]
        private FogOfWar fogOfWar = null;

        [SerializeField]
        private GameObject exampleRevealer = null;
        
        private void Start()
        {
            // This part is meant to be modified following the project's scene structure later...
            try
            {
                fogOfWar = GameObject.Find("FogWar").GetComponent<FogOfWar>();
            }
            catch
            {
                Debug.LogErrorFormat("Failed to fetch csFogWar component. " +
                    "Please rename the gameobject that the module is attachted to as \"FogWar\", " +
                    "or change the implementation located in the csFogVisibilityAgent.cs script.");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Vector3 randomPoint = new Vector3(
                    Random.Range(-fogOfWar.levelData.levelDimensionX / 2.0f, fogOfWar.levelData.levelDimensionX / 2.0f),
                    fogOfWar.LevelMidPoint.position.y + 0.5f,
                    Random.Range(-fogOfWar.levelData.levelDimensionY / 2.0f, fogOfWar.levelData.levelDimensionY / 2.0f));

                // Instantiating & fetching the revealer Transform
                Transform randomTransform = Instantiate(exampleRevealer, randomPoint, Quaternion.identity).GetComponent<Transform>();

                // Utilizing the constructor, setting updateOnlyOnMove to true will not update the fog texture immediately
                int index = fogOfWar.AddFogRevealer(new FogOfWar.FogRevealer(randomTransform, 3, false));
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (fogOfWar.FogRevealers.Count > 2)
                {
                    fogOfWar.RemoveFogRevealer(fogOfWar.FogRevealers.Count - 1);
                }
            }
        }
    }
}
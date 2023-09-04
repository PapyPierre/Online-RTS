using UnityEngine;

namespace Ressources.AOSFogWar.Used_Scripts
{
    public class FogAgentIsland : FogAgent
    {
        [SerializeField] private GameObject minimapCanvas;

        public void Init(GameObject graph, GameObject uiCanvas, GameObject minimapCanva)
        {
            base.Init(graph, uiCanvas);
            minimapCanvas = minimapCanva;
            minimapCanvas.SetActive(false);
        }
        
        protected override void OnFirstSeenTime()
        {
            minimapCanvas.SetActive(true);
        }
    }
}

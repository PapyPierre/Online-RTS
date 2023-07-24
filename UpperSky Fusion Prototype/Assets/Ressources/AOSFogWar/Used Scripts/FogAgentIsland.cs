using UnityEngine;

namespace Ressources.AOSFogWar.Used_Scripts
{
    public class FogAgentIsland : FogAgent
    {
        [SerializeField] private GameObject minimapIcone;

        public override void Init(GameObject graph, GameObject canvas, GameObject minimapIcon)
        {
            base.Init(graph, canvas);
            minimapIcone.SetActive(false);
        }
        
        protected override void OnFirstSeenTime()
        {
            minimapIcone.SetActive(true);
        }
    }
}

using UnityEngine;

namespace AOSFogWar.Used_Scripts
{
    public class FogAgent_Island : FogAgent
    {
        [SerializeField] private GameObject minimapIcone;

        public override void Init(GameObject graph, GameObject canvas)
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

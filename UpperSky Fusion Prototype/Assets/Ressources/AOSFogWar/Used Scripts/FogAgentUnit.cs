using UnityEngine;

namespace Ressources.AOSFogWar.Used_Scripts
{
    public class FogAgentUnit : FogAgent
    {
        [SerializeField] private GameObject minimapIcone;

        public void Init(GameObject graph, GameObject canvas, GameObject minimapIcon)
        {
            base.Init(graph, canvas);
            minimapIcone = minimapIcon;
            minimapIcone.SetActive(false);
        }

        protected override void OnVisible()
        {
            base.OnVisible();
            minimapIcone.SetActive(true);
        }

        protected override void OnHide()
        {
            base.OnHide();
            minimapIcone.SetActive(false);
        }
    }
}

/*
 * ORIGINAL SCRIPT : 
 * 
 * Created :    Winter 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    FogWar
 * Filename :   csFogVisibilityAgent.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 *
 * HAVE BEEN MODIFIED BY PIERRE FERRARI
 */

using AOSFogWar.Used_Scripts;
using NaughtyAttributes;
using UnityEngine;

namespace Ressources.AOSFogWar.Used_Scripts
{
    public class FogAgent : MonoBehaviour
    {
        private FogOfWar _fogOfWar;

        [ReadOnly] public bool isVisible;
        private bool _haveBeenSeenOnce;
        [SerializeField] private bool keepVisible;

        [SerializeField, Range(0, 2)] private int additionalRadius;

        private GameObject _graphObject;
        private GameObject _uIcanvas;

        public virtual void Init(GameObject graph, GameObject uiCanva)
        {
            _fogOfWar = FogOfWar.Instance;
            
            _graphObject = graph;
            _uIcanvas = uiCanva;
        }
        
        private void Update()
        {
            if (_fogOfWar is null || _fogOfWar.CheckWorldGridRange(transform.position) == false) return;

            isVisible = _fogOfWar.CheckVisibility(transform.position, additionalRadius);

            if (isVisible)
            {
                if (!_haveBeenSeenOnce)
                {
                    _haveBeenSeenOnce = true;
                    OnFirstSeenTime();
                }
                
                OnVisible();
            }
            else
            {
                if (keepVisible && _haveBeenSeenOnce) return;
                
                OnHide();
            }
        }

        protected virtual void OnFirstSeenTime() { }

        protected virtual void OnVisible()
        {
            if (!_graphObject.activeSelf) _graphObject.SetActive(true);
            if (_uIcanvas != null) _uIcanvas.SetActive(true);
        }

        protected virtual void OnHide()
        {
            if (_graphObject.activeSelf) _graphObject.SetActive(false);
            if (_uIcanvas != null) _uIcanvas.SetActive(false);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_fogOfWar is null || Application.isPlaying is false) return;
            
            if (_fogOfWar.CheckWorldGridRange(transform.position) is false)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(
                    new Vector3(
                        Mathf.RoundToInt(transform.position.x),
                        0,
                        Mathf.RoundToInt(transform.position.z)),
                    (_fogOfWar.UnitScale / 2.0f) + additionalRadius);

                return;
            }

            Gizmos.color = _fogOfWar.CheckVisibility(transform.position, additionalRadius) ? Color.green : Color.yellow;

            Gizmos.DrawWireSphere(
                new Vector3(
                    Mathf.RoundToInt(transform.position.x),
                    0,
                    Mathf.RoundToInt(transform.position.z)),
                (_fogOfWar.UnitScale / 2.0f) + additionalRadius);
        }
        #endif
    }
}
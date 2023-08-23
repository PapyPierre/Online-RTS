using Element.Entity.Buildings;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Skills.Skills_Behaviour
{
    public class GomorrahBomb : NetworkBehaviour
    {
        [Expandable] public GomorrahBombData data;
        [Networked] private PlayerController Owner { get;  set; }
        private NetworkObject _thisObj;

        public void Init(PlayerController owner, NetworkObject obj)
        {
            Owner = owner;
            _thisObj = obj;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (!other.collider.CompareTag("Unit"))
            {
                Explode();
            }
        }

        private void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, data.ImpactRange, 
                LayerMask.GetMask("Building"));

            foreach (var col in colliders)
            {
                var building = col.GetComponent<BaseBuilding>();
                if (building.Owner == Owner || building.IsDead) continue;
               building.RPC_TakeDamage(data.ImpactDamage, data.ArmorPenetration);
               break;
            }
         
            Owner.Runner.Spawn(data.ExplosionVfx, transform.position);
            Owner.Runner.Despawn(_thisObj);
        }
    }
}

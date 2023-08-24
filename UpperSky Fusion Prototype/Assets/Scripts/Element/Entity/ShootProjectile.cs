using System.Collections;
using Fusion;
using UnityEngine;

namespace Element.Entity
{
    public class ShootProjectile : NetworkBehaviour
    {
        [Networked] private BaseEntity Target { get; set; }
        private Vector3 _dir;
        [SerializeField] private float speed;
        [SerializeField] private NetworkPrefabRef explostionVfx;
        private bool explode;

        private int _damageOnExlosion;
        private int _armorPenenetration;
        private BaseEntity _shooterOfThisObj;

        public void Init(BaseEntity target, int damage, int armorPen, BaseEntity shooter)
        {
            _dir = target.transform.position - transform.position;
            Target = target;
            _damageOnExlosion = damage;
            _armorPenenetration = armorPen;
            _shooterOfThisObj = shooter;
            var lifeTime = Vector3.Distance(transform.position, Target.transform.position) / speed;
            StartCoroutine(WaitToExplode(lifeTime));
        }
        
        public override void FixedUpdateNetwork()
        {
            if (explode) return;
            
            transform.position += _dir * Runner.DeltaTime * speed;
            
            if (Vector3.Distance(transform.position, Target.transform.position) < 2)
            {
                Target.RPC_TakeDamage(_damageOnExlosion, _armorPenenetration, _shooterOfThisObj);
                Explode();
            }
        }

        private IEnumerator WaitToExplode(float delay)
        {
            yield return new WaitForSeconds(delay);
            Explode();
        }

        private void Explode()
        {
            if (explode) return;
            explode = true;
            
            GameManager.Instance.thisPlayer.Runner.Spawn(explostionVfx, transform.position);
            GameManager.Instance.thisPlayer.Runner.Despawn(Object);
        }
    }
}
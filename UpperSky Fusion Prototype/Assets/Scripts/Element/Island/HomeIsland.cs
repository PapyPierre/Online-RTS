using Player;
using UnityEngine;

namespace Element.Island
{
    public class HomeIsland : BaseIsland
    {
        public override void Init(Transform parent, PlayerController owner)
        {
            base.Init(parent, owner);
            Owner.SpawnStartBuilding(this);
        }
    }
}

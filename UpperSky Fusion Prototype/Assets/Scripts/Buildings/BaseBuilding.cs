using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace Entity.Buildings
{
    public class BaseBuilding : NetworkBehaviour
    {
        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;
    }
}

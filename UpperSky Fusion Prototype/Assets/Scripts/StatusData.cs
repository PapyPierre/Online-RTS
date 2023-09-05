using System.Collections;
using System.Collections.Generic;
using Element.Entity.Military_Units;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusData", menuName = "Data/StatusData", order = 5)]
public class StatusData : ScriptableObject
{
   [field: SerializeField]
   public UnitsManager.UnitStatusEnum Status { get; private set; }
   
   [field: SerializeField]
   public float UnitMoveSpeedModificator { get; private set; }
   
   [field: SerializeField]
   public float UnitHpModificator { get; private set; }
   
   [field: SerializeField]
   public float UnitDamageModificator { get; private set; }
}

using System;
using Element.Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Element.Entity.Buildings
{
    public class FormationBuilding : BaseBuilding
    {
        public readonly UnitFormationQueue FormationQueue = new ();
        [HideInInspector] public float timeLeftToForm;
        private bool notEnoughSupplies;
        
        protected override void Update()
        {
            base.Update();
            if (FormationQueue.IsNotEmpty()) ManageFormation();
            UpdateFormation();
        }
        
        private void ManageFormation()
        {
            if (timeLeftToForm <= 0)
            {
                TryFormFirstUnitInQueue();
            }
        }

        public void AddUnitToFormationQueue(UnitsManager.AllUnitsEnum unit)
        {        
            PlayerController player = GameManager.thisPlayer;
            
            var unitWoodCost = UnitsManager.allUnitsData[(int) unit].WoodCost;
            var unitMetalsCost = UnitsManager.allUnitsData[(int) unit].MetalsCost;
            var unitOriCost =UnitsManager.allUnitsData[(int) unit].OrichalqueCost;

            // Check if player have enough ressources 
            if (player.ressources.CurrentWood >= unitWoodCost 
                && player.ressources.CurrentMetals >= unitMetalsCost 
                && player.ressources.CurrentOrichalque >= unitOriCost)
            {
                if (FormationQueue.Count() < 5) // 5 because there is 5 slots in a formation queue
                {
                    player.ressources.CurrentWood -= unitWoodCost;
                    player.ressources.CurrentMetals -= unitMetalsCost;
                    player.ressources.CurrentOrichalque -= unitOriCost;

                    FormationQueue.Enqueue(unit);
                    if (FormationQueue.Count() is 1)
                    {
                        timeLeftToForm = UnitsManager.allUnitsData[(int) unit].ProductionTime;

                    }
                    UIManager.UpdateFormationQueueDisplay(this);
                }
                else Debug.Log("Queue is full");
            }
            else Debug.Log("not enough ressources");
        }

        public void RemoveUnitFromFormationQueue(int index)
        {
            if (!FormationQueue.IsNotEmpty()) return;

            PlayerController player = GameManager.thisPlayer;

            UnitsManager.AllUnitsEnum unitAtThisIndex = FormationQueue.PeekAtGivenIndex(index);

            player.ressources.CurrentWood += UnitsManager.allUnitsData[(int) unitAtThisIndex].WoodCost;
            player.ressources.CurrentMetals += UnitsManager.allUnitsData[(int) unitAtThisIndex].MetalsCost;
            player.ressources.CurrentOrichalque += UnitsManager.allUnitsData[(int) unitAtThisIndex].OrichalqueCost;
            
            if (index is 0 && FormationQueue.Count() >= 2)
            {
                timeLeftToForm = UnitsManager.allUnitsData[(int) FormationQueue.PeekAtGivenIndex(1)].ProductionTime;
            }
            
            FormationQueue.RemoveAtGivenIndex(index);
            UIManager.UpdateFormationQueueDisplay(this);
        }
        
        private void TryFormFirstUnitInQueue()
        {
            var unitSupplyCost = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].SupplyCost;

            if (unitSupplyCost + Owner.ressources.CurrentSupply > Owner.ressources.CurrentMaxSupply)
            {
                if (!notEnoughSupplies)
                {
                    Debug.Log("not enough available supplies");
                    notEnoughSupplies = true;
                }
                return;
            }
            
            Owner.ressources.CurrentSupply += unitSupplyCost;
            notEnoughSupplies = false;
                
            timeLeftToForm = 100; // Par sécurité
            FormFirstUnitInQueue();
        }
        
        private void FormFirstUnitInQueue()
        {
            // faire spawn la premier unité dans la queue
            var prefab = UnitsManager.allUnitsPrefab[(int) FormationQueue.Dequeue()];
            
            Vector3 myPos = transform.position;
            var randomX = Random.Range(-2f, 2f);
            var randomZ = Random.Range(-2f, 2f);
            Vector3 spawnPos = new Vector3(myPos.x + randomX, UnitsManager.flyingHeightOfUnits, myPos.z + randomZ);
            NetworkObject obj = Runner.Spawn(prefab, spawnPos, Quaternion.identity, Object.StateAuthority);

            BaseUnit unit = obj.GetComponent<BaseUnit>();
            unit.Init(Owner, unit.Data);

            ApplyStatusToFormedUnits(unit, myIsland.data.Type);

            if (FormationQueue.IsNotEmpty())
            {
                timeLeftToForm = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            }
            
            UIManager.UpdateFormationQueueDisplay(this);
        }

        private void ApplyStatusToFormedUnits(BaseUnit unit,IslandTypesEnum  islandTypes)
        {
            switch (islandTypes)
            {
                case IslandTypesEnum.Tropical:
                    unit.TryAddStatusToUnit(UnitsManager.UnitStatusEnum.Immune);
                    break;
                case IslandTypesEnum.Cursed:
                    unit.TryAddStatusToUnit(UnitsManager.UnitStatusEnum.Cursed);
                    break;
            }
        }
        
        private void UpdateFormation()
        {
            timeLeftToForm -= 1 * Time.deltaTime;

            if (FormationQueue.IsNotEmpty())
            {
                if (GameManager.thisPlayer.lastSelectedElement == this && !notEnoughSupplies)
                {
                    UpdateFormationQueueSliderWithNewValue();
                }
            }
        }
        
        public void UpdateFormationQueueSliderWithNewValue()
        {
            var totalTimeToRun = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            var timeSpendAlready = totalTimeToRun - timeLeftToForm;
            var timeSpentOnTotalTime = timeSpendAlready / totalTimeToRun;
            
            UIManager.UpdateFormationQueueSlider(timeSpentOnTotalTime);
        }
    }
}

using System.Collections.Generic;

namespace Element.Entity.Military_Units
{
    public class UnitFormationQueue
    {
        private readonly List<UnitsManager.AllUnitsEnum> queue = new ();

        public bool IsNotEmpty()
        {
            return queue.Count > 0;
        }

        public UnitsManager.AllUnitsEnum Peek()
        {
            return queue[0];
        }

        public UnitsManager.AllUnitsEnum Dequeue()
        {
            var unitToDequeue = queue[0];
            queue.Remove(0);
            return unitToDequeue;
        }

        public void Enqueue(UnitsManager.AllUnitsEnum unit)
        {
            queue.Add(unit);
        }

        public UnitsManager.AllUnitsEnum PeekAtGivenIndex(int i)
        {
            return queue[i];
        }

        public void RemoveAtGivenIndex(int i)
        {
            queue.Remove(queue[i]);
        }

        public int Count()
        {
            return queue.Count;
        }
    }
}

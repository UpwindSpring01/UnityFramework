using UnityEngine;

namespace Helpers_KevinLoddewykx.General.WeightedArray
{
    public abstract class WeightedScriptableObject : ScriptableObject
    {
        public abstract WeightedArrayObject[] Elements { get; }

        public WeightedArrayObject Element
        {
            get { return Elements[0]; }
        }

        public int GetRandomObjectIndex(int type)
        {
            return Elements[type].WeightedArray.GetRandomObjectIndex();
        }

        public int GetCountWeighted(int type)
        {
            return Elements[type].WeightedArray.GetWeightedElementsCount();
        }

        public GameObject GetObject(int type, int index)
        {
            return Elements[type].WeightedArray.GetGameObject(index);
        }

        public int GetPrevious(int type, int index)
        {
            return Elements[type].WeightedArray.GetPrevious(index);
        }

        public int GetNext(int type, int index)
        {
            return Elements[type].WeightedArray.GetNext(index);
        }
    }
}

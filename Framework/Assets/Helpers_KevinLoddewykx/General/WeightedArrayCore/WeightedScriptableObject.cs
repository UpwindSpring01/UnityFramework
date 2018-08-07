using UnityEngine;

namespace Helpers_KevinLoddewykx.General.WeightedArrayCore
{
    public abstract class WeightedScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        public abstract string[] Names { get; }
#endif

        public abstract WeightedArray[] Elements { get; }

        public WeightedArray Element
        {
            get { return Elements[0]; }
        }

        public int GetRandomObjectIndex(int elementIndex)
        {
            return Elements[elementIndex].GetRandomObjectIndex();
        }

        public int GetCountWeighted(int elementIndex)
        {
            return Elements[elementIndex].GetWeightedElementsCount();
        }

        public GameObject GetObject(int elementIndex, int index)
        {
            return Elements[elementIndex].GetGameObject(index);
        }

        public int GetPrevious(int elementIndex, int index)
        {
            return Elements[elementIndex].GetPrevious(index);
        }

        public int GetNext(int elementIndex, int index)
        {
            return Elements[elementIndex].GetNext(index);
        }
    }
}

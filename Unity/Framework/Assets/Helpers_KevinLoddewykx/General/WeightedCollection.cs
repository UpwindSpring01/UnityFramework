using Helpers_KevinLoddewykx.General.WeightedArray;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    [CreateAssetMenu(menuName = "Resources/Weighted Collection")]
    public class WeightedCollection : BaseWeightedCollection
    {
        [SerializeField]
        [HideInInspector]
        private WeightedArrayObject[] _elements = new WeightedArrayObject[] { new WeightedArrayObject("Weighted Gameobjects", false) };
        public override WeightedArrayObject[] Elements
        {
            get { return _elements; }
        }
    }
}

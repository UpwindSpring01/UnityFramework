using Helpers_KevinLoddewykx.General.WeightedArray;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    [CreateAssetMenu(menuName = "Resources/Weighted Collection (Allow Empty)")]
    public class WeightedCollectionAllowEmpty : BaseWeightedCollection
    {
        [SerializeField]
        [HideInInspector]
        private WeightedArrayObject[] _elements = new WeightedArrayObject[] { new WeightedArrayObject("Weighted Gameobjects (Allow Empty)", true) };
        public override WeightedArrayObject[] Elements
        {
            get { return _elements; }
        }
    }
}

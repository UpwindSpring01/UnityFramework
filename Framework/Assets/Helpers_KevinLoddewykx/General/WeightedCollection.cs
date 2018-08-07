using Helpers_KevinLoddewykx.General.WeightedArrayCore;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    [CreateAssetMenu(menuName = "Resources/Weighted Collection")]
    public class WeightedCollection : WeightedScriptableObject
    {
        [SerializeField]
        private WeightedArray[] _elements = new WeightedArray[] { new WeightedArray(false) };

        public override WeightedArray[] Elements
        {
            get { return _elements; }
        }

#if UNITY_EDITOR
        [SerializeField]
        private string[] _names = new string[] { "Weighted Gameobjects" };

        public override string[] Names
        {
            get { return _names; }
        }
#endif
    }
}

using Helpers_KevinLoddewykx.General.WeightedArrayCore;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    [CreateAssetMenu(menuName = "Resources/Weighted Collection (Single, No Empty)")]
    public class WeightedCollectionSingle : WeightedScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private WeightedArray[] _elements = new WeightedArray[] { new WeightedArray(false) };

        public override WeightedArray[] Elements
        {
            get { return _elements; }
        }

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private string[] _names = new string[] { "Weighted Gameobjects" };

        public override string[] Names
        {
            get { return _names; }
        }
#endif
    }
}

using System;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General.WeightedArray
{
    [Serializable]
    public class WeightedObject
    {
        public GameObject Object;
        public int Weight;
    }

    [Serializable]
    public class WeightedArrayObject
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private WeightedArray _weightedArray;

        public string Name
        {
            get { return _name; }
        }

        public WeightedArray WeightedArray
        {
            get { return _weightedArray; }
        }

        public WeightedArrayObject(string name, bool allowNull)
        {
            _name = name;
            _weightedArray = new WeightedArray(allowNull);
        }
    }
}
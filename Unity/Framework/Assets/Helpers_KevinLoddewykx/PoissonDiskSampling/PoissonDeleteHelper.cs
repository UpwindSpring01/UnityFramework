#if UNITY_EDITOR
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public class PoissonDeleteHelper : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private PoissonPlacer _placer;

        public PoissonPlacer Placer
        {
            get { return _placer; }
            set { _placer = value; }
        }
    }
}
#endif
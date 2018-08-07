using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [CustomEditor(typeof(PoissonPlacer))]
    public class PoissonPlacerEditor : Editor
    {
        private PoissonHelper _helper;

        private void OnEnable()
        {
            PoissonPlacer placer = (PoissonPlacer)target;
            _helper = new PoissonHelper(placer);
            _helper.Init();
        }

        private void OnDestroy()
        {
            if (target == null)
            {
                _helper.ShutDown();
            }
            else
            {
                _helper.StopUndoRedoTracking();
            }
        }

        public override void OnInspectorGUI()
        {
            if (((PoissonPlacer)target).enabled)
            {
                _helper.CreateUI();
            }
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}
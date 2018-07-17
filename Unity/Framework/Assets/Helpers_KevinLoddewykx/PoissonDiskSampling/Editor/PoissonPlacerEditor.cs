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
            _helper = new PoissonHelper(target, placer.ModeData, placer.Data, placer.EditorData, null);
            _helper.Init();
        }

        private void OnDestroy()
        {
            if (target == null)
            {
                _helper.ShutDown();
            }
        }

        public override void OnInspectorGUI()
        {
            _helper.CreateUI();
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}
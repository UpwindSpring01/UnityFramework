#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [ExecuteInEditMode]
    public class PoissonPlacer : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private PoissonModeData _modeData = new PoissonModeData() { Mode = DistributionMode.Plane };

        [SerializeField]
        [HideInInspector]
        private List<PoissonData> _data = new List<PoissonData>() { new PoissonData() };

        [SerializeField]
        [HideInInspector]
        private PoissonInternalEditorData _editorData = new PoissonInternalEditorData();

        public PoissonModeData ModeData => _modeData;

        public List<PoissonData> Data => _data;

        public PoissonInternalEditorData EditorData => _editorData;

        public void Awake()
        {
            if(!Application.isPlaying && !_editorData.HelperVisual)
            {
                _editorData.InitVisual(_modeData, _data[_editorData.SelectedLevelIndex], transform);
            }
        }
    }
}
#endif
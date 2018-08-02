#if UNITY_EDITOR
using Helpers_KevinLoddewykx.General;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [ExecuteInEditMode]
    public class PoissonPlacer : MonoBehaviour, IPoissonDataHolder
    {
        [SerializeField]
        [HideInInspector]
        private PoissonModeData _modeData = new PoissonModeData() { Mode = DistributionMode.Plane };

        [SerializeField]
        [HideInInspector]
        private List<PoissonData> _data = new List<PoissonData>() { new PoissonData() };

        [SerializeField]
        [HideInInspector]
        private PoissonUIData _uiData = new PoissonUIData();

        [SerializeField]
        [HideInInspector]
        private PoissonInternalEditorData _editorData = new PoissonInternalEditorData();

        public PoissonModeData ModeData
        {
            get { return _modeData; }
            set { _modeData = value; }
        }

        public List<PoissonData> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public PoissonUIData UIData
        {
            get { return _uiData; }
            set { _uiData = value; }
        }

        public PoissonInternalEditorData EditorData
        {
            get { return _editorData; }
            set { _editorData = value; }
        }

        public bool IsWindow => false;

        public void VisualTransformChanged()
        {
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                EditorConnector.Connector.Register(this);
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                EditorConnector.Connector.Unregister(this);
            }
        }

        private void Start()
        {
            // Empty start so the enable/disable checkbox is added to the inspector
        }

        private void Reset()
        {
            EditorConnector.Connector.Reset(this);
        }
    }
}
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Currently broken
// Presets (waiting for the new built-in Preset Manager)
// UI, margins not resetted
// Visualizer shader not optimal

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public class PoissonDiskSamplingWindow : EditorWindow, IPoissonDataHolder
    {
        [MenuItem("Tools/Poisson Disk Distribution")]
        static void PoissonWindow()
        {
            PoissonDiskSamplingWindow window = (PoissonDiskSamplingWindow)CreateInstance(typeof(PoissonDiskSamplingWindow));
            window.titleContent = new GUIContent("Poisson Disk Distribution");
            window.minSize = new Vector2(500, 400);
            window.ShowUtility();
        }

        [SerializeField]
        private List<PoissonData> _data = new List<PoissonData>() { new PoissonData() };

        [SerializeField]
        private PoissonModeData _modeData = new PoissonModeData();

        [SerializeField]
        private PoissonInternalEditorData _editorData = new PoissonInternalEditorData();

        [SerializeField]
        private PoissonUIData _uiData = new PoissonUIData();

        [SerializeField]
        private long _serializedId = 0;

        private long _id = 0;

        private PoissonHelper _helper;

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

        public bool IsWindow => true;

        public PoissonDiskSamplingWindow()
        {
            _helper = new PoissonHelper(this);
        }

        private void OnDestroy()
        {
            _helper.CleanupPlacedObjects(0);
        }

        private void OnGUI()
        {
            _helper.CreateUI();
        }

        private void OnEnable()
        {
            _helper.Init();
            // Set _editorData to new instance, so it does not conflict with undo redo inside the PoissonHelper
            // PlacedObjects and other vars were being resetted
            _editorData = null;
            Undo.undoRedoPerformed += UndoRedoEvent;
        }

        private void OnDisable()
        {
            _helper.ShutDown();
            _editorData = _helper.EditorData;

            Undo.undoRedoPerformed -= UndoRedoEvent;
        }

        private void UndoRedoEvent()
        {
            if (_helper.EditorData.HelperVisual && _serializedId != _id)
            {
                _helper.RefreshDistribution();
                _id = _serializedId;
            }

            Repaint();
        }

        public void VisualTransformChanged()
        {
            ++_serializedId;
            ++_id;
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Currently broken
// Undo/Redo
// The default reset button from a monobehaviour
// Presets
// UI, margins not resetted
// Visualizer shader not optimal

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonDiskSamplingWindow : EditorWindow
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

        private PoissonHelper _helper;

        public PoissonDiskSamplingWindow()
        {
            _helper = new PoissonHelper(this, _modeData, _data, _editorData, this);
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

            Undo.undoRedoPerformed += UndoRedoEvent;
        }

        private void OnDisable()
        {
            _helper.ShutDown();

            Undo.undoRedoPerformed -= UndoRedoEvent;
        }

        private void UndoRedoEvent()
        {
            Repaint();
        }
    }
}
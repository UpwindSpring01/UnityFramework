using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
    public partial class PoissonHelper
    {
        private readonly PoissonModeData _modeData;
        private readonly List<PoissonData> _data;
        private readonly PoissonInternalEditorData _editorData;

        private Object _object;
        private EditorWindow _window;

        private readonly bool _isPlacer;
        private readonly bool _isPrefab;

        public PoissonHelper(Object obj, PoissonModeData modeData, List<PoissonData> data, PoissonInternalEditorData editorData, EditorWindow window)
        {
            _object = obj;
            _modeData = modeData;
            _data = data;
            _editorData = editorData;
            _window = window;

            _selectedData = _data[_editorData.SelectedLevelIndex];
            _isPlacer = _object is PoissonPlacer;
            _isPrefab = _isPlacer && (PrefabUtility.GetPrefabType(_object) == PrefabType.Prefab);
        }

        public void Init()
        {
            if (!_isPrefab)
            {
                PoissonHelperInternalStorage.Instance.RemoveAndAdd(_object, this);
                
                _editorData.InitVisual(_modeData, _selectedData, (_isPlacer) ? ((PoissonPlacer)_object).transform : null);
            }
        }

        public void ShutDown()
        {
            if (!_isPrefab)
            {

                PoissonHelperInternalStorage.Instance.Remove(_object);
                _editorData.DestroyVisual();
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if(!_editorData.HelperVisual)
            {
                PoissonHelperInternalStorage.Instance.Remove(_object);
                return;
            }
            if (_modeData.Mode != DistributionMode.Surface && _editorData.HelperVisual.transform.hasChanged)
            {
                if (_modeData.RealtimePreview)
                {
                    bool isValidSurface, preValid, currValid, postValid;
                    int highestValid;
                    ValidateSettings(false, out isValidSurface, out preValid, out currValid, out postValid, out highestValid);

                    _editorData.LastFrameValid = isValidSurface && preValid && currValid && postValid;
                    if (_editorData.LastFrameValid)
                    {
                        DistributePoisson(0, highestValid, true);
                    }
                    _editorData.HelperVisual.transform.hasChanged = false;
                }
                _window?.Repaint();
            }
        }
    }
}
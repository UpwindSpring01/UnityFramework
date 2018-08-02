using Helpers_KevinLoddewykx.General;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public class PoissonHelperInternalStorage : SingletonScriptableObject<PoissonHelperInternalStorage>, IPoissonConnector
    {
        [SerializeField]
        [HideInInspector]
        private List<IPoissonDataHolder> _placers = new List<IPoissonDataHolder>();

        [SerializeField]
        [HideInInspector]
        private List<PoissonHelper> _helpers = new List<PoissonHelper>();

        public void RemoveAndAdd(IPoissonDataHolder obj, PoissonHelper helper)
        {
            Remove(obj);
            SceneView.onSceneGUIDelegate += helper.OnSceneGUI;
            if (!helper.DataHolder.IsWindow)
            {
                Undo.undoRedoPerformed += helper.OnUndoRedoPerformedPlacer;
            }
            _helpers.Add(helper);
            _placers.Add(obj);
        }

        public void Remove(IPoissonDataHolder obj)
        {
            int index = _placers.IndexOf(obj);
            if (index >= 0)
            {
                SceneView.onSceneGUIDelegate -= _helpers[index].OnSceneGUI;
                if (!_helpers[index].DataHolder.IsWindow)
                {
                    Undo.undoRedoPerformed -= _helpers[index].OnUndoRedoPerformedPlacer;
                }
                _helpers.RemoveAt(index);
                _placers.RemoveAt(index);
            }
        }

        public void RemoveUndoRedoTracking(IPoissonDataHolder obj)
        {
            int index = _placers.IndexOf(obj);
            if (index >= 0)
            {
                Undo.undoRedoPerformed -= _helpers[index].OnUndoRedoPerformedPlacer;
            }
        }

        public void Reset(IPoissonDataHolder obj)
        {
            int index = _placers.IndexOf(obj);
            if (index >= 0)
            {
                _helpers[index].Reset();
                _helpers[index].EditorData.RefreshVisual(_helpers[index].ModeData, _helpers[index].SelectedData);
            }
        }

        public void Register(IPoissonDataHolder obj)
        {
            // Create visual and attach onSceneGUIDelegate + undoRedoPerformed listeners
            new PoissonHelper(obj).Init();
        }

        public void Unregister(IPoissonDataHolder obj)
        {
            // Create visual and attach onSceneGUIDelegate + undoRedoPerformed listeners
            Remove(obj);
            obj.EditorData.DestroyVisual(obj.ModeData);
        }
    }
}

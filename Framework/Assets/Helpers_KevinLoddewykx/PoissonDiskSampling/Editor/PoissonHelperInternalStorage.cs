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

        public bool RemoveAndAdd(IPoissonDataHolder obj, PoissonHelper helper)
        {
            bool existed = Remove(obj);
            SceneView.onSceneGUIDelegate += helper.OnSceneGUI;
            if (!helper.DataHolder.IsWindow)
            {
                Undo.undoRedoPerformed += helper.OnUndoRedoPerformedPlacer;
            }
            _helpers.Add(helper);
            _placers.Add(obj);

            return existed;
        }

        public bool Remove(IPoissonDataHolder obj)
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
                return true;
            }
            return false;
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
                _helpers[index].EditorData.RefreshVisual(_helpers[index].ModeData, _helpers[index].SelectedData, _helpers[index].DataHolder.IsWindow);
            }
        }

        public void Register(IPoissonDataHolder obj)
        {
            // Create visual and attach onSceneGUIDelegate + undoRedoPerformed listeners
            new PoissonHelper(obj).Init();
        }

        public void Unregister(IPoissonDataHolder obj)
        {
            // Destroy visual and detach onSceneGUIDelegate + undoRedoPerformed listeners
            Remove(obj);

            obj.EditorData.DestroyVisual(obj.ModeData);
        }
    }
}

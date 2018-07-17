using Helpers_KevinLoddewykx;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public class PoissonHelperInternalStorage : SingletonScriptableObject<PoissonHelperInternalStorage>
    {
        [SerializeField]
        [HideInInspector]
        private List<Object> _placers = new List<Object>();

        [SerializeField]
        [HideInInspector]
        private List<PoissonHelper> _sceneFuncs = new List<PoissonHelper>();

        public void RemoveAndAdd(Object obj, PoissonHelper helper)
        {
            Remove(obj);
            SceneView.onSceneGUIDelegate += helper.OnSceneGUI;
            _sceneFuncs.Add(helper);
            _placers.Add(obj);
        }

        public void Remove(Object obj)
        {
            int index = _placers.IndexOf(obj);
            if(index >= 0)
            {
                SceneView.onSceneGUIDelegate -= _sceneFuncs[index].OnSceneGUI;

                _sceneFuncs.RemoveAt(index);
                _placers.RemoveAt(index);
            }
        }
    }
}

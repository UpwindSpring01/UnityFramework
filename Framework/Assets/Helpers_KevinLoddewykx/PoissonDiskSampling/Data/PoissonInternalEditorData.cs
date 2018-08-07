#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
    public partial class PoissonInternalEditorData
    {
        [System.Serializable]
        public class GridPoint
        {
            [SerializeField]
            public Vector2 Point = Vector2.zero;
            [SerializeField]
            public bool HasObject = false;
        }

        [System.Serializable]
        public class GameObjectList : List<GameObject>, ISerializationCallbackReceiver
        {
            [SerializeField]
            private List<GameObject> _objects;

            public void OnBeforeSerialize()
            {
                _objects = this;
            }

            public void OnAfterDeserialize()
            {
                Clear();
                foreach (GameObject obj in _objects)
                {
                    Add(obj);
                }
            }
        }

        [System.Serializable]
        public class Vector2List : List<GridPoint>, ISerializationCallbackReceiver
        {
            [SerializeField]
            private List<GridPoint> _objects;

            public void OnBeforeSerialize()
            {
                _objects = this;
            }

            public void OnAfterDeserialize()
            {
                Clear();
                foreach (GridPoint obj in _objects)
                {
                    Add(obj);
                }
            }
        }

        [System.Serializable]
        public class StoredGrid
        {
            [SerializeField]
            public Vector2List[] Grid2D = null;
            [SerializeField]
            public int GridWidth = 0;
            [SerializeField]
            public int GridDepth = 0;

            [SerializeField]
            public bool ReadOnly = false;
        }

        [SerializeField]
        public List<GameObjectList> PlacedObjects = new List<GameObjectList>() { new GameObjectList() };

        [SerializeField]
        public List<StoredGrid> Grids = new List<StoredGrid>() { new StoredGrid() };

        [SerializeField]
        public int HighestDistributedLevel = -1;

        [SerializeField]
        public bool LastFrameValid = true;

        /*******************
        * Visual
        * *****************/
        [SerializeField]
        public GameObject HelperVisual = null;

        [SerializeField]
        public Material TopMaterial = null;
        [SerializeField]
        public Material FaceMaterial = null;

        [SerializeField]
        public MeshRenderer Renderer = null;
        [SerializeField]
        public MeshFilter MeshFilter = null;
        [SerializeField]
        public PoissonDeleteHelper DeleteHelper = null;

        [SerializeField]
        public Mesh BoxMesh = null;
        [SerializeField]
        public Mesh CylinderMesh = null;
        [SerializeField]
        public Mesh PlaneMesh = null;
        [SerializeField]
        public Mesh EllipseMesh = null;
    }
}
#endif
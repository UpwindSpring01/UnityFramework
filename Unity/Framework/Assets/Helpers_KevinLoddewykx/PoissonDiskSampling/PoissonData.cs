using Helpers_KevinLoddewykx.General;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public enum EBoundsMode
    {
        Renderer,
        Collider
    }

    [System.Serializable]
    public class PoissonData
    {
        [System.Serializable]
        public class ObjectOptions
        {
            public Transform Parent = null;
            public float MinScale = 0.5f;
            public float MaxScale = 2.0f;
            public bool RotateX = false;
            public bool RotateY = false;
            public bool RotateZ = false;
            public bool ScaleX = false;
            public bool ScaleY = false;
            public bool ScaleZ = false;
            public bool AlignToSurface = false;
            public bool UniformScaling = false;
            public float MinHeightOffset = 0.0f;
            public float MaxHeightOffset = 0.0f;
            public float MinDot = -1.0f;
            public float MaxDot = 1.0f;

            public ObjectOptions ShallowClone()
            {
                return (ObjectOptions)MemberwiseClone();
            }
        }

        [SerializeField]
        public Texture2D Map = null;

        [SerializeField]
        public LayerMask ProjectionLayerMask = ~0;
        [SerializeField]
        public QueryTriggerInteraction ProjectionRaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

        [SerializeField]
        public float MinDist = 5.0f;
        [SerializeField]
        public float MaxDist = 50.0f;
        [SerializeField]
        public float DistToKeepNextLevel = 0.0f;

        [SerializeField]
        public int MinClump = 0;
        [SerializeField]
        public int MaxClump = 5;

        [SerializeField]
        public float MinClumpRange = 1;
        [SerializeField]
        public float MaxClumpRange = 5;

        [SerializeField]
        public bool UseSeed = false;
        [SerializeField]
        public int Seed = 0;
        [SerializeField]
        public int Samples = 10;

        [SerializeField]
        public bool SphereCollisionCheck = false;
        [SerializeField]
        public bool BoxCollisionCheck = false;

        [SerializeField]
        public BaseWeightedCollection PoissonObjects;
        [SerializeField]
        public BaseWeightedCollection ClumpObjects;

        [SerializeField]
        public ObjectOptions[] PoissonObjectOptions;
        [SerializeField]
        public ObjectOptions[] ClumpObjectOptions;

        [SerializeField]
        public LayerMask OverlapLayerMask = ~0;
        [SerializeField]
        public QueryTriggerInteraction OverlapRaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

        [SerializeField]
        public EBoundsMode BoundsMode = EBoundsMode.Renderer;

        [SerializeField]
        public int MaxSamples = 0;

        [SerializeField]
        public int MaxSamplesPreview = 10000;

        public PoissonData DeepCopy()
        {
            PoissonData data = (PoissonData)MemberwiseClone();
            if (PoissonObjectOptions != null)
            {
                data.PoissonObjectOptions = new ObjectOptions[PoissonObjectOptions.Length];
                for (int i = 0; i < PoissonObjectOptions.Length; ++i)
                {
                    data.PoissonObjectOptions[i] = PoissonObjectOptions[i].ShallowClone();
                }
            }

            if (ClumpObjectOptions != null)
            {
                data.ClumpObjectOptions = new ObjectOptions[ClumpObjectOptions.Length];
                for (int i = 0; i < ClumpObjectOptions.Length; ++i)
                {
                    data.ClumpObjectOptions[i] = ClumpObjectOptions[i].ShallowClone();
                }
            }
            return data;
        }
    }


}
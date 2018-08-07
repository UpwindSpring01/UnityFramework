#if UNITY_EDITOR
using Helpers_KevinLoddewykx.General;
using Helpers_KevinLoddewykx.General.WeightedArrayCore;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
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
            public bool ScaleHeightOffset = false;

            public float MinHeightOffset = 0.0f;
            public float MaxHeightOffset = 0.0f;

            public float MinDot = -1.0f;
            public float MaxDot = 1.0f;

            public ObjectOptions ShallowClone()
            {
                return (ObjectOptions)MemberwiseClone();
            }

            public static void Copy(ObjectOptions from, ObjectOptions to)
            {
                to.Parent = from.Parent;

                to.MinScale = from.MinScale;
                to.MaxScale = from.MaxScale;

                to.RotateX = from.RotateX;
                to.RotateY = from.RotateY;
                to.RotateZ = from.RotateZ;

                to.ScaleX = from.ScaleX;
                to.ScaleY = from.ScaleY;
                to.ScaleZ = from.ScaleZ;

                to.AlignToSurface = from.AlignToSurface;
                to.UniformScaling = from.UniformScaling;
                to.ScaleHeightOffset = from.ScaleHeightOffset;

                to.MinHeightOffset = from.MinHeightOffset;
                to.MaxHeightOffset = from.MaxHeightOffset;

                to.MinDot = from.MinDot;
                to.MaxDot = from.MaxDot;
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
        public WeightedScriptableObject PoissonObjects;
        [SerializeField]
        public WeightedScriptableObject ClumpObjects;

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
        [SerializeField]
        public int MaxSubPlacersNesting = 1;

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

        public static void Copy(PoissonData from, PoissonData to)
        {
            to.Map = from.Map;

            to.ProjectionLayerMask = from.ProjectionLayerMask;
            to.ProjectionRaycastTriggerInteraction = from.ProjectionRaycastTriggerInteraction;

            to.MinDist = from.MinDist;
            to.MaxDist = from.MaxDist;
            to.DistToKeepNextLevel = from.DistToKeepNextLevel;

            to.MinClump = from.MinClump;
            to.MaxClump = from.MaxClump;

            to.MinClumpRange = from.MinClumpRange;
            to.MaxClumpRange = from.MaxClumpRange;

            to.UseSeed = from.UseSeed;
            to.Seed = from.Seed;
            to.Samples = from.Samples;

            to.SphereCollisionCheck = from.SphereCollisionCheck;
            to.BoxCollisionCheck = from.BoxCollisionCheck;

            to.PoissonObjects = from.PoissonObjects;
            to.ClumpObjects = from.ClumpObjects;

            int fromLength = (from.PoissonObjectOptions == null) ? -1 : from.PoissonObjectOptions.Length;
            int toLength = (to.PoissonObjectOptions == null) ? -1 : to.PoissonObjectOptions.Length;
            if (fromLength != toLength)
            {
                if (fromLength == -1)
                {
                    to.PoissonObjectOptions = null;
                }
                else
                {
                    to.PoissonObjectOptions = new ObjectOptions[fromLength];
                    to.PoissonObjectOptions.InitNew();
                }
            }
            for(int i = 0; i < fromLength; ++i)
            {
                ObjectOptions.Copy(from.PoissonObjectOptions[i], to.PoissonObjectOptions[i]);
            }

            fromLength = (from.ClumpObjectOptions == null) ? -1 : from.ClumpObjectOptions.Length;
            toLength = (to.ClumpObjectOptions == null) ? -1 : to.ClumpObjectOptions.Length;
            if (fromLength != toLength)
            {
                if (fromLength == -1)
                {
                    to.ClumpObjectOptions = null;
                }
                else
                {
                    to.ClumpObjectOptions = new ObjectOptions[fromLength];
                    to.ClumpObjectOptions.InitNew();
                }
            }
            for (int i = 0; i < fromLength; ++i)
            {
                ObjectOptions.Copy(from.ClumpObjectOptions[i], to.ClumpObjectOptions[i]);
            }

            to.OverlapLayerMask = from.OverlapLayerMask;
            to.OverlapRaycastTriggerInteraction = from.OverlapRaycastTriggerInteraction;
            to.BoundsMode = from.BoundsMode;

            to.MaxSamples = from.MaxSamples;
            to.MaxSamplesPreview = from.MaxSamplesPreview;
            to.MaxSubPlacersNesting = from.MaxSubPlacersNesting;
        }
    }
}
#endif
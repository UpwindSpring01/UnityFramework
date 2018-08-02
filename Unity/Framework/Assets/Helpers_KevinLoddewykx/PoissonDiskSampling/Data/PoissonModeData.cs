#if UNITY_EDITOR
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
    public enum DistributionMode
    {
        Surface,
        Plane,
        Ellipse,
        ProjectionRect,
        ProjectionEllipse
    }

    [System.Serializable]
    public class PoissonModeData
    {
        [SerializeField]
        public DistributionMode Mode = DistributionMode.Surface;

        [SerializeField]
        public MeshFilter SurfaceMeshFilter = null;

        [SerializeField]
        public bool RealtimePreview = false;

        [SerializeField]
        public float RejectPercentageX = 0.0f;

        [SerializeField]
        public float RejectPercentageY = 0.0f;

        [SerializeField]
        public Vector3 Position = Vector3.zero;
        [SerializeField]
        public Quaternion Rotation = Quaternion.identity;
        [SerializeField]
        public Vector3 Scale = Vector3.one;

        public PoissonModeData DeepCopy()
        {
            return (PoissonModeData)MemberwiseClone();
        }

        public static void Copy(PoissonModeData from, PoissonModeData to)
        {
            to.Mode = from.Mode;
            to.SurfaceMeshFilter = from.SurfaceMeshFilter;

            to.RealtimePreview = from.RealtimePreview;
            to.RejectPercentageX = from.RejectPercentageX;
            to.RejectPercentageY = from.RejectPercentageY;

            to.Position.x = from.Position.x;
            to.Position.y = from.Position.y;
            to.Position.z = from.Position.z;

            to.Rotation.x = from.Rotation.x;
            to.Rotation.y = from.Rotation.y;
            to.Rotation.z = from.Rotation.z;
            to.Rotation.w = from.Rotation.w;

            to.Scale.x = from.Scale.x;
            to.Scale.y = from.Scale.y;
            to.Scale.z = from.Scale.z;
        }
    }
}
#endif
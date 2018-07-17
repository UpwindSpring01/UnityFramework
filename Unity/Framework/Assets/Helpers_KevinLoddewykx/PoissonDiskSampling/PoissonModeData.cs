using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
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
        public Vector2 RejectPercentage = Vector2.zero;
    }
}
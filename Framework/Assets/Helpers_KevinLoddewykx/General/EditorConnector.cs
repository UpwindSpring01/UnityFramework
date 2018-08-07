
using Helpers_KevinLoddewykx.PoissonDiskSampling;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    public static class EditorConnector
    {
        public static IPoissonConnector Connector { get; set; }
    }

    public interface IPoissonConnector
    {
        void Reset(IPoissonDataHolder behaviour);

        void Register(IPoissonDataHolder behaviour);

        void Unregister(IPoissonDataHolder behaviour);
    }
}
using Helpers_KevinLoddewykx.General;
using Helpers_KevinLoddewykx.PoissonDiskSampling;
using UnityEditor;

namespace Helpers_KevinLoddewykx
{
    [InitializeOnLoad]
    public class StartUpConnector
    {
        static StartUpConnector()
        {
            EditorConnector.Connector = PoissonHelperInternalStorage.Instance;
        }
    }
}

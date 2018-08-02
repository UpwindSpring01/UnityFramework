using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [CustomEditor(typeof(PoissonDeleteHelper))]
    public class PoissonDeleteHelperEditor : Editor
    {
        private void OnSceneGUI()
        {
            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "SoftDelete")
                {
                    PoissonDeleteHelper helper = target as PoissonDeleteHelper;
                    if (helper && helper.Placer)
                    {
                        PoissonHelperInternalStorage.Instance.Remove(helper.Placer);
                        Undo.DestroyObjectImmediate(helper.Placer);
                    }
                }
            }
        }
    }
}

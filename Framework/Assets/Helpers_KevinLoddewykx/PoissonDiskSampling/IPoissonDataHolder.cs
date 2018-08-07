#if UNITY_EDITOR
using System.Collections.Generic;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public interface IPoissonDataHolder
    {
        PoissonModeData ModeData { get; set; }

        List<PoissonData> Data { get; set; }

        PoissonUIData UIData { get; set; }

        PoissonInternalEditorData EditorData { get; set; }

        bool IsWindow { get; }

        void VisualTransformChanged();
    }
}
#endif

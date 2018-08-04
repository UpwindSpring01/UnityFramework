using Helpers_KevinLoddewykx.General;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
    public partial class PoissonHelper
    {
        private List<PoissonData> Data { get; set; }

        public IPoissonDataHolder DataHolder { get; private set; }

        private bool IsPrefab { get; }

        public PoissonInternalEditorData EditorData { get; }
        public PoissonModeData ModeData { get; private set; }

        public PoissonUIData UIData { get; private set; }

        public PoissonData SelectedData { get; private set; }


        public PoissonHelper(IPoissonDataHolder dataHolder)
        {
            DataHolder = dataHolder;
            EditorData = dataHolder.EditorData;

            IsPrefab = !DataHolder.IsWindow && (PrefabUtility.GetPrefabType((Object)dataHolder) == PrefabType.Prefab);
        }

        public void Init()
        {
            if (!IsPrefab)
            {
                bool existed = PoissonHelperInternalStorage.Instance.RemoveAndAdd(DataHolder, this);

                EditorData.InitVisual(DataHolder.ModeData, DataHolder.Data[DataHolder.UIData.SelectedLevelIndex], (DataHolder.IsWindow) ? null : (PoissonPlacer)DataHolder, DataHolder.IsWindow, !existed);
                if(!DataHolder.IsWindow)
                {
                    DataHolder.ModeData.Surface = ((PoissonPlacer)DataHolder).gameObject;
                }
            }
        }

        public void ShutDown()
        {
            if (!IsPrefab)
            {
                PoissonHelperInternalStorage.Instance.Remove(DataHolder);
                EditorData.DestroyVisual(DataHolder.ModeData);
            }
        }

        public void StopUndoRedoTracking()
        {
            PoissonHelperInternalStorage.Instance.RemoveUndoRedoTracking(DataHolder);
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (!EditorData.HelperVisual)
            {
                PoissonHelperInternalStorage.Instance.Remove(DataHolder);
                return;
            }
            if (!IsPrefab && EditorData.HelperVisual.transform.hasChanged)
            {
                LoadDataHolder();

                ModeData.Position = EditorData.HelperVisual.transform.localPosition;
                ModeData.Rotation = EditorData.HelperVisual.transform.localRotation;
                ModeData.Scale = EditorData.HelperVisual.transform.localScale;

                if (DataHolder.ModeData.Mode != DistributionMode.Surface)
                {
                    DistributeRealtime();
                    if (DataHolder.IsWindow)
                    {
                        ((EditorWindow)DataHolder).Repaint();
                    }
                }
                EditorData.HelperVisual.transform.hasChanged = false;

                Undo.RecordObject((Object)DataHolder, "PDS Param");
                StoreDataHolder();
                DataHolder.VisualTransformChanged();
            }
        }

        private void DistributeRealtime()
        {
            if (ModeData.RealtimePreview)
            {
                bool isValidSurface, preValid, currValid, postValid;
                int highestValid;
                ValidateSettings(false, out isValidSurface, out preValid, out currValid, out postValid, out highestValid);

                EditorData.LastFrameValid = isValidSurface && preValid && currValid && postValid;
                if (EditorData.LastFrameValid)
                {
                    DistributePoisson(0, highestValid, true);
                }
            }
        }

        public void OnUndoRedoPerformedPlacer()
        {
            if (EditorData.HelperVisual)
            {
                RefreshDistribution();
            }
        }

        public void RefreshDistribution()
        {
            LoadDataHolder();
            EditorData.RefreshVisual(ModeData, SelectedData, DataHolder.IsWindow);
            DistributeRealtime();
        }

        private void StoreDataHolder()
        {
            PoissonModeData.Copy(ModeData, DataHolder.ModeData);
            PoissonUIData.Copy(UIData, DataHolder.UIData);

            DataHolder.Data.Resize(Data.Count);
            for (int i = 0; i < Data.Count; ++i)
            {
                PoissonData.Copy(Data[i], DataHolder.Data[i]);
            }
        }

        private void LoadDataHolder()
        {
            ModeData = DataHolder.ModeData.DeepCopy();
            UIData = DataHolder.UIData.DeepCopy();

            Data = new List<PoissonData>(DataHolder.Data.Count);
            foreach (PoissonData level in DataHolder.Data)
            {
                Data.Add(level.DeepCopy());
            }

            SelectedData = Data[UIData.SelectedLevelIndex];
        }
    }
}
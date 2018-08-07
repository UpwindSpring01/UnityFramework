#if UNITY_EDITOR
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    [System.Serializable]
    public class PoissonUIData
    {
        [SerializeField]
        public int PoissonSelected = 0;
        [SerializeField]
        public int ClumpSelected = 0;

        [SerializeField]
        public bool DuplicateLevel = false;
        [SerializeField]
        public int InsertLevelAt = 1;
        [SerializeField]
        public int SelectedLevelIndex = 0;

        // Foldouts
        [SerializeField]
        public bool LevelCategory = true;
        [SerializeField]
        public bool GeneralCategory = true;
        [SerializeField]
        public bool ModeCategory = true;
        [SerializeField]
        public bool PoissonCategory = true;
        [SerializeField]
        public bool ClumpCategory = false;

        public PoissonUIData DeepCopy()
        {
            return (PoissonUIData)MemberwiseClone();
        }

        public static void Copy(PoissonUIData from, PoissonUIData to)
        {
            to.PoissonSelected = from.PoissonSelected;
            to.ClumpSelected = from.ClumpSelected;

            to.DuplicateLevel = from.DuplicateLevel;
            to.InsertLevelAt = from.InsertLevelAt;
            to.SelectedLevelIndex = from.SelectedLevelIndex;

            to.LevelCategory = from.LevelCategory;
            to.GeneralCategory = from.GeneralCategory;
            to.ModeCategory = from.ModeCategory;
            to.PoissonCategory = from.PoissonCategory;
            to.ClumpCategory = from.ClumpCategory;
        }
    }
}
#endif
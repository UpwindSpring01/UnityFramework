using UnityEditor;
using UnityEngine;
using System.Linq;
using Helpers_KevinLoddewykx.General;
using Helpers_KevinLoddewykx.General.WeightedArrayCore;
using System.Collections.Generic;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonData;
using UnityEditorInternal;
using System;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonInternalEditorData;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonHelper
    {
        private Vector2 _masterScroll = Vector2.zero;

        private static readonly Color BACKGROUND_COLOR_SUB_MENU = new Color(0.70f, 0.70f, 0.70f);
        private static readonly float LABEL_WIDTH = 120.0f;

        private GUIStyle LeftColumnStyle;
        private GUIStyle RightColumnStyle;
        private GUIStyle RowStyle;
        private GUIStyle SingleRowStyle;
        private GUIStyle BoxStyle;
        private GUIStyle SubBoxStyle;
        private GUIStyle SubLeftColumnStyle;
        private GUIStyle LeftColumnFoldoutStyle;

        private GUIStyle ButtonStyle;
        private GUIStyle NumberFieldStyle;
        private GUIStyle PopupStyle;
        private GUIStyle FoldoutStyle;
        private GUIStyle ToggleStyle;
        private GUIStyle SliderStyle;

        private const int MinimumWidth = 420;
        private const int BoxMargin = 3;
        private const int ColumnGap = 5;
        private int LeftMarginInspector;

        private bool _modeHasChanged = false;

        private void InitStyles()
        {
            LeftMarginInspector = EditorStyles.inspectorDefaultMargins.padding.left;
            RowStyle = new GUIStyle();
            SingleRowStyle = new GUIStyle();
            LeftColumnFoldoutStyle = new GUIStyle();
            BoxStyle = new GUIStyle(GUI.skin.box);
            SubBoxStyle = new GUIStyle(GUI.skin.box);

            if (DataHolder.IsWindow)
            {
                LeftColumnStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                RightColumnStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                SubLeftColumnStyle = LeftColumnStyle;

                LeftColumnStyle.margin.left = 0;

                BoxStyle.margin.left = 0;
                BoxStyle.margin.right = 0;
                BoxStyle.padding.left = BoxMargin;
                BoxStyle.padding.right = BoxMargin;

                SubBoxStyle.margin.left = 0;
                SubBoxStyle.margin.right = 0;
                SubBoxStyle.padding.left = BoxMargin;
                SubBoxStyle.padding.right = BoxMargin;
            }
            else
            {
                LeftColumnStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                RightColumnStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                SubLeftColumnStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);

                LeftColumnStyle.margin.left = LeftMarginInspector;

                BoxStyle.margin.left = 0;
                BoxStyle.margin.right = 0;
                BoxStyle.padding.left = 0;
                BoxStyle.padding.right = BoxMargin;

                SubBoxStyle.margin.left = BoxMargin;
                SubBoxStyle.margin.right = 0;
                SubBoxStyle.padding.left = 0;
                SubBoxStyle.padding.right = BoxMargin;

                SubLeftColumnStyle.margin.left = LeftMarginInspector - BoxMargin;
                SubLeftColumnStyle.margin.right = ColumnGap;
                SubLeftColumnStyle.padding.left = 0;
                SubLeftColumnStyle.padding.right = 0;

                SingleRowStyle.margin.left = LeftMarginInspector - BoxMargin;
            }

            LeftColumnFoldoutStyle.margin.left = 0;
            LeftColumnFoldoutStyle.margin.right = ColumnGap;
            LeftColumnFoldoutStyle.padding.left = 0;
            LeftColumnFoldoutStyle.padding.right = 0;

            LeftColumnStyle.margin.right = ColumnGap;
            LeftColumnStyle.padding.left = 0;
            LeftColumnStyle.padding.right = 0;

            RightColumnStyle.margin.left = ColumnGap;
            RightColumnStyle.margin.right = 0;
            RightColumnStyle.padding.left = 0;
            RightColumnStyle.padding.right = 0;

            // Control Styles
            FoldoutStyle = new GUIStyle(EditorStyles.foldout);
            FoldoutStyle.margin.left = (DataHolder.IsWindow) ? 0 : LeftMarginInspector;
            FoldoutStyle.margin.right = 0;
            FoldoutStyle.fontStyle = FontStyle.Bold;

            PopupStyle = new GUIStyle(EditorStyles.popup);
            PopupStyle.margin.left = 0;
            PopupStyle.margin.right = 0;

            NumberFieldStyle = new GUIStyle(EditorStyles.numberField);
            NumberFieldStyle.margin.left = 0;
            NumberFieldStyle.margin.right = 0;

            ToggleStyle = new GUIStyle(EditorStyles.toggle);
            ToggleStyle.margin.left = 0;
            ToggleStyle.margin.right = 0;

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.margin.left = 0;
            ButtonStyle.margin.right = 0;
        }

        public void CreateUI()
        {
            LoadDataHolder();

            InitStyles();

            // FIXME: This is needed because EditorGUILayout.Slider and EditorGUILayout.VectorXField uses numberfield internally.
            // But resetting it, at the end of this function does not work, everything will be drawn then with the last setted setting.
            EditorStyles.numberField.margin.left = 0;
            EditorStyles.numberField.margin.right = 0;

            // This one ise used by EditorGUILayout.ObjectField internally
            EditorStyles.layerMaskField.margin.left = 0;
            EditorStyles.layerMaskField.margin.right = 0;

            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            bool hasChanged = false;

            _modeHasChanged = false;

            if (DataHolder.IsWindow)
            {
                _masterScroll = EditorGUILayout.BeginScrollView(_masterScroll);
            }
            EditorGUILayout.Space();
            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                float halfWidth = (Mathf.Max(MinimumWidth, EditorGUIUtility.currentViewWidth) - ColumnGap) * 0.5f;
                if (!DataHolder.IsWindow)
                {
                    halfWidth -= LeftMarginInspector * 0.5f;
                }
                EditorGUI.BeginChangeCheck();

                CreateModeUI(halfWidth);
                _modeHasChanged = EditorGUI.EndChangeCheck();

                CreateLevelUI(halfWidth);
                using (new EditorGUI.DisabledScope(EditorData.Grids[UIData.SelectedLevelIndex].ReadOnly))
                {
                    CreateGeneralUI(halfWidth);
                    CreatePoissonUI(halfWidth);
                    CreateClumpUI(halfWidth);
                }

                hasChanged = changeScope.changed;

                if (!IsPrefab)
                {
                    hasChanged |= CreateDistributionButtons(halfWidth, changeScope);
                }
            }

            EditorGUILayout.Space();
            if (DataHolder.IsWindow)
            {
                EditorGUILayout.EndScrollView();
            }

            if (!IsPrefab)
            {
                EditorData.HelperVisual.transform.hasChanged = false;
            }
            
            if (hasChanged)
            {
                Undo.RecordObject((UnityEngine.Object)DataHolder, "PDS Param");
                StoreDataHolder();
                DataHolder.VisualTransformChanged();
            }
        }

        private void CreateModeUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap + ((DataHolder.IsWindow) ? 0 : LeftMarginInspector)));
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;

            EditorGUILayout.BeginHorizontal(RowStyle);
            EditorGUILayout.BeginVertical(LeftColumnFoldoutStyle, GUILayout.MaxWidth(halfWidth + ((DataHolder.IsWindow) ? 0 : LeftMarginInspector)));
            EditorGUILayout.BeginHorizontal(RowStyle);

            UIData.ModeCategory = EditorGUILayout.Foldout(UIData.ModeCategory, "Mode", true, FoldoutStyle);

            bool isDisabled = EditorData.Grids[0].ReadOnly;
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                EditorGUI.BeginChangeCheck();
                ModeData.Mode = (DistributionMode)EditorGUILayout.EnumPopup(ModeData.Mode, PopupStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorData.UpdateVisualMode(ModeData);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
            ModeData.RealtimePreview = EditorGUILayout.Toggle("Realtime preview:", ModeData.RealtimePreview, ToggleStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (UIData.ModeCategory)
                {
                    CreateModeSpecificUI(halfWidth);
                }
            }

            EditorGUILayout.EndVertical();

            GUILayout.Label("", GUI.skin.horizontalSlider);
        }

        private void CreateLevelUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap + ((DataHolder.IsWindow) ? 0 : LeftMarginInspector)));
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;

            EditorGUILayout.BeginHorizontal(RowStyle);
            EditorGUILayout.BeginVertical(LeftColumnFoldoutStyle, GUILayout.MaxWidth(halfWidth + ((DataHolder.IsWindow) ? 0 : LeftMarginInspector)));
            EditorGUILayout.BeginHorizontal(RowStyle);
            UIData.LevelCategory = EditorGUILayout.Foldout(UIData.LevelCategory, "Level", true, FoldoutStyle);

            string[] levels = new string[Data.Count];
            int[] levelIndices = new int[Data.Count];
            List<string> levelsInsert = new List<string>();
            List<int> levelIndicesInsert = new List<int>();
            for (int i = 0; i < Data.Count; ++i)
            {
                levels[i] = "" + i;
                if (DataHolder.IsWindow && EditorData.Grids[i].ReadOnly)
                {
                    levels[i] += " (Applied)";
                }
                else
                {
                    levelsInsert.Add("" + i);
                    levelIndicesInsert.Add(i);
                    if (i <= EditorData.HighestDistributedLevel)
                    {
                        levels[i] += " (Distributed: " + EditorData.PlacedObjects[i].Count + ")";
                    }
                }
                levelIndices[i] = i;
            }
            levelsInsert.Add("" + Data.Count);
            levelIndicesInsert.Add(Data.Count);
            int newSelectedLevelIndex = EditorGUILayout.IntPopup(UIData.SelectedLevelIndex, levels, levelIndices, PopupStyle);
            if (newSelectedLevelIndex != UIData.SelectedLevelIndex)
            {
                UIData.SelectedLevelIndex = newSelectedLevelIndex;
                SelectedData = Data[newSelectedLevelIndex];

                EditorData.UpdateVisualTexture(ModeData, SelectedData);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (UIData.LevelCategory)
            {
                EditorGUILayout.BeginHorizontal(RowStyle);

                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                UIData.DuplicateLevel = EditorGUILayout.Toggle("Duplicate selected:", UIData.DuplicateLevel, ToggleStyle);
                using (new EditorGUI.DisabledGroupScope(Data.Count == 1))
                {
                    if (GUILayout.Button("Delete Selected Level", ButtonStyle))
                    {
                        CleanupPlacedObjects(EditorData, UIData.SelectedLevelIndex);
                        EditorData.HighestDistributedLevel = Math.Min(UIData.SelectedLevelIndex - 1, EditorData.HighestDistributedLevel);

                        Data.RemoveAt(UIData.SelectedLevelIndex);
                        EditorData.Grids.RemoveAt(UIData.SelectedLevelIndex);
                        EditorData.PlacedObjects.RemoveAt(UIData.SelectedLevelIndex);

                        UIData.SelectedLevelIndex = Mathf.Min(Data.Count - 1, UIData.SelectedLevelIndex);
                        UIData.InsertLevelAt = Mathf.Min(Data.Count, UIData.InsertLevelAt);
                        SelectedData = Data[UIData.SelectedLevelIndex];
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                UIData.InsertLevelAt = EditorGUILayout.IntPopup("Insert at:", UIData.InsertLevelAt, levelsInsert.ToArray(), levelIndicesInsert.ToArray(), PopupStyle);
                string addMode = UIData.DuplicateLevel ? "Duplicate Selected" : "Insert";
                if (GUILayout.Button(addMode + " Level At " + UIData.InsertLevelAt, ButtonStyle))
                {
                    CleanupPlacedObjects(EditorData, UIData.InsertLevelAt);
                    EditorData.HighestDistributedLevel = Mathf.Min(EditorData.HighestDistributedLevel, UIData.InsertLevelAt - 1);

                    if (UIData.DuplicateLevel)
                    {
                        Data.Insert(UIData.InsertLevelAt, SelectedData.DeepCopy());
                    }
                    else
                    {
                        Data.Insert(UIData.InsertLevelAt, new PoissonData());
                        EditorData.UpdateVisualTexture(ModeData, Data[UIData.InsertLevelAt]);
                    }
                    EditorData.Grids.Insert(UIData.InsertLevelAt, new StoredGrid());
                    EditorData.PlacedObjects.Insert(UIData.InsertLevelAt, new GameObjectList());

                    UIData.SelectedLevelIndex = UIData.InsertLevelAt;
                    SelectedData = Data[UIData.SelectedLevelIndex];
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateGeneralUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;

            if (UIData.GeneralCategory = EditorGUILayout.Foldout(UIData.GeneralCategory, "General", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.UseSeed = EditorGUILayout.Toggle(new GUIContent("Use seed:"), SelectedData.UseSeed, ToggleStyle);
                SelectedData.MaxSubPlacersNesting = Math.Max(0, EditorGUILayout.IntField(new GUIContent("Max nesting:", "How deep nested subplacers can be triggered."), SelectedData.MaxSubPlacersNesting));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(!SelectedData.UseSeed))
                {
                    SelectedData.Seed = EditorGUILayout.IntField(new GUIContent("Seed:"), SelectedData.Seed, NumberFieldStyle);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = BACKGROUND_COLOR_SUB_MENU;
                EditorGUILayout.BeginHorizontal(SubBoxStyle);
                halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;
                GUI.backgroundColor = oldColor;

                EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("Overlap Test Settings", EditorStyles.boldLabel);
                SelectedData.SphereCollisionCheck = EditorGUILayout.Toggle(new GUIContent("Sphere:"), SelectedData.SphereCollisionCheck, ToggleStyle);
                SelectedData.BoxCollisionCheck = EditorGUILayout.Toggle(new GUIContent("Box:"), SelectedData.BoxCollisionCheck, ToggleStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.BoundsMode = (EBoundsMode)EditorGUILayout.EnumPopup("Mode:", SelectedData.BoundsMode, PopupStyle);
                SelectedData.OverlapLayerMask = EditorGUILayout.MaskField("Layer mask:", SelectedData.OverlapLayerMask, InternalEditorUtility.layers, PopupStyle);
                SelectedData.OverlapRaycastTriggerInteraction = (QueryTriggerInteraction)EditorGUILayout.EnumPopup("Trigger query mode:", SelectedData.OverlapRaycastTriggerInteraction, PopupStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreatePoissonUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;

            if (UIData.PoissonCategory = EditorGUILayout.Foldout(UIData.PoissonCategory, "Poisson", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.MaxSamples = EditorGUILayout.IntField(new GUIContent("Max Samples:", "Max samples: <= 0 for no limit"), SelectedData.MaxSamples, NumberFieldStyle);
                EditorGUI.BeginChangeCheck();
                SelectedData.Map = (Texture2D)EditorGUILayout.ObjectField("Map:", SelectedData.Map, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorData.UpdateVisualTexture(ModeData, SelectedData);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.MaxSamplesPreview = EditorGUILayout.IntField(new GUIContent("Max Preview:", "Max samples for realtime preview: <= 0 for no limit"), SelectedData.MaxSamplesPreview, NumberFieldStyle);
                SelectedData.Samples = EditorGUILayout.IntField(new GUIContent("Samples/Object:"), SelectedData.Samples, NumberFieldStyle);
                SelectedData.MinDist = EditorGUILayout.FloatField(new GUIContent("Min distance:"), SelectedData.MinDist, NumberFieldStyle);
                if (SelectedData.MinDist <= 0.0f)
                {
                    SelectedData.MinDist = 1.0f;
                }
                using (new EditorGUI.DisabledScope(SelectedData.Map == null))
                {
                    if (SelectedData.MaxDist <= 0.0f)
                    {
                        SelectedData.MaxDist = 10.0f;
                    }
                    SelectedData.MaxDist = EditorGUILayout.FloatField(new GUIContent("Max distance:"), SelectedData.MaxDist, NumberFieldStyle);
                }

                SelectedData.DistToKeepNextLevel = EditorGUILayout.FloatField(new GUIContent("Distance Next Level:", "The distance the next level needs to keep from the distributed points."), SelectedData.DistToKeepNextLevel, NumberFieldStyle);
                float maxDist = (SelectedData.Map == null) ? SelectedData.MinDist : Mathf.Max(SelectedData.MaxDist, SelectedData.MinDist);
                SelectedData.DistToKeepNextLevel = Mathf.Min(SelectedData.DistToKeepNextLevel, maxDist);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUI.BeginChangeCheck();
                SelectedData.PoissonObjects = (WeightedScriptableObject)EditorGUILayout.ObjectField("Poisson Data:", SelectedData.PoissonObjects, typeof(WeightedScriptableObject), false);
                int poissonObjectsCount = 0;
                if (SelectedData.PoissonObjects?.Element.HasWeightedElementsNonNull() ?? false)
                {
                    // While you can not set settings for objects which equals to null, still reserve space for them in PoissonObjectOptions. 
                    // So we can access them with index of the gameobject position inside the WeightedArray .
                    poissonObjectsCount = SelectedData.PoissonObjects.Element.Objects.Count;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SelectedData.PoissonObjectOptions = new ObjectOptions[poissonObjectsCount];
                    SelectedData.PoissonObjectOptions.InitNew();
                }
                else if ((SelectedData.PoissonObjectOptions?.Length ?? 0) != poissonObjectsCount)
                {
                    List<ObjectOptions> options = SelectedData.PoissonObjectOptions.ToList();
                    options.Resize(poissonObjectsCount);
                    SelectedData.PoissonObjectOptions = options.ToArray();

                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                CreateOptions(SelectedData.PoissonObjects?.Element, SelectedData.PoissonObjectOptions, ref UIData.PoissonSelected, halfWidth);
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateClumpUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;

            if (UIData.ClumpCategory = EditorGUILayout.Foldout(UIData.ClumpCategory, "Clumping", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.MinClump = EditorGUILayout.IntField(new GUIContent("Min clumping:"), SelectedData.MinClump, NumberFieldStyle);
                SelectedData.MinClumpRange = EditorGUILayout.FloatField(new GUIContent("Min clump range:"), SelectedData.MinClumpRange, NumberFieldStyle);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                SelectedData.MaxClump = EditorGUILayout.IntField(new GUIContent("Max clumping:"), SelectedData.MaxClump, NumberFieldStyle);
                SelectedData.MaxClumpRange = EditorGUILayout.FloatField(new GUIContent("Max clump range:"), SelectedData.MaxClumpRange, NumberFieldStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUI.BeginChangeCheck();
                SelectedData.ClumpObjects = (WeightedScriptableObject)EditorGUILayout.ObjectField("Clumping Data:", SelectedData.ClumpObjects, typeof(WeightedScriptableObject), false);
                int clumpObjectsCount = 0;
                if (SelectedData.ClumpObjects?.Element.HasWeightedElementsNonNull() ?? false)
                {
                    // While you can not set settings for objects which equals to null, still reserve space for them in ClumpObjectOptions. 
                    // So we can access them with index of the gameobject position inside the WeightedArray .
                    clumpObjectsCount = SelectedData.ClumpObjects.Element.Objects.Count;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SelectedData.ClumpObjectOptions = new ObjectOptions[clumpObjectsCount];
                    SelectedData.ClumpObjectOptions.InitNew();
                }
                else if ((SelectedData.ClumpObjectOptions?.Length ?? 0) != clumpObjectsCount)
                {
                    List<ObjectOptions> options = SelectedData.ClumpObjectOptions.ToList();
                    options.Resize(clumpObjectsCount);
                    SelectedData.ClumpObjectOptions = options.ToArray();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                CreateOptions(SelectedData.ClumpObjects?.Element, SelectedData.ClumpObjectOptions, ref UIData.ClumpSelected, halfWidth);
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateOptions(WeightedArray objects, ObjectOptions[] options, ref int selectedIndex, float halfWidth)
        {
            bool disabled = (!objects?.HasWeightedElementsNonNull()) ?? true;

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = BACKGROUND_COLOR_SUB_MENU;
            EditorGUILayout.BeginVertical(SubBoxStyle);
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;
            GUI.backgroundColor = oldColor;

            using (new EditorGUI.DisabledScope(disabled))
            {
                GUIStyle centerStyle = new GUIStyle();
                centerStyle.alignment = TextAnchor.MiddleCenter;

                string[] objectStrings = null;
                IEnumerable<Tuple<WeightedObject, int>> objectsNonNull = null;

                if (!disabled)
                {
                    objectsNonNull = objects.Objects
                        .Select((o, i) => new Tuple<WeightedObject, int>(o, i))
                        .Where((o) => o.Item1.Object != null && o.Item1.Weight > 0);

                    objectStrings = objectsNonNull
                        .Select((a, i) => i + ": " + objects.GetChance(a.Item1).ToString("F2") + "%) " + a.Item1.Object.name)
                        .ToArray();

                    if (selectedIndex >= objectStrings.Count())
                    {
                        selectedIndex = Mathf.Max(objectStrings.Count() - 1, 0);
                    }
                }
                else
                {
                    objectStrings = new string[] { "" };
                    selectedIndex = 0;
                }

                ObjectOptions selectedOptions;
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                
                // Pass in an empty label or set EditorGUIUtility.fieldWidth to the EditorGUIUtility.labelWidth (== LABEL_WIDTH)
                // otherwise the width will be incorrect when the vertical scrollbar is visible
                selectedIndex = EditorGUILayout.Popup("", selectedIndex, objectStrings, PopupStyle);
                if (disabled)
                {
                    selectedOptions = new ObjectOptions();
                }
                else
                {
                    selectedOptions = options[objectsNonNull.ElementAt(selectedIndex).Item2];
                }
                EditorGUIUtility.fieldWidth = 0;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(RowStyle);
                using (new EditorGUILayout.VerticalScope(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth)))
                {
                    selectedOptions.Parent = (Transform)EditorGUILayout.ObjectField("Parent:", selectedOptions.Parent, typeof(Transform), true);

                    if (selectedOptions.Parent != null)
                    {
                        PrefabType prefabType = PrefabUtility.GetPrefabType(selectedOptions.Parent);
                        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
                        {
                            selectedOptions.Parent = null;
                        }
                    }

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUIUtility.labelWidth = LABEL_WIDTH;
                    EditorGUILayout.PrefixLabel("Rotate:", RowStyle);
                    EditorGUIUtility.labelWidth = 12.0f;
                    using (new EditorGUI.DisabledScope(selectedOptions.AlignToSurface == true))
                    {
                        selectedOptions.RotateX = EditorGUILayout.Toggle("X", selectedOptions.RotateX, ToggleStyle, GUILayout.ExpandWidth(false));
                    }
                    selectedOptions.RotateY = EditorGUILayout.Toggle("Y", selectedOptions.RotateY, ToggleStyle, GUILayout.ExpandWidth(false));
                    using (new EditorGUI.DisabledScope(selectedOptions.AlignToSurface == true))
                    {
                        selectedOptions.RotateZ = EditorGUILayout.Toggle("Z", selectedOptions.RotateZ, ToggleStyle, GUILayout.ExpandWidth(false));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUIUtility.labelWidth = LABEL_WIDTH;
                    EditorGUILayout.PrefixLabel("Scale:", RowStyle);
                    EditorGUIUtility.labelWidth = 12.0f;
                    selectedOptions.ScaleX = EditorGUILayout.Toggle("X", selectedOptions.ScaleX, ToggleStyle, GUILayout.ExpandWidth(false));
                    selectedOptions.ScaleY = EditorGUILayout.Toggle("Y", selectedOptions.ScaleY, ToggleStyle, GUILayout.ExpandWidth(false));
                    selectedOptions.ScaleZ = EditorGUILayout.Toggle("Z", selectedOptions.ScaleZ, ToggleStyle, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = LABEL_WIDTH;
                    selectedOptions.AlignToSurface = EditorGUILayout.Toggle("Align Z to surface:", selectedOptions.AlignToSurface, ToggleStyle);
                    selectedOptions.UniformScaling = EditorGUILayout.Toggle("Uniform scaling:", selectedOptions.UniformScaling, ToggleStyle);
                }
                using (new EditorGUILayout.VerticalScope(RightColumnStyle, GUILayout.MaxWidth(halfWidth)))
                {
                    // Magic number 9
                    EditorGUIUtility.fieldWidth = (halfWidth - LABEL_WIDTH) * 0.5f - 9.0f;

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    // Add non-empty label to skip the first column
                    EditorGUILayout.PrefixLabel(" ", EditorStyles.objectField, RowStyle);
                    // Because unity layouting is pain
                    EditorGUIUtility.labelWidth = 0.0000001f;
                    EditorGUILayout.LabelField("Min", centerStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("Max", centerStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = LABEL_WIDTH;

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Scale:", RowStyle);
                    selectedOptions.MinScale = EditorGUILayout.FloatField(selectedOptions.MinScale, NumberFieldStyle);
                    selectedOptions.MaxScale = EditorGUILayout.FloatField(selectedOptions.MaxScale, NumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Height offset:", RowStyle);
                    selectedOptions.MinHeightOffset = EditorGUILayout.FloatField(selectedOptions.MinHeightOffset, NumberFieldStyle);
                    selectedOptions.MaxHeightOffset = EditorGUILayout.FloatField(selectedOptions.MaxHeightOffset, NumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Face . Up:", RowStyle);
                    selectedOptions.MinDot = EditorGUILayout.FloatField(selectedOptions.MinDot, NumberFieldStyle);
                    selectedOptions.MaxDot = EditorGUILayout.FloatField(selectedOptions.MaxDot, NumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.fieldWidth = 0;
                    using (new EditorGUI.DisabledScope(selectedOptions.ScaleY == false))
                    {
                        selectedOptions.ScaleHeightOffset = EditorGUILayout.Toggle(new GUIContent("Scale height offset:", "If enabled the height offset is scaled by the calculated Y scale"), selectedOptions.ScaleHeightOffset, ToggleStyle);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private bool CreateDistributionButtons(float halfWidth, EditorGUI.ChangeCheckScope changeScope)
        {
            bool needSaving = false;
            GUILayout.Label("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginVertical(RowStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap));
            bool isValidSurface, preValid, currValid, postValid;
            int highestValid;
            ValidateSettings(true, out isValidSurface, out preValid, out currValid, out postValid, out highestValid);
            EditorGUILayout.EndVertical();

            if (DataHolder.IsWindow)
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
            }

            if (ModeData.RealtimePreview && isValidSurface && highestValid >= 0 && !EditorData.Grids[UIData.SelectedLevelIndex].ReadOnly && (changeScope.changed || (!EditorData.LastFrameValid && highestValid > EditorData.HighestDistributedLevel)))
            {
                if (_modeHasChanged)
                {
                    DistributePoisson(0, highestValid, true);
                }
                else
                {
                    DistributePoisson(Mathf.Max(Mathf.Min(EditorData.HighestDistributedLevel, UIData.SelectedLevelIndex), 0), highestValid, true);
                }

            }
            EditorData.LastFrameValid = isValidSurface && preValid && currValid && postValid;

            using (new EditorGUI.DisabledScope(!isValidSurface))
            {
                using (new EditorGUI.DisabledScope(!(preValid && currValid) || EditorData.Grids[UIData.SelectedLevelIndex].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: [0 - " + UIData.SelectedLevelIndex + "]"), ButtonStyle))
                    {
                        DistributePoisson(0, UIData.SelectedLevelIndex, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(currValid && EditorData.HighestDistributedLevel >= UIData.SelectedLevelIndex - 1) || EditorData.Grids[UIData.SelectedLevelIndex].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: " + UIData.SelectedLevelIndex), ButtonStyle))
                    {
                        DistributePoisson(UIData.SelectedLevelIndex, UIData.SelectedLevelIndex, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(currValid && postValid && EditorData.HighestDistributedLevel >= UIData.SelectedLevelIndex - 1) || EditorData.Grids.Last().ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: [" + UIData.SelectedLevelIndex + " - " + (Data.Count - 1) + "]"), ButtonStyle))
                    {
                        DistributePoisson(UIData.SelectedLevelIndex, Data.Count - 1, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(preValid && currValid && postValid) || EditorData.Grids.Last().ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute All"), ButtonStyle))
                    {
                        DistributePoisson(0, Data.Count - 1, false);
                    }
                }
            }

            if (DataHolder.IsWindow)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(EditorData.Grids[UIData.SelectedLevelIndex].ReadOnly || EditorData.HighestDistributedLevel < UIData.SelectedLevelIndex))
                {
                    if (GUILayout.Button(new GUIContent("Apply [0 - " + UIData.SelectedLevelIndex + "]"), ButtonStyle))
                    {
                        SetReadOnly(UIData.SelectedLevelIndex);
                        UIData.InsertLevelAt = Mathf.Max(Data.Count, UIData.SelectedLevelIndex + 1);
                        needSaving = true;
                    }
                }
                using (new EditorGUI.DisabledScope(EditorData.HighestDistributedLevel == -1 || EditorData.Grids[EditorData.HighestDistributedLevel].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Apply [0 - " + EditorData.HighestDistributedLevel + "]"), ButtonStyle))
                    {
                        SetReadOnly(EditorData.HighestDistributedLevel);
                        UIData.InsertLevelAt = Mathf.Max(Data.Count, EditorData.HighestDistributedLevel + 1);
                        needSaving = true;
                    }
                }
            }
            using (new EditorGUI.DisabledScope(EditorData.HighestDistributedLevel == -1 || EditorData.Grids[EditorData.HighestDistributedLevel].ReadOnly))
            {
                if (GUILayout.Button(new GUIContent("Clear Unapplied Levels"), ButtonStyle))
                {
                    CleanupPlacedObjects(EditorData, 0);

                    EditorData.HighestDistributedLevel = -1;
                    for (int i = 0; i < EditorData.Grids.Count; ++i)
                    {
                        if (EditorData.Grids[i].ReadOnly)
                        {
                            EditorData.HighestDistributedLevel = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (EditorData.HighestDistributedLevel == -1)
                    {
                        EditorData.UpdateAllowVisualTransformChanges(DataHolder.IsWindow);
                    }
                }
            }
            using (new EditorGUI.DisabledScope(EditorData.HighestDistributedLevel == -1))
            {
                if (GUILayout.Button(new GUIContent("Reset Settings"), ButtonStyle))
                {
                    Reset(false);
                    needSaving = true;
                }
            }

            if (DataHolder.IsWindow)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            return needSaving;
        }

        public void Reset(bool loadAndStore = true)
        {
            if (loadAndStore)
            {
                LoadDataHolder();
            }
            EditorData.HighestDistributedLevel = -1;

            EditorData.Grids.Clear();
            Data.Clear();

            CleanupPlacedObjects(EditorData, 0);
            EditorData.PlacedObjects.Clear();

            UIData.SelectedLevelIndex = 0;
            UIData.InsertLevelAt = 0;
            EditorData.Grids.Add(new StoredGrid());
            EditorData.PlacedObjects.Add(new GameObjectList());
            Data.Add(new PoissonData());

            SelectedData = Data[UIData.SelectedLevelIndex];

            ModeData.RealtimePreview = false;

            EditorData.UpdateAllowVisualTransformChanges(DataHolder.IsWindow);
            EditorData.UpdateVisualTexture(ModeData, SelectedData);

            if (loadAndStore)
            {
                StoreDataHolder();
            }
        }
    }
}
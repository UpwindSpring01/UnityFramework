using UnityEditor;
using UnityEngine;
using System.Linq;
using Helpers_KevinLoddewykx.General;
using Helpers_KevinLoddewykx.General.WeightedArray;
using System.Collections.Generic;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonData;
using UnityEditorInternal;
using System;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonInternalEditorData;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonHelper
    {
        private PoissonData _selectedData;

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

        private GUIStyle LeftNumberFieldStyle;
        private GUIStyle RightNumberFieldStyle;

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
            LeftNumberFieldStyle = new GUIStyle(EditorStyles.numberField);
            RightNumberFieldStyle = new GUIStyle(EditorStyles.numberField);
            BoxStyle = new GUIStyle(GUI.skin.box);
            SubBoxStyle = new GUIStyle(GUI.skin.box);

            if (_window != null)
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
                SubLeftColumnStyle.margin.right = 0;
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
            LeftNumberFieldStyle.margin.left = 0;
            LeftNumberFieldStyle.margin.right = 0;

            RightNumberFieldStyle.margin.left = 0;
            RightNumberFieldStyle.margin.right = 0;

            FoldoutStyle = new GUIStyle(EditorStyles.foldout);
            FoldoutStyle.margin.left = (_window == null) ? LeftMarginInspector : 0;
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

            _modeHasChanged = false;



            if (_window != null)
            {
                _masterScroll = EditorGUILayout.BeginScrollView(_masterScroll);
                EditorGUILayout.Space();
            }

            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                
                float halfWidth = (Mathf.Max(MinimumWidth, EditorGUIUtility.currentViewWidth) - ColumnGap) * 0.5f;
                if (_window == null)
                {
                    halfWidth -= LeftMarginInspector * 0.5f;
                }
                EditorGUI.BeginChangeCheck();

                {
                    Undo.RecordObject(_object, "PDS Param");

                    CreateModeUI(halfWidth);
                    _modeHasChanged = EditorGUI.EndChangeCheck();

                    CreateLevelUI(halfWidth);
                    using (new EditorGUI.DisabledScope(_editorData.Grids[_editorData.SelectedLevelIndex].ReadOnly))
                    {
                        CreateGeneralUI(halfWidth);
                        CreatePoissonUI(halfWidth);
                        CreateClumpUI(halfWidth);
                    }
                }
                if (!_isPrefab)
                {
                    CreateDistributionButtons(halfWidth, changeScope);
                }
            }
            if (_window != null)
            {
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();

            if (!_isPrefab)
            {
                _editorData.HelperVisual.transform.hasChanged = false;
            }
        }

        private void CreateModeUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap + ((_window == null) ? LeftMarginInspector : 0)));
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;

            EditorGUILayout.BeginHorizontal(RowStyle);
            EditorGUILayout.BeginVertical(LeftColumnFoldoutStyle, GUILayout.MaxWidth(halfWidth + ((_window == null) ? LeftMarginInspector : 0)));
            EditorGUILayout.BeginHorizontal(RowStyle);

            _editorData.ModeCategory = EditorGUILayout.Foldout(_editorData.ModeCategory, "Mode", true, FoldoutStyle);

            bool isDisabled = _editorData.Grids[0].ReadOnly;
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                EditorGUI.BeginChangeCheck();
                if (_window)
                {
                    _modeData.Mode = (DistributionMode)EditorGUILayout.EnumPopup(_modeData.Mode, PopupStyle);
                }
                else
                {
                    List<string> modes = Enum.GetNames(typeof(DistributionMode)).ToList();
                    modes.Remove(DistributionMode.Surface.ToString());

                    _modeData.Mode = (DistributionMode)Enum.Parse(typeof(DistributionMode),
                        modes[EditorGUILayout.Popup(modes.IndexOf(_modeData.Mode.ToString()), modes.ToArray(), PopupStyle)]);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _editorData.UpdateVisualMode(_modeData);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
            _modeData.RealtimePreview = EditorGUILayout.Toggle("Realtime preview:", _modeData.RealtimePreview, ToggleStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (_editorData.ModeCategory)
                {
                    CreateModeSpecificUI(halfWidth);
                }
            }

            EditorGUILayout.EndVertical();

            GUILayout.Label("", GUI.skin.horizontalSlider);
        }

        private void CreateLevelUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap + ((_window == null) ? LeftMarginInspector : 0)));
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;

            EditorGUILayout.BeginHorizontal(RowStyle);
            EditorGUILayout.BeginVertical(LeftColumnFoldoutStyle, GUILayout.MaxWidth(halfWidth + ((_window == null) ? LeftMarginInspector : 0)));
            EditorGUILayout.BeginHorizontal(RowStyle);
            _editorData.LevelCategory = EditorGUILayout.Foldout(_editorData.LevelCategory, "Level", true, FoldoutStyle);

            string[] levels = new string[_data.Count];
            int[] levelIndices = new int[_data.Count];
            List<string> levelsInsert = new List<string>();
            List<int> levelIndicesInsert = new List<int>();
            for (int i = 0; i < _data.Count; ++i)
            {
                levels[i] = "" + i;
                if (_editorData.Grids[i].ReadOnly)
                {
                    levels[i] += " (Applied)";
                }
                else
                {
                    levelsInsert.Add("" + i);
                    levelIndicesInsert.Add(i);
                    if (i <= _editorData.HighestDistributedLevel)
                    {
                        levels[i] += " (Distributed: " + _editorData.PlacedObjects[i].Count + ")";
                    }
                }
                levelIndices[i] = i;
            }
            levelsInsert.Add("" + _data.Count);
            levelIndicesInsert.Add(_data.Count);
            int newSelectedLevelIndex = EditorGUILayout.IntPopup(_editorData.SelectedLevelIndex, levels, levelIndices, PopupStyle);
            if (newSelectedLevelIndex != _editorData.SelectedLevelIndex)
            {
                _editorData.SelectedLevelIndex = newSelectedLevelIndex;
                _selectedData = _data[newSelectedLevelIndex];

                _editorData.UpdateVisualTexture(_modeData, _selectedData);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (_editorData.LevelCategory)
            {
                EditorGUILayout.BeginHorizontal(RowStyle);

                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                _editorData.DuplicateLevel = EditorGUILayout.Toggle("Duplicate selected:", _editorData.DuplicateLevel, ToggleStyle);
                using (new EditorGUI.DisabledGroupScope(_data.Count == 1))
                {
                    if (GUILayout.Button("Delete Selected Level", ButtonStyle))
                    {
                        CleanupPlacedObjects(_editorData.SelectedLevelIndex);
                        _editorData.HighestDistributedLevel = Math.Min(_editorData.SelectedLevelIndex - 1, _editorData.HighestDistributedLevel);

                        _data.RemoveAt(_editorData.SelectedLevelIndex);
                        _editorData.Grids.RemoveAt(_editorData.SelectedLevelIndex);
                        _editorData.PlacedObjects.RemoveAt(_editorData.SelectedLevelIndex);

                        _editorData.SelectedLevelIndex = Mathf.Min(_data.Count - 1, _editorData.SelectedLevelIndex);
                        _selectedData = _data[_editorData.SelectedLevelIndex];
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                _editorData.InsertLevelAt = EditorGUILayout.IntPopup("Insert at:", _editorData.InsertLevelAt, levelsInsert.ToArray(), levelIndicesInsert.ToArray(), PopupStyle);
                string addMode = _editorData.DuplicateLevel ? "Duplicate Selected" : "Insert";
                if (GUILayout.Button(addMode + " Level At " + _editorData.InsertLevelAt, ButtonStyle))
                {
                    CleanupPlacedObjects(_editorData.InsertLevelAt);
                    _editorData.HighestDistributedLevel = Mathf.Min(_editorData.HighestDistributedLevel, _editorData.InsertLevelAt - 1);

                    if (_editorData.DuplicateLevel)
                    {
                        _data.Insert(_editorData.InsertLevelAt, _selectedData.DeepCopy());
                    }
                    else
                    {
                        _data.Insert(_editorData.InsertLevelAt, new PoissonData());
                    }
                    _editorData.Grids.Insert(_editorData.InsertLevelAt, new StoredGrid());
                    _editorData.PlacedObjects.Insert(_editorData.InsertLevelAt, new GameObjectList());

                    _editorData.SelectedLevelIndex = _editorData.InsertLevelAt;
                    _selectedData = _data[_editorData.SelectedLevelIndex];
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateGeneralUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;

            if (_editorData.GeneralCategory = EditorGUILayout.Foldout(_editorData.GeneralCategory, "General", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.UseSeed = EditorGUILayout.Toggle(new GUIContent("Use seed:"), _selectedData.UseSeed, ToggleStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(!_selectedData.UseSeed))
                {
                    _selectedData.Seed = EditorGUILayout.IntField(new GUIContent("Seed:"), _selectedData.Seed, NumberFieldStyle);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = BACKGROUND_COLOR_SUB_MENU;
                EditorGUILayout.BeginHorizontal(SubBoxStyle);
                halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;
                GUI.backgroundColor = oldColor;

                EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("Overlap Test Settings", EditorStyles.boldLabel);
                _selectedData.SphereCollisionCheck = EditorGUILayout.Toggle(new GUIContent("Sphere:"), _selectedData.SphereCollisionCheck, ToggleStyle);
                _selectedData.BoxCollisionCheck = EditorGUILayout.Toggle(new GUIContent("Box:"), _selectedData.BoxCollisionCheck, ToggleStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.BoundsMode = (EBoundsMode)EditorGUILayout.EnumPopup("Mode:", _selectedData.BoundsMode, PopupStyle);
                _selectedData.OverlapLayerMask = EditorGUILayout.MaskField("Layer mask:", _selectedData.OverlapLayerMask, InternalEditorUtility.layers, PopupStyle);
                _selectedData.OverlapRaycastTriggerInteraction = (QueryTriggerInteraction)EditorGUILayout.EnumPopup("Trigger query mode:", _selectedData.OverlapRaycastTriggerInteraction, PopupStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreatePoissonUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;

            if (_editorData.PoissonCategory = EditorGUILayout.Foldout(_editorData.PoissonCategory, "Poisson", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.MaxSamples = EditorGUILayout.IntField(new GUIContent("Max Samples:", "Max samples: <= 0 for no limit"), _selectedData.MaxSamples, NumberFieldStyle);
                EditorGUI.BeginChangeCheck();
                _selectedData.Map = (Texture2D)EditorGUILayout.ObjectField("Map:", _selectedData.Map, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck())
                {
                    _editorData.UpdateVisualTexture(_modeData, _selectedData);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.MaxSamplesPreview = EditorGUILayout.IntField(new GUIContent("Max Preview:", "Max samples for realtime preview: <= 0 for no limit"), _selectedData.MaxSamplesPreview, NumberFieldStyle);
                _selectedData.Samples = EditorGUILayout.IntField(new GUIContent("Samples/Object:"), _selectedData.Samples, NumberFieldStyle);
                _selectedData.MinDist = EditorGUILayout.FloatField(new GUIContent("Min distance:"), _selectedData.MinDist, NumberFieldStyle);
                if (_selectedData.MinDist <= 0.0f)
                {
                    _selectedData.MinDist = 1.0f;
                }
                using (new EditorGUI.DisabledScope(_selectedData.Map == null))
                {
                    if (_selectedData.MaxDist <= 0.0f)
                    {
                        _selectedData.MaxDist = 10.0f;
                    }
                    _selectedData.MaxDist = EditorGUILayout.FloatField(new GUIContent("Max distance:"), _selectedData.MaxDist, NumberFieldStyle);
                }

                _selectedData.DistToKeepNextLevel = EditorGUILayout.FloatField(new GUIContent("Distance Next Level:", "The distance the next level needs to keep from the distributed points."), _selectedData.DistToKeepNextLevel, NumberFieldStyle);
                float maxDist = (_selectedData.Map == null) ? _selectedData.MinDist : _selectedData.MaxDist;
                _selectedData.DistToKeepNextLevel = Mathf.Min(_selectedData.DistToKeepNextLevel, maxDist);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUI.BeginChangeCheck();
                _selectedData.PoissonObjects = (BaseWeightedCollection)EditorGUILayout.ObjectField("Poisson Data:", _selectedData.PoissonObjects, typeof(BaseWeightedCollection), false);
                int poissonObjectsCount = 0;
                if (_selectedData.PoissonObjects?.Element.WeightedArray.HasWeightedElementsNonNull() ?? false)
                {
                    poissonObjectsCount = _selectedData.PoissonObjects.Element.WeightedArray.GetWeightedElementsCountNonNull();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _selectedData.PoissonObjectOptions = new ObjectOptions[poissonObjectsCount];
                    _selectedData.PoissonObjectOptions.InitNew();
                }
                else if ((_selectedData.PoissonObjectOptions?.Length ?? 0) != poissonObjectsCount)
                {
                    List<ObjectOptions> options = _selectedData.PoissonObjectOptions.ToList();
                    options.Resize(poissonObjectsCount);
                    _selectedData.PoissonObjectOptions = options.ToArray();

                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                CreateOptions(_selectedData.PoissonObjects?.Element.WeightedArray, _selectedData.PoissonObjectOptions, ref _editorData.PoissonSelected, halfWidth);
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateClumpUI(float halfWidth)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;

            if (_editorData.ClumpCategory = EditorGUILayout.Foldout(_editorData.ClumpCategory, "Clumping", true, FoldoutStyle))
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.MinClump = EditorGUILayout.IntField(new GUIContent("Min clumping:"), _selectedData.MinClump, NumberFieldStyle);
                _selectedData.MinClumpRange = EditorGUILayout.FloatField(new GUIContent("Min clump range:"), _selectedData.MinClumpRange, NumberFieldStyle);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                _selectedData.MaxClump = EditorGUILayout.IntField(new GUIContent("Max clumping:"), _selectedData.MaxClump, NumberFieldStyle);
                _selectedData.MaxClumpRange = EditorGUILayout.FloatField(new GUIContent("Max clump range:"), _selectedData.MaxClumpRange, NumberFieldStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUI.BeginChangeCheck();
                _selectedData.ClumpObjects = (BaseWeightedCollection)EditorGUILayout.ObjectField("Clumping Data:", _selectedData.ClumpObjects, typeof(BaseWeightedCollection), false);
                int clumpObjectsCount = 0;
                if (_selectedData.ClumpObjects?.Element.WeightedArray.HasWeightedElementsNonNull() ?? false)
                {
                    clumpObjectsCount = _selectedData.ClumpObjects.Element.WeightedArray.GetWeightedElementsCountNonNull();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _selectedData.ClumpObjectOptions = new ObjectOptions[clumpObjectsCount];
                    _selectedData.ClumpObjectOptions.InitNew();
                }
                else if ((_selectedData.ClumpObjectOptions?.Length ?? 0) != clumpObjectsCount)
                {
                    List<ObjectOptions> options = _selectedData.ClumpObjectOptions.ToList();
                    options.Resize(clumpObjectsCount);
                    _selectedData.ClumpObjectOptions = options.ToArray();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                CreateOptions(_selectedData.ClumpObjects?.Element.WeightedArray, _selectedData.ClumpObjectOptions, ref _editorData.ClumpSelected, halfWidth);
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateOptions(WeightedArray objects, ObjectOptions[] options, ref int selectedIndex, float halfWidth)
        {
            bool disabled = (!objects?.HasWeightedElementsNonNull()) ?? true;

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = BACKGROUND_COLOR_SUB_MENU;
            EditorGUILayout.BeginVertical(SubBoxStyle);
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;
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
                        .Where((o) => o.Object != null && o.Weight > 0)
                        .Select((o, i) => new System.Tuple<WeightedObject, int>(o, i));

                    objectStrings = objectsNonNull
                        .Select((a) => objects.GetChance(a.Item1).ToString("F2") + "%) " + a.Item1.Object.name)
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
                using (new EditorGUILayout.VerticalScope(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth)))
                {

                    selectedIndex = EditorGUILayout.Popup(selectedIndex, objectStrings, PopupStyle);
                    if (disabled)
                    {
                        selectedOptions = new ObjectOptions();
                    }
                    else
                    {
                        selectedOptions = options[objectsNonNull.ElementAt(selectedIndex).Item2];
                    }

                    if (_window != null)
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
                    EditorGUIUtility.fieldWidth = (halfWidth - LABEL_WIDTH) * 0.5f - 9.0f;
                    EditorGUILayout.PrefixLabel("");

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUIUtility.labelWidth = LABEL_WIDTH;
                    EditorGUILayout.PrefixLabel(" ");
                    // Because unity layouting is pain
                    EditorGUIUtility.labelWidth = 0.0000001f;
                    EditorGUILayout.LabelField("Min", centerStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("Max", centerStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = LABEL_WIDTH;
                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Scale:");
                    selectedOptions.MinScale = EditorGUILayout.FloatField(selectedOptions.MinScale, LeftNumberFieldStyle);
                    selectedOptions.MaxScale = EditorGUILayout.FloatField(selectedOptions.MaxScale, RightNumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Height offset:");
                    selectedOptions.MinHeightOffset = EditorGUILayout.FloatField(selectedOptions.MinHeightOffset, LeftNumberFieldStyle);
                    selectedOptions.MaxHeightOffset = EditorGUILayout.FloatField(selectedOptions.MaxHeightOffset, RightNumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.PrefixLabel("Face . Up:");
                    selectedOptions.MinDot = EditorGUILayout.FloatField(selectedOptions.MinDot, LeftNumberFieldStyle);
                    selectedOptions.MaxDot = EditorGUILayout.FloatField(selectedOptions.MaxDot, RightNumberFieldStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.fieldWidth = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateDistributionButtons(float halfWidth, EditorGUI.ChangeCheckScope changeScope)
        {
            GUILayout.Label("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginVertical(RowStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap));
            bool isValidSurface, preValid, currValid, postValid;
            int highestValid;
            ValidateSettings(true, out isValidSurface, out preValid, out currValid, out postValid, out highestValid);
            EditorGUILayout.EndVertical();

            if (_window != null)
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                EditorGUILayout.BeginVertical(LeftColumnStyle, GUILayout.MaxWidth(halfWidth));
            }

            if (_modeData.RealtimePreview && isValidSurface && highestValid >= 0 && !_editorData.Grids[_editorData.SelectedLevelIndex].ReadOnly && (changeScope.changed || (!_editorData.LastFrameValid && highestValid > _editorData.HighestDistributedLevel)))
            {
                if (_modeHasChanged)
                {
                    DistributePoisson(0, highestValid, true);
                }
                else
                {
                    DistributePoisson(Mathf.Max(Mathf.Min(_editorData.HighestDistributedLevel, _editorData.SelectedLevelIndex), 0), highestValid, true);
                }
                
            }
            _editorData.LastFrameValid = isValidSurface && preValid && currValid && postValid;

            using (new EditorGUI.DisabledScope(!isValidSurface))
            {
                using (new EditorGUI.DisabledScope(!(preValid && currValid) || _editorData.Grids[_editorData.SelectedLevelIndex].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: [0 - " + _editorData.SelectedLevelIndex + "]"), ButtonStyle))
                    {
                        DistributePoisson(0, _editorData.SelectedLevelIndex, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(currValid && _editorData.HighestDistributedLevel >= _editorData.SelectedLevelIndex - 1) || _editorData.Grids[_editorData.SelectedLevelIndex].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: " + _editorData.SelectedLevelIndex), ButtonStyle))
                    {
                        DistributePoisson(_editorData.SelectedLevelIndex, _editorData.SelectedLevelIndex, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(currValid && postValid && _editorData.HighestDistributedLevel >= _editorData.SelectedLevelIndex - 1) || _editorData.Grids.Last().ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute Level: [" + _editorData.SelectedLevelIndex + " - " + (_data.Count - 1) + "]"), ButtonStyle))
                    {
                        DistributePoisson(_editorData.SelectedLevelIndex, _data.Count - 1, false);
                    }
                }
                using (new EditorGUI.DisabledScope(!(preValid && currValid && postValid) || _editorData.Grids.Last().ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Distribute All"), ButtonStyle))
                    {
                        DistributePoisson(0, _data.Count - 1, false);
                    }
                }
            }

            if (_window != null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(_editorData.Grids[_editorData.SelectedLevelIndex].ReadOnly || _editorData.HighestDistributedLevel < _editorData.SelectedLevelIndex))
                {
                    if (GUILayout.Button(new GUIContent("Apply [0 - " + _editorData.SelectedLevelIndex + "]"), ButtonStyle))
                    {
                        SetReadOnly(_editorData.SelectedLevelIndex);
                    }
                }
                using (new EditorGUI.DisabledScope(_editorData.HighestDistributedLevel == -1 || _editorData.Grids[_editorData.HighestDistributedLevel].ReadOnly))
                {
                    if (GUILayout.Button(new GUIContent("Apply [0 - " + _editorData.HighestDistributedLevel + "]"), ButtonStyle))
                    {
                        SetReadOnly(_editorData.HighestDistributedLevel);
                    }
                }
            }
            using (new EditorGUI.DisabledScope(_editorData.HighestDistributedLevel == -1 || _editorData.Grids[_editorData.HighestDistributedLevel].ReadOnly))
            {
                if (GUILayout.Button(new GUIContent("Clear unapplied distributed"), ButtonStyle))
                {
                    CleanupPlacedObjects(0);

                    _editorData.HighestDistributedLevel = -1;
                    for (int i = 0; i < _editorData.Grids.Count; ++i)
                    {
                        if (_editorData.Grids[i].ReadOnly)
                        {
                            _editorData.HighestDistributedLevel = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (_editorData.HighestDistributedLevel == -1)
                    {
                        _editorData.UpdateAllowVisualTransformChanges();

                    }
                }
            }
            using (new EditorGUI.DisabledScope(_editorData.HighestDistributedLevel == -1))
            {
                if (GUILayout.Button(new GUIContent("Reset settings"), ButtonStyle))
                {
                    _editorData.HighestDistributedLevel = -1;

                    _editorData.Grids.Clear();
                    _data.Clear();

                    CleanupPlacedObjects(0);
                    _editorData.PlacedObjects.Clear();
                    
                    _editorData.SelectedLevelIndex = 0;
                    _editorData.Grids.Add(new StoredGrid());
                    _editorData.PlacedObjects.Add(new GameObjectList());
                    _data.Add(new PoissonData());

                    _selectedData = _data[_editorData.SelectedLevelIndex];

                    _editorData.UpdateAllowVisualTransformChanges();
                }
            }

            if (_window != null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
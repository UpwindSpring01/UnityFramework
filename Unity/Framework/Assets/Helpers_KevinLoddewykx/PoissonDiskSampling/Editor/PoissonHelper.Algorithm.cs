// Reference: http://devmag.org.za/2009/05/03/poisson-disk-sampling/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Helpers_KevinLoddewykx.General.WeightedArrayCore;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonData;
using static Helpers_KevinLoddewykx.PoissonDiskSampling.PoissonInternalEditorData;
using UnityEditorInternal;
using Helpers_KevinLoddewykx.General;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonHelper
    {
        // Internal vars
        private PoissonData _activeData;

        private StoredGrid _activeGrid = null;
        private int _activeLevel = 0;

        private Bounds _surfaceBounds;
        private IEnumerable<Collider> _surfaceColliders;

        private Collider[] _collidersOverlapHelper = null;

        private int _samples = 0;
        private int _currNestingLevel = 0;
        private int _maxNestingLevel = 0;

        public static void CleanupPlacedObjects(PoissonInternalEditorData editorData, int start)
        {
            for (int i = start; i <= editorData.PlacedObjects.Count - 1; ++i)
            {
                foreach (GameObject gameObject in editorData.PlacedObjects[i])
                {
                    if (gameObject)
                    {
                        PoissonPlacer placer = gameObject.GetComponent<PoissonPlacer>();
                        if (placer != null)
                        {
                            PoissonHelperInternalStorage.Instance.Remove(placer);
                            CleanupPlacedObjects(placer.EditorData, 0);
                            placer.EditorData.DestroyVisual(placer.ModeData);
                        }
                        Object.DestroyImmediate(gameObject);
                    }
                }
                editorData.PlacedObjects[i].Clear();
            }
        }

        private void SetReadOnly(int end)
        {
            for (int i = 0; i <= end; ++i)
            {
                EditorData.PlacedObjects[i].Clear();
                EditorData.Grids[i].ReadOnly = true;
            }
            EditorData.UpdateAllowVisualTransformChanges(DataHolder.IsWindow);
            SceneView.RepaintAll();
        }

        private List<GridPoint>[] CellsAroundGridPoint(StoredGrid grid, int x, int y)
        {
            List<GridPoint>[] cells = new List<GridPoint>[25];

            int endRow = Mathf.Min(grid.GridDepth - 1, y + 2);
            int endCol = Mathf.Min(grid.GridWidth - 1, x + 2);

            int counter = 0;
            for (int row = Mathf.Max(0, y - 2); row <= endRow; ++row)
            {
                for (int col = Mathf.Max(0, x - 2); col <= endCol; ++col)
                {
                    cells[counter] = grid.Grid2D[row * grid.GridWidth + col];
                    ++counter;
                }
            }

            return cells;
        }

        private float Remap(float value, float origFrom, float origTo, float newFrom, float newTo)
        {
            return (((value - origFrom) / (origTo - origFrom)) * (newTo - newFrom)) + newFrom;
        }

        // Returns true when a placed object is to close.
        private bool InNeighbourhood(StoredGrid grid, Vector2 loc, float finalMinDistSqrd, out bool toCloseToProcess)
        {
            int remappedX = Mathf.CeilToInt(Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, 0, grid.GridWidth - 1));
            int remappedY = Mathf.CeilToInt(Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, 0, grid.GridDepth - 1));

            List<GridPoint>[] cells = CellsAroundGridPoint(grid, remappedX, remappedY);

            toCloseToProcess = false;
            foreach (List<GridPoint> cellContents in cells)
            {
                if (cellContents != null)
                {
                    foreach (GridPoint gridPoint in cellContents)
                    {
                        Vector2 vec = gridPoint.Point - loc;
                        if (vec.sqrMagnitude <= finalMinDistSqrd)
                        {
                            toCloseToProcess = true;
                            if (gridPoint.HasObject)
                            {
                                return true;
                            }
                            
                        }
                    }
                }
            }
            return false;
        }

        // Returns true when a placed object is to close.
        private bool InNeighbourhood(Vector2 loc, float finalMinDistSqrd, out bool toCloseToProcess)
        {
            if (!InNeighbourhood(_activeGrid, loc, finalMinDistSqrd, out toCloseToProcess))
            {
                for (int i = 0; i < _activeLevel; ++i)
                {
                    StoredGrid grid = EditorData.Grids[i];
                    bool unused;
                    if (InNeighbourhood(grid, loc, Mathf.Max(Data[i].DistToKeepNextLevel * Data[i].DistToKeepNextLevel, finalMinDistSqrd), out unused))
                    {
                        // Object found
                        return true;
                    }
                }
                return false;
            }

            // Object found
            toCloseToProcess = true;
            return true;
        }

        private bool CheckSphereOverlap(Bounds scaledBounds, LayerMask correctedMask)
        {
            float radius = Mathf.Max(scaledBounds.extents.x, scaledBounds.extents.y, scaledBounds.extents.z);

            int amount = Physics.OverlapSphereNonAlloc(scaledBounds.center, radius, _collidersOverlapHelper,
                correctedMask, _activeData.OverlapRaycastTriggerInteraction);
            return amount > 0;
        }

        private bool CheckBoxOverlap(Bounds scaledBounds, Quaternion orientation, LayerMask correctedMask)
        {
            int amount = Physics.OverlapBoxNonAlloc(scaledBounds.center, scaledBounds.extents, _collidersOverlapHelper, orientation,
                correctedMask, _activeData.OverlapRaycastTriggerInteraction);
            return amount > 0;
        }

        private float GetFinalMinDistance(Vector2 loc)
        {
            if (_activeData.Map == null)
            {
                return _activeData.MinDist;
            }
            else
            {
                int xPos = (int)Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, 0, _activeData.Map.width - 1);
                int yPos = (int)Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, 0, _activeData.Map.height - 1);
                Color color = _activeData.Map.GetPixel((_activeData.Map.width - 1) - xPos, (_activeData.Map.height - 1) - yPos);
                return _activeData.MinDist + (color.grayscale * (_activeData.MaxDist - _activeData.MinDist));
            }
        }

        private Vector2 GenerateRandomPointAround(Vector2 loc, out float finalMinDist)
        {
            ++_samples;

            finalMinDist = GetFinalMinDistance(loc);

            float radius = finalMinDist * (Random.value + 1);
            Vector2 randomVector = Random.insideUnitCircle;
            randomVector.Normalize();

            return new Vector2(loc.x + radius * randomVector.x, loc.y + radius * randomVector.y);
        }

        private Vector2 GenerateRandomPointClumping(Vector2 loc)
        {
            float radius = Random.Range(_activeData.MinClumpRange, _activeData.MaxClumpRange);

            Vector2 randomVector = Random.insideUnitCircle;
            randomVector.Normalize();

            return new Vector2(loc.x + radius * randomVector.x, loc.y + radius * randomVector.y);
        }

        private void ApplySettingsToObject(ObjectOptions options, Vector3 normalDir, ref Vector3 newPosition, out Quaternion newRotation, out Vector3 newScale)
        {
            newScale = new Vector3(1, 1, 1);
            if (options.UniformScaling == true)
            {
                float rand = Random.Range(options.MinScale, options.MaxScale);

                if (options.ScaleX == true) { newScale.x = rand; }
                if (options.ScaleY == true) { newScale.y = rand; }
                if (options.ScaleZ == true) { newScale.z = rand; }
            }
            else
            {
                if (options.ScaleX == true) { newScale.x = Random.Range(options.MinScale, options.MaxScale); }
                if (options.ScaleY == true) { newScale.y = Random.Range(options.MinScale, options.MaxScale); }
                if (options.ScaleZ == true) { newScale.z = Random.Range(options.MinScale, options.MaxScale); }
            }

            newRotation = new Quaternion();

            if (options.AlignToSurface == true)
            {
                Quaternion q1 = (options.RotateY == true) ? Quaternion.AngleAxis(Random.Range(0, 360), normalDir) : Quaternion.identity;
                Quaternion q2 = Quaternion.FromToRotation(Vector3.up, normalDir);
                newRotation = q1 * q2;
            }
            else
            {
                Vector3 eulerAngles = new Vector3();

                if (options.RotateX == true) { eulerAngles.x = Random.Range(0.0f, 360.0f); }
                if (options.RotateY == true) { eulerAngles.y = Random.Range(0.0f, 360.0f); }
                if (options.RotateZ == true) { eulerAngles.z = Random.Range(0.0f, 360.0f); }

                newRotation = Quaternion.Euler(eulerAngles);
            }

            float heightOffset = Random.Range(options.MinHeightOffset, options.MaxHeightOffset);
            if(options.ScaleY && options.ScaleHeightOffset)
            {
                heightOffset *= newScale.y;
            }
            newPosition += normalDir * heightOffset;
        }

        private bool CreateRandomObjectAtLoc(Vector2 loc, float finalMinDistSqrd, out bool objectPlaced, WeightedArray weightedObjects, ObjectOptions[] objectOptions, bool preview)
        {
            objectPlaced = false;

            if (InBounds(loc))
            {
                bool toCloseToProcess = true;
                // Don't do InNeighbourhood for clumping algorithm
                if (finalMinDistSqrd <= 0 || (!InNeighbourhood(loc, finalMinDistSqrd, out toCloseToProcess) && !InInnerBounds(loc)))
                {
                    RaycastHit hit;
                    if (CastRay(loc, out hit))
                    {
                        int index = weightedObjects.GetRandomObjectIndex();
                        ObjectOptions options = objectOptions[index];

                        float dotResult = Vector3.Dot(hit.normal, Vector3.up);
                        if (dotResult >= options.MinDot && dotResult <= options.MaxDot)
                        {
                            Vector3 pos = hit.point;
                            Quaternion rot;
                            Vector3 scale;
                            ApplySettingsToObject(options, hit.normal, ref pos, out rot, out scale);

                            GameObject newObject = weightedObjects.GetGameObject(index);
                            if (newObject != null)
                            {
                                if (_activeData.SphereCollisionCheck || _activeData.BoxCollisionCheck)
                                {
                                    Bounds bounds;
                                    if (GetBounds(newObject, _activeData.BoundsMode, pos, rot, scale, out bounds))
                                    {
                                        if (ModeData.Mode == DistributionMode.Surface)
                                        {
                                            ModeData.Surface.gameObject.SetActive(false);
                                        }

                                        bool hasOverlap = true;
                                        LayerMask correctedMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(_activeData.OverlapLayerMask);
                                        if (_activeData.SphereCollisionCheck)
                                        {
                                            hasOverlap = CheckSphereOverlap(bounds, correctedMask);
                                        }
                                        if (_activeData.BoxCollisionCheck && hasOverlap)
                                        {
                                            hasOverlap = CheckBoxOverlap(bounds, rot, correctedMask);
                                        }
                                        if (ModeData.Mode == DistributionMode.Surface)
                                        {
                                            ModeData.Surface.gameObject.SetActive(true);
                                        }
                                        if (hasOverlap)
                                        {
                                            return !toCloseToProcess;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return !toCloseToProcess;
                            }
                            objectPlaced = true;
                            GameObject obj = Object.Instantiate(newObject, null);
                            obj.transform.position = pos;
                            obj.transform.rotation = rot;
                            obj.transform.localScale = scale;

                            // When not doing this the colliders aren't properly initialized and OverlapXXX won't work
                            obj.SetActive(false);
                            obj.SetActive(true);

                            obj.transform.parent = options.Parent;

                            EditorData.PlacedObjects[_activeLevel].Add(obj);

                            if (_currNestingLevel < _maxNestingLevel)
                            {
                                PoissonPlacer placer = obj.GetComponent<PoissonPlacer>();
                                if (placer != null && placer.enabled)
                                {
                                    PoissonHelper helper = new PoissonHelper(placer);
                                    helper.Init();
                                    bool isValidSurface, preValid, currValid, postValid;
                                    int highestValid;
                                    helper.LoadDataHolder();
                                    helper.ValidateSettings(false, out isValidSurface, out preValid, out currValid, out postValid, out highestValid);

                                    placer.EditorData.LastFrameValid = isValidSurface && preValid && currValid && postValid;
                                    if (highestValid >= 0)
                                    {
                                        Random.State randState = Random.state;
                                        helper.DistributePoisson(0, highestValid, preview, _currNestingLevel + 1, _maxNestingLevel);
                                        Random.state = randState;
                                    }
                                    helper.StoreDataHolder();
                                    
                                }
                            }
                        }
                    }
                }
                return !toCloseToProcess || objectPlaced;
            }
            return false;
        }

        private void GenerateObject(Vector2 loc, float minDistSqrd, List<Vector2> processList, bool preview)
        {
            bool objectPlaced;
            if (CreateRandomObjectAtLoc(loc, minDistSqrd, out objectPlaced, _activeData.PoissonObjects.Element, _activeData.PoissonObjectOptions, preview))
            {
                processList.Add(loc);

                // If object didn't get placed due to dot check, still add it to grid, otherwise and endless loop might occur.
                int remappedX = Mathf.CeilToInt(Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, 0, _activeGrid.GridWidth - 1));
                int remappedY = Mathf.CeilToInt(Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, 0, _activeGrid.GridDepth - 1));

                int gridLoc = remappedY * _activeGrid.GridWidth + remappedX;
                if (_activeGrid.Grid2D[gridLoc] == null)
                {
                    _activeGrid.Grid2D[gridLoc] = new Vector2List();
                }
                _activeGrid.Grid2D[gridLoc].Add(new GridPoint { Point = loc, HasObject = objectPlaced });

                if (objectPlaced && (_activeData.ClumpObjects?.Element.HasWeightedElements() ?? false))
                {
                    int clumpingAmount = Random.Range(_activeData.MinClump, _activeData.MaxClump);

                    for (int i = 0; i < clumpingAmount; ++i)
                    {
                        CreateRandomObjectAtLoc(GenerateRandomPointClumping(loc), -1.0f, out objectPlaced, _activeData.ClumpObjects.Element, _activeData.ClumpObjectOptions, preview);
                    }
                }
            }
        }

        private void DistributionInit(int maxNesting)
        {
            _activeGrid = EditorData.Grids[_activeLevel];
            _activeData = Data[_activeLevel];

            if (_activeData.SphereCollisionCheck || _activeData.BoxCollisionCheck)
            {
                if (_collidersOverlapHelper == null)
                {
                    _collidersOverlapHelper = new Collider[1];
                }
            }
            else
            {
                _collidersOverlapHelper = null;
            }

            if (_activeData.UseSeed)
            {
                Random.InitState(_activeData.Seed);
            }
            float cellSize;
            if (_activeData.Map == null)
            {
                cellSize = 1.0f / (_activeData.MinDist / Mathf.Sqrt(2.0f));
            }
            else
            {
                cellSize = 1.0f / (_activeData.MaxDist / Mathf.Sqrt(2.0f));
            }

            _activeGrid.GridWidth = Mathf.CeilToInt(_surfaceBounds.size.x * cellSize);
            _activeGrid.GridDepth = Mathf.CeilToInt(_surfaceBounds.size.z * cellSize);
            _activeGrid.Grid2D = new Vector2List[_activeGrid.GridWidth * _activeGrid.GridDepth];

            if(maxNesting == -1)
            {
                _maxNestingLevel = _activeData.MaxSubPlacersNesting;
            }
        }

        private void DistributePoisson(int start, int end, bool preview, int currNesting = 0, int maxNesting = -1)
        {
            if (!DataHolder.IsWindow && EditorData.HelperVisual != null)
            {
                EditorData.HelperVisual.transform.SetAsFirstSibling();
            }

            CleanupPlacedObjects(EditorData, start);
            _currNestingLevel = currNesting;

            for (_activeLevel = start; _activeLevel <= end; ++_activeLevel)
            {
                if (EditorData.Grids[_activeLevel].ReadOnly)
                {
                    continue;
                }

                DistributionInit(maxNesting);

                _samples = 0;
                int maxSamples = (preview) ? _activeData.MaxSamplesPreview : _activeData.MaxSamples;

                List<Vector2> processList = new List<Vector2>();

                Vector2 startLoc = GenerateStartPoint();
                float finalMinDist = GetFinalMinDistance(startLoc);
                GenerateObject(startLoc, finalMinDist, processList, preview);

                while (processList.Count > 0 && (maxSamples <= 0 || _samples < maxSamples))
                {
                    int randomIndex = Random.Range(0, processList.Count - 1);
                    Vector2 currPoint = processList[randomIndex];
                    processList.RemoveAt(randomIndex);

                    for (int i = 0; i < _activeData.Samples; ++i)
                    {
                        Vector2 newPoint = GenerateRandomPointAround(currPoint, out finalMinDist);
                        GenerateObject(newPoint, finalMinDist * finalMinDist, processList, preview);
                    }
                }
            }

            EditorData.HighestDistributedLevel = end;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Helpers_KevinLoddewykx.General.WeightedArrayCore;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonHelper
    {
        private static bool GetBounds(GameObject obj, EBoundsMode boundsMode, Vector3 position, Quaternion rotation, Vector3 scale, out Bounds bounds)
        {
            PoissonPlacer placer = obj.GetComponent<PoissonPlacer>();
            if (placer != null)
            {
                Vector3 placerScale = placer.ModeData.Scale;
                placerScale.y = 0.5f;

                Matrix4x4 placerMat = Matrix4x4.TRS(placer.ModeData.Position, placer.ModeData.Rotation, placerScale);
                Matrix4x4 parentMat = Matrix4x4.TRS(position, rotation, scale);

                Matrix4x4 finalMat = parentMat * placerMat;
                bounds = CalculateBoundsFromMatrix(finalMat);
                return true;
            }
            return CalculateUnscaledBounds(obj, boundsMode, position, rotation, scale, out bounds);
        }

        private static Bounds CalculateBoundsFromMatrix(Matrix4x4 finalMat)
        {
            Bounds bounds;
            
            Vector3 center = finalMat.GetColumn(3);
            Vector3 extents = Vector3.zero;

            Vector3[] vectors = new Vector3[]
            {
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(-0.5f, -0.5f, 0.5f),
                        new Vector3(-0.5f, 0.5f, -0.5f),
                        new Vector3(-0.5f, 0.5f, 0.5f),
                        new Vector3(0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, -0.5f, 0.5f),
                        new Vector3(0.5f, 0.5f, -0.5f),
                        new Vector3(0.5f, 0.5f, 0.5f)
            };
            foreach (Vector3 vec in vectors)
            {
                Vector3 point = finalMat.MultiplyPoint(vec);
                extents.x = Mathf.Max(extents.x, Mathf.Abs(point.x - center.x));
                extents.y = Mathf.Max(extents.y, Mathf.Abs(point.z - center.z));
                extents.z = Mathf.Max(extents.z, Mathf.Abs(point.z - center.z));
            }

            bounds = new Bounds(center, extents * 2);
            return bounds;
        }

        private static bool CalculateUnscaledBounds(GameObject obj, EBoundsMode boundsMode, Vector3 position, Quaternion rotation, Vector3 scale, out Bounds bounds)
        {
            Vector3 tempPos = obj.transform.localPosition;
            Quaternion tempRot = obj.transform.localRotation;
            Vector3 tempScale = obj.transform.localScale;
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
            obj.transform.localScale = scale;

            bool result = false;
            switch (boundsMode)
            {
                case EBoundsMode.Renderer:
                    result = CalculateBoundsRenderer(obj, out bounds);
                    break;
                case EBoundsMode.Collider:
                    result = CalculateBoundsColliderInstantiate(obj, out bounds);
                    break;
                default:
                    bounds = new Bounds();
                    break;
            }

            obj.transform.localPosition = tempPos;
            obj.transform.localRotation = tempRot;
            obj.transform.localScale = tempScale;

            return result;
        }

        private static bool CalculateBoundsRenderer(GameObject obj, out Bounds bounds)
        {
            Renderer[] components = obj.GetComponentsInChildren<Renderer>();
            bool hasBounds = false;
            bounds = new Bounds();
            foreach (Renderer comp in components)
            {
                if (comp.enabled)
                {
                    if (!hasBounds)
                    {
                        bounds = comp.bounds;
                        hasBounds = true;
                        continue;
                    }
                    bounds.Encapsulate(comp.bounds);
                }
            }

            return hasBounds;
        }

        private static bool CalculateBoundsColliderInstantiate(GameObject obj, out Bounds bounds)
        {
            GameObject tempObj = Object.Instantiate(obj);
            bool hasBounds = CalculateBoundsCollider(tempObj, out bounds);
            Object.DestroyImmediate(tempObj);

            return hasBounds;
        }

        private static bool CalculateBoundsCollider(GameObject obj, out Bounds bounds)
        {
            Collider[] components = obj.GetComponentsInChildren<Collider>();

            bool hasBounds = false;
            bounds = new Bounds();
            foreach (Collider comp in components)
            {
                if (comp.enabled)
                {
                    if (!hasBounds)
                    {
                        bounds = comp.bounds;
                        hasBounds = true;
                        continue;
                    }
                    bounds.Encapsulate(comp.bounds);
                }
            }
            return hasBounds;
        }

        private bool ValidateSurfaceBounds(bool showErrors)
        {
            switch (ModeData.Mode)
            {
                case DistributionMode.Surface:
                    if (ModeData.Surface != null)
                    {
                        PrefabType prefabType = PrefabUtility.GetPrefabType(ModeData.Surface);
                        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
                        {
                            LogError(showErrors, "Mode -> Surface: surface object needs to be part of the scene.");
                            return false;
                        }

                        _surfaceColliders = ModeData.Surface.GetComponents<Collider>() ?? null;
                        if (!_surfaceColliders.Any())
                        {
                            LogError(showErrors, "Mode -> Surface: surface object requires a collider component.");
                            return false;
                        }

                        _surfaceColliders = _surfaceColliders.Where((c) => c.enabled);
                        if (!_surfaceColliders.Any())
                        {
                            LogError(showErrors, "Mode -> Surface: surface object requires atleast one enabled collider component.");
                            return false;
                        }

                        CalculateBoundsCollider(ModeData.Surface, out _surfaceBounds);
                        if (_surfaceBounds.extents.x == 0 || _surfaceBounds.extents.z == 0)
                        {
                            LogError(showErrors, "Mode -> Surface: and/or Z bounds are zero.");
                            return false;
                        }

                        if (!EditorData.Grids[0].ReadOnly)
                        {
                            foreach (List<GameObject> placedObjects in EditorData.PlacedObjects)
                            {
                                if (placedObjects.Contains(ModeData.Surface))
                                {
                                    LogError(showErrors, "Mode -> Surface: can not use an active generated gameobject as surface.");
                                    return false;
                                }
                            }
                        }
                        return true;
                    }
                    LogError(showErrors, "Mode -> Surface: no surface object set.");
                    return false;
                case DistributionMode.ProjectionPlane:
                case DistributionMode.ProjectionEllipse:
                    {
                        Vector3 scale = EditorData.HelperVisual.transform.localScale;
                        if (scale.x == 0 || scale.y == 0 || scale.z == 0)
                        {
                            LogError(showErrors, "Mode -> Scale: contains a scale of 0 or a parent gameobject.");
                            return false;
                        }

                        _surfaceBounds.size = CalculateBoundsFromMatrix(EditorData.HelperVisual.transform.localToWorldMatrix).size;
                        return true;
                    }
                case DistributionMode.Plane:
                case DistributionMode.Ellipse:
                    {
                        Vector3 scale = EditorData.HelperVisual.transform.lossyScale;
                        if (scale.x == 0 || scale.z == 0)
                        {
                            LogError(showErrors, "Mode -> Scale: contains a scale of 0 or a parent gameobject.");
                            return false;
                        }

                        Matrix4x4 finalMat = Matrix4x4.Scale(new Vector3(1, 0, 1)) * EditorData.HelperVisual.transform.localToWorldMatrix;
                        _surfaceBounds.size = CalculateBoundsFromMatrix(finalMat).size;
                        return true;
                    }
                default:
                    return false;
            }
        }

        private static bool ValidateMap(Texture2D map, int index, bool showErrors)
        {
            if (map != null)
            {
                try
                {
                   map.GetPixel(0, 0);
                }
                catch (UnityException e)
                {
                    if (e.Message.StartsWith("Texture '" + map.name + "' is not readable"))
                    {
                        LogError(showErrors, "Poisson -> Map [Level " + index + "]: please enable read/write on texture [" + map.name + "]");
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool ValidateLevel(PoissonData data, int index, bool showErrors)
        {
            if (data.PoissonObjects == null)
            {
                LogError(showErrors, "Poisson -> Poisson Data [Level " + index + "]: variable not set");
                return false;
            }
            else if (!data.PoissonObjects.Element.HasWeightedElementsNonNull())
            {
                LogError(showErrors, "Poisson -> Poisson Data [Level " + index + "]: no non null weighted elements inside the weighted data object");
                return false;
            }

            if ((!data.ClumpObjects?.Element.HasWeightedElementsNonNull()) ?? false)
            {
                LogError(showErrors, "Clumping -> Clump Data [Level " + index + "]: no non null weighted elements inside the weighted data object");
                return false;
            }

            return ValidateMap(data.Map, index, showErrors);
        }

        private static void LogError(bool showError, string errorMsg)
        {
            if (showError)
            {
                EditorGUILayout.HelpBox(errorMsg, MessageType.Error, true);
            }
        }

        private void ValidateSettings(bool showErrors, out bool isValidSurface, out bool preValid, out bool currValid, out bool postValid, out int highestValid)
        {
            isValidSurface = ValidateSurfaceBounds(showErrors);
            preValid = true;
            currValid = true;
            postValid = true;
            highestValid = -1;
            if (isValidSurface)
            {
                for (int i = 0; i < Data.Count; ++i)
                {
                    PoissonData data = Data[i];
                    if (i < UIData.SelectedLevelIndex)
                    {
                        preValid &= ValidateLevel(data, i, showErrors);
                        if(preValid)
                        {
                            highestValid = i;
                        }
                    }
                    else if (i > UIData.SelectedLevelIndex)
                    {
                        postValid &= ValidateLevel(data, i, showErrors);
                        if (postValid)
                        {
                            highestValid = i;
                        }
                    }
                    else
                    {
                        currValid = ValidateLevel(data, i, showErrors);
                        if (currValid)
                        {
                            highestValid = i;
                        }
                    }
                }
            }
        }
    }
}

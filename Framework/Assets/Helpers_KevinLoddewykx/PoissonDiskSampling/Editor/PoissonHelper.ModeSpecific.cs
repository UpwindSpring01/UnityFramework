using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Helpers_KevinLoddewykx.PoissonDiskSampling
{
    public partial class PoissonHelper
    {
        private void CreateModeSpecificUI(float halfWidth)
        {
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = BACKGROUND_COLOR_SUB_MENU;
            EditorGUILayout.BeginVertical(SubBoxStyle);
            halfWidth -= (DataHolder.IsWindow) ? BoxMargin : BoxMargin * 0.5f;
            GUI.backgroundColor = oldColor;

            if (ModeData.Mode != DistributionMode.Surface)
            {
                if (ModeData.Mode >= DistributionMode.ProjectionPlane)
                {
                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                    SelectedData.ProjectionLayerMask = EditorGUILayout.MaskField("Layer mask:", SelectedData.ProjectionLayerMask, InternalEditorUtility.layers, PopupStyle);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                    SelectedData.ProjectionRaycastTriggerInteraction = (QueryTriggerInteraction)EditorGUILayout.EnumPopup("Trigger query mode:", SelectedData.ProjectionRaycastTriggerInteraction, PopupStyle);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginVertical(SingleRowStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap));
                EditorGUI.BeginChangeCheck();

                Vector3 pos = EditorGUILayout.Vector3Field("Position:", ModeData.Position);
                Quaternion rot = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation:", ModeData.Rotation.eulerAngles));
                Vector3 scale = ModeData.Scale;
                if (ModeData.Mode >= DistributionMode.ProjectionPlane)
                {
                    scale = EditorGUILayout.Vector3Field("Scale:", scale);
                }
                else
                {
                    Vector2 scale2D = EditorGUILayout.Vector2Field("Scale:", new Vector2(scale.x, scale.z));
                    scale.x = scale2D.x;
                    scale.z = scale2D.y;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    ModeData.Position = pos;
                    ModeData.Rotation = rot;
                    ModeData.Scale = scale;
                    if (!IsPrefab)
                    {
                        EditorData.HelperVisual.transform.localPosition = pos;
                        EditorData.HelperVisual.transform.localRotation = rot;
                        EditorData.HelperVisual.transform.localScale = scale;
                        SceneView.RepaintAll();
                    }
                }

                EditorGUI.BeginChangeCheck();
                ModeData.RejectPercentageX = EditorGUILayout.Slider("Exclude X %:", ModeData.RejectPercentageX, 0.0f, 1.0f);
                ModeData.RejectPercentageY = EditorGUILayout.Slider("Exclude Z %:", ModeData.RejectPercentageY, 0.0f, 1.0f);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorData.UpdateVisualPercentages(ModeData);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(RowStyle);

                EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(EditorData.Grids[0].ReadOnly || !DataHolder.IsWindow))
                {
                    ModeData.Surface = (GameObject)EditorGUILayout.ObjectField("Surface:", ModeData.Surface, typeof(GameObject), true);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private Vector2 GenerateStartPoint()
        {
            switch (ModeData.Mode)
            {
                case DistributionMode.Ellipse:
                case DistributionMode.ProjectionEllipse:
                    {
                        float angle = Random.Range(0.0f, 2.0f * Mathf.PI);
                        float radius = Mathf.Sqrt(Random.Range(0.0f, 1.0f));
                        float x = (radius * Mathf.Cos(angle)) * (_surfaceBounds.size.x * 0.5f);
                        float z = (radius * Mathf.Sin(angle)) * (_surfaceBounds.size.z * 0.5f);

                        return new Vector2(x, z);
                    }
                case DistributionMode.Surface:
                case DistributionMode.ProjectionPlane:
                case DistributionMode.Plane:
                default:
                    return new Vector2(Random.Range(_surfaceBounds.min.x, _surfaceBounds.max.x), Random.Range(_surfaceBounds.min.z, _surfaceBounds.max.z));
            }

        }

        private bool InBounds(Vector2 loc)
        {
            if (ModeData.Mode == DistributionMode.ProjectionEllipse || ModeData.Mode == DistributionMode.Ellipse)
            {
                return ((loc.x * loc.x) / (_surfaceBounds.extents.x * _surfaceBounds.extents.x)
                  + (loc.y * loc.y) / (_surfaceBounds.extents.z * _surfaceBounds.extents.z)) <= 1.0f;
            }
            else
            {
                return loc.x >= _surfaceBounds.min.x && loc.x <= _surfaceBounds.max.x
                    && loc.y >= _surfaceBounds.min.z && loc.y <= _surfaceBounds.max.z;
            }
        }

        private bool InInnerBounds(Vector2 loc)
        {
            bool result = false;
            switch (ModeData.Mode)
            {
                case DistributionMode.Surface:
                    break;
                case DistributionMode.Plane:
                case DistributionMode.ProjectionPlane:
                    {
                        float x = _surfaceBounds.extents.x * ModeData.RejectPercentageX;
                        float z = _surfaceBounds.extents.z * ModeData.RejectPercentageY;

                        result = loc.x >= _surfaceBounds.center.x - x && loc.x <= _surfaceBounds.center.x + x
                            && loc.y >= _surfaceBounds.center.z - z && loc.y <= _surfaceBounds.center.z + z;
                    }
                    break;
                case DistributionMode.Ellipse:
                case DistributionMode.ProjectionEllipse:
                    {
                        float x = _surfaceBounds.extents.x * ModeData.RejectPercentageX;
                        float z = _surfaceBounds.extents.z * ModeData.RejectPercentageY;
                        result = ((loc.x * loc.x) / (x * x)
                          + (loc.y * loc.y) / (z * z)) <= 1.0f;
                    }
                    break;
            }
            return result;
        }

        private bool CastRay(Vector2 loc, out RaycastHit hit)
        {
            switch (ModeData.Mode)
            {
                case DistributionMode.Surface:
                    foreach (Collider col in _surfaceColliders)
                    {
                        if (col.Raycast(new Ray(new Vector3(loc.x, _surfaceBounds.max.y + 1, loc.y), Vector3.down), out hit, _surfaceBounds.size.y + 2))
                        {
                            return true;
                        }
                    }
                    hit = new RaycastHit();
                    return false;
                case DistributionMode.ProjectionPlane:
                case DistributionMode.ProjectionEllipse:
                    {
                        float xPos = Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, -0.5f, 0.5f);
                        float zPos = Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, -0.5f, 0.5f);

                        Transform transform = EditorData.HelperVisual.transform;
                        Matrix4x4 mat = transform.localToWorldMatrix;
                        Vector3 startPos = mat.MultiplyPoint(new Vector3(xPos, 0.0f, zPos));

                        LayerMask correctedMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(_activeData.ProjectionLayerMask);
                        Vector3 direction = mat.MultiplyPoint(new Vector3(xPos, -1.0f, zPos)) - startPos;
                        return Physics.Raycast(startPos, direction, out hit, direction.magnitude, correctedMask, _activeData.ProjectionRaycastTriggerInteraction);
                    }
                case DistributionMode.Plane:
                case DistributionMode.Ellipse:
                    {
                        hit = new RaycastHit();

                        float xPos = Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, -0.5f, 0.5f);
                        float zPos = Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, -0.5f, 0.5f);

                        Transform transform = EditorData.HelperVisual.transform;
                        Matrix4x4 mat = transform.localToWorldMatrix;

                        hit.normal = mat.MultiplyVector(Vector3.up).normalized;
                        hit.point = mat.MultiplyPoint(new Vector3(xPos, 0.0f, zPos));

                        return true;
                    }
                default:
                    hit = new RaycastHit();
                    return true;
            }
        }
    }
}

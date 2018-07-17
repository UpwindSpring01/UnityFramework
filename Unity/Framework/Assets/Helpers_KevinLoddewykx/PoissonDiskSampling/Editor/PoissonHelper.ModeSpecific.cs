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
            halfWidth -= (_window == null) ? BoxMargin * 0.5f : BoxMargin;
            GUI.backgroundColor = oldColor;

            if (_modeData.Mode != DistributionMode.Surface)
            {
                if (_modeData.Mode >= DistributionMode.ProjectionRect)
                {
                    EditorGUILayout.BeginHorizontal(RowStyle);
                    EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                    _selectedData.ProjectionLayerMask = EditorGUILayout.MaskField("Layer mask:", _selectedData.ProjectionLayerMask, InternalEditorUtility.layers, PopupStyle);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                    _selectedData.ProjectionRaycastTriggerInteraction = (QueryTriggerInteraction)EditorGUILayout.EnumPopup("Trigger query mode:", _selectedData.ProjectionRaycastTriggerInteraction, PopupStyle);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginVertical(SingleRowStyle, GUILayout.MaxWidth(halfWidth * 2 + ColumnGap));
                EditorGUI.BeginChangeCheck();

                if (!_isPrefab && _editorData.HelperVisual.transform.hasChanged)
                {
                    _editorData.Position = _editorData.HelperVisual.transform.localPosition;
                    _editorData.Rotation = _editorData.HelperVisual.transform.localRotation;
                    _editorData.Scale = _editorData.HelperVisual.transform.localScale;
                }

                Vector3 pos = EditorGUILayout.Vector3Field("Position:", _editorData.Position);
                Quaternion rot = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation:", _editorData.Rotation.eulerAngles));
                Vector3 scale = _editorData.Scale;
                if (_modeData.Mode >= DistributionMode.ProjectionRect)
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
                    _editorData.Position = pos;
                    _editorData.Rotation = rot;
                    _editorData.Scale = scale;
                    if (!_isPrefab)
                    {
                        _editorData.HelperVisual.transform.localPosition = pos;
                        _editorData.HelperVisual.transform.localRotation = rot;
                        _editorData.HelperVisual.transform.localScale = scale;
                        SceneView.RepaintAll();
                    }
                }

                EditorGUI.BeginChangeCheck();
                _modeData.RejectPercentage.x = EditorGUILayout.Slider("Exclude X %:", _modeData.RejectPercentage.x, 0.0f, 1.0f);
                _modeData.RejectPercentage.y = EditorGUILayout.Slider("Exclude Z %:", _modeData.RejectPercentage.y, 0.0f, 1.0f);
                if (EditorGUI.EndChangeCheck())
                {
                    _editorData.UpdateVisualPercentages(_modeData);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(RowStyle);

                EditorGUILayout.BeginVertical(SubLeftColumnStyle, GUILayout.MaxWidth(halfWidth));
                using (new EditorGUI.DisabledScope(_editorData.Grids[0].ReadOnly))
                {
                    _modeData.SurfaceMeshFilter = (MeshFilter)EditorGUILayout.ObjectField("Surface:", _modeData.SurfaceMeshFilter, typeof(MeshFilter), true);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(RightColumnStyle, GUILayout.MaxWidth(halfWidth));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private Vector2 GenerateStartPoint()
        {
            switch (_modeData.Mode)
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
                case DistributionMode.ProjectionRect:
                case DistributionMode.Plane:
                default:
                    return new Vector2(Random.Range(_surfaceBounds.min.x, _surfaceBounds.max.x), Random.Range(_surfaceBounds.min.z, _surfaceBounds.max.z));
            }

        }

        private bool InBounds(Vector2 loc)
        {
            if (_modeData.Mode == DistributionMode.ProjectionEllipse || _modeData.Mode == DistributionMode.Ellipse)
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
            switch (_modeData.Mode)
            {
                case DistributionMode.Surface:
                    break;
                case DistributionMode.Plane:
                case DistributionMode.ProjectionRect:
                    {
                        float x = _surfaceBounds.extents.x * _modeData.RejectPercentage.x;
                        float z = _surfaceBounds.extents.z * _modeData.RejectPercentage.y;

                        result = loc.x >= _surfaceBounds.center.x - x && loc.x <= _surfaceBounds.center.x + x
                            && loc.y >= _surfaceBounds.center.z - z && loc.y <= _surfaceBounds.center.z + z;
                    }
                    break;
                case DistributionMode.Ellipse:
                case DistributionMode.ProjectionEllipse:
                    {
                        float x = _surfaceBounds.extents.x * _modeData.RejectPercentage.x;
                        float z = _surfaceBounds.extents.z * _modeData.RejectPercentage.y;
                        result = ((loc.x * loc.x) / (x * x)
                          + (loc.y * loc.y) / (z * z)) <= 1.0f;
                    }
                    break;
            }
            return result;
        }

        private bool CastRay(Vector2 loc, out RaycastHit hit)
        {
            switch (_modeData.Mode)
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
                case DistributionMode.ProjectionRect:
                case DistributionMode.ProjectionEllipse:
                    {
                        float xPos = Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, -0.5f, 0.5f);
                        float zPos = Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, -0.5f, 0.5f);

                        Transform transform = _editorData.HelperVisual.transform;
                        Matrix4x4 mat = transform.localToWorldMatrix;
                        Vector3 startPos = mat.MultiplyPoint(new Vector3(xPos, 0.0f, zPos));

                        LayerMask correctedMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(_activeData.ProjectionLayerMask);
                        return Physics.Raycast(startPos, mat.MultiplyPoint(new Vector3(xPos, -1.0f, zPos)) - startPos, out hit, _surfaceBounds.size.y, correctedMask, _activeData.ProjectionRaycastTriggerInteraction);
                    }
                case DistributionMode.Plane:
                case DistributionMode.Ellipse:
                    {
                        hit = new RaycastHit();

                        float xPos = Remap(loc.x, _surfaceBounds.min.x, _surfaceBounds.max.x, -0.5f, 0.5f);
                        float zPos = Remap(loc.y, _surfaceBounds.min.z, _surfaceBounds.max.z, -0.5f, 0.5f);

                        Transform transform = _editorData.HelperVisual.transform;
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

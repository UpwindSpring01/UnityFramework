using UnityEngine;
using UnityEditor;

namespace Helpers_KevinLoddewykx.General
{
    [CustomEditor(typeof(Transform))]
    public class TransformInspectorEditor : Editor
    {
        protected static Vector3 savedPosition = Vector3.zero;
        protected static Vector3 savedEulerAngles = Vector3.zero;
        protected static Vector3 savedScale = Vector3.zero;


        public override void OnInspectorGUI()
        {
            const float widthSingleBtn = 16;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin.left = 0;
            buttonStyle.margin.right = 0;
            buttonStyle.padding.left = 3;
            buttonStyle.padding.right = 3;

            Transform t = (Transform)target;

            // Replicate the standard transform inspector gui
            EditorGUIUtility.labelWidth = 58;
            EditorGUI.indentLevel = 0;

            GUILayout.BeginHorizontal();
            Vector3 position = EditorGUILayout.Vector3Field("Position", t.localPosition);
            if (GUILayout.Button("0", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                position = Vector3.zero;
            }
            if (GUILayout.Button("C", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                savedPosition = position;
            }
            if (GUILayout.Button("P", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                position = savedPosition;
            }
            if (GUILayout.Button("S", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                EditorGUIUtility.systemCopyBuffer = SerializeV3ForCode(position);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
            if (GUILayout.Button("0", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                eulerAngles = Vector3.zero;
            }
            if (GUILayout.Button("C", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                savedEulerAngles = eulerAngles;
            }
            if (GUILayout.Button("P", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                eulerAngles = savedEulerAngles;
            }
            if (GUILayout.Button("S", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                EditorGUIUtility.systemCopyBuffer = SerializeV3ForCode(eulerAngles);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Vector3 scale = EditorGUILayout.Vector3Field("Scale", t.localScale);
            if (GUILayout.Button("1", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                scale = Vector3.one;
            }
            if (GUILayout.Button("C", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                savedScale = scale;
            }
            if (GUILayout.Button("P", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                scale = savedScale;
            }
            if (GUILayout.Button("S", buttonStyle, GUILayout.Width(widthSingleBtn)))
            {
                EditorGUIUtility.systemCopyBuffer = SerializeV3ForCode(scale);
            }
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("C"))
            {
                savedPosition = position;
                savedEulerAngles = eulerAngles;
                savedScale = scale;
            }
            GUILayout.Space(-1.0f);
            if (GUILayout.Button("P"))
            {
                position = savedPosition;
                eulerAngles = savedEulerAngles;
                scale = savedScale;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Uniform X scale"))
            {
                scale = new Vector3(scale.x, scale.x, scale.x);
            }

            GUILayout.FlexibleSpace();
            Transform parentT = t.parent;
            while (parentT?.hideFlags.HasFlag(HideFlags.HideInHierarchy) ?? false)
            {
                parentT = parentT.parent;
            }

            GUI.enabled = parentT != null;
            if (GUILayout.Button("▲"))
            {
                Selection.activeTransform = parentT;
            }

            Transform childT = null;
            for (int i = 0; i < t.childCount; ++i)
            {
                if (!t.GetChild(i).hideFlags.HasFlag(HideFlags.HideInHierarchy))
                {
                    childT = t.GetChild(i);
                    break;
                }
            }
            GUI.enabled = childT != null;
            if (GUILayout.Button("▼"))
            {
                Selection.activeGameObject = childT.gameObject;
            }

            GUI.enabled = true;
            if (GUILayout.Button("+"))
            {
                GameObject child = new GameObject("Child");
                child.transform.parent = t;
                child.transform.localPosition = Vector3.zero;
                child.transform.localEulerAngles = Vector3.zero;
                child.transform.localScale = Vector3.one;

                Undo.RegisterCreatedObjectUndo(child, "Add Child");

                Selection.activeGameObject = child;
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                Undo.RecordObject(t, "Transform Change");

                t.localPosition = FixIfNaN(position);
                t.localEulerAngles = FixIfNaN(eulerAngles);
                t.localScale = FixIfNaN(scale);
            }
        }

        private Vector3 FixIfNaN(Vector3 v)
        {
            if (float.IsNaN(v.x))
            {
                v.x = 0;
            }
            if (float.IsNaN(v.y))
            {
                v.y = 0;
            }
            if (float.IsNaN(v.z))
            {
                v.z = 0;
            }
            return v;
        }

        private string SerializeV3ForCode(Vector3 input)
        {
            string result = "";

            result = "Vector3(" +
                input.x.ToString() + "f, " +
                input.y.ToString() + "f, " +
                input.z.ToString() + "f)";

            return result;
        }
    }
}
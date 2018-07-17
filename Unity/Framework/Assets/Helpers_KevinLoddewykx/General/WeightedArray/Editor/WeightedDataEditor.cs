using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Helpers_KevinLoddewykx.General.WeightedArray
{
    [CustomEditor(typeof(WeightedScriptableObject), true)]
    public class WeightedDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WeightedScriptableObject weightedData = (WeightedScriptableObject)target;
            foreach (WeightedArrayObject ele in weightedData.Elements)
            {
                using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
                {
                    CreateGUI(ele);
                    if(changeScope.changed)
                    {
                        ele.WeightedArray.RecalcTotalWeight();

                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }

        private void CreateGUI(WeightedArrayObject objArrayObject)
        {
            WeightedArray weightedArray = objArrayObject.WeightedArray;
            // title
            GUILayout.Label(objArrayObject.Name, EditorStyles.boldLabel);

            // list
            if (weightedArray.HasElements())
            {
                GUILayout.BeginVertical(GUI.skin.box);

                EditorGUIUtility.labelWidth = 80.0f;
                int totalchance = weightedArray.TotalWeight;
                float toProcentScaler = 100.0f / totalchance;

                for (int i = 0; i < weightedArray.Objects.Count; ++i)
                {
                    GUILayout.BeginHorizontal();

                    WeightedObject weightedObj = weightedArray.Objects[i];
                    float chance = weightedObj.Weight * toProcentScaler;
                    weightedObj.Object = (GameObject)EditorGUILayout.ObjectField(
                        new GUIContent((i + 1) + ".) " + chance.ToString("F2") + "%"),
                        weightedObj.Object, typeof(GameObject), false);
                    if(!weightedArray.AllowNull && weightedObj.Object == null)
                    {
                        weightedArray.Objects.RemoveAt(i);
                        --i;
                        GUILayout.EndHorizontal();
                        return;
                    }
                    weightedArray.Objects[i].Weight = Mathf.Max(0, EditorGUILayout.IntField(weightedArray.Objects[i].Weight, GUILayout.Width(75)));

                    if (GUILayout.Button(new GUIContent("X", "Remove"), GUILayout.Width(35)))
                    {
                        weightedArray.Objects.RemoveAt(i);
                        --i;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUIUtility.labelWidth = 0;
                GUILayout.EndVertical();
            }

            // add
            GameObject go = (GameObject)EditorGUILayout.ObjectField("Add Element", null, typeof(GameObject), false);
            if (go != null)
            {
                WeightedObject newObj = new WeightedObject() { Object = go, Weight = 1 };
                if (weightedArray.Objects == null)
                {
                    weightedArray.Objects = new List<WeightedObject>() { newObj };
                }
                else
                {
                    weightedArray.Objects.Add(newObj);
                }
            }
        }
    }
}
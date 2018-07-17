using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                    if (_instance == null)
                    {
                        _instance = CreateInstance<T>();
                        //if (!AssetDatabase.IsValidFolder("Asserts/Resources"))
                        //{
                        //    AssetDatabase.CreateFolder("Assets", "Resources");

                        //}
                        //if (!AssetDatabase.IsValidFolder("Asserts/Resources/Editor"))
                        //{
                        //    AssetDatabase.CreateFolder("Assets/Resources", "Editor");
                        //}
                        //AssetDatabase.CreateAsset(_instance, "Assets/Resources/Editor/PoissonInternalData.Asset");

                    }
                }
                return _instance;
            }
        }
    }
}

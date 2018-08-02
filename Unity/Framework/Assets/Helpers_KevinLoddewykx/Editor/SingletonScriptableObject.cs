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
                    _instance = CreateInstance<T>();
                }
                return _instance;
            }
        }
    }
}

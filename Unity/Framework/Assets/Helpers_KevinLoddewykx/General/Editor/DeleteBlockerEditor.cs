using UnityEditor;
using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    [CustomEditor(typeof(DeleteBlocker))]
    public class DeleteBlockerEditor : Editor
    {
        private void OnSceneGUI()
        {
            if (Event.current.type == EventType.ValidateCommand || Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "SoftDelete")
                {
                    Event.current.Use();
                }
            }
        }
    }
}

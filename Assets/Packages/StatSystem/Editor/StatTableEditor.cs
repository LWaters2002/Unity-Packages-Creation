using SkillTree.Runtime;
using UnityEditor;
using UnityEngine;

namespace StatSystem.Editor
{
    [CustomEditor(typeof(StatTable))]
    public class SkillTreeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Stat Table Editor"))
            {
                StatTableEditorWindow.Open((StatTable)target);
            }
            
            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                Debug.Log("Saved Stat Table");
            }
        }
    }
}
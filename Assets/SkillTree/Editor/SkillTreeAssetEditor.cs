using SkillTree.Runtime;
using UnityEditor;
using UnityEngine;

namespace SkillTree.Editor
{
    [CustomEditor(typeof(SkillTreeAsset))]
    public class SkillTreeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Skill Tree Editor"))
            {
                SkillTreeEditorWindow.Open((SkillTreeAsset)target);
            }
        
            base.OnInspectorGUI();
        }
    }
}
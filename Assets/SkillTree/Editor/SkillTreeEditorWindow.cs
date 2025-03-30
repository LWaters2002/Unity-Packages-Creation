using SkillTree.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeEditorWindow : EditorWindow
    {
        [SerializeField]
        private SkillTreeAsset activeSkillTreeAsset;
    
        [SerializeField]
        private SkillTreeGraphView graphView;

        [SerializeField]
        SerializedObject serializedObject;
    
        private void OnGUI()
        {
            GUILayout.Label("Skill Tree Editor", EditorStyles.boldLabel);
        }

        public static void Open(SkillTreeAsset targetAsset)
        {
            SkillTreeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<SkillTreeEditorWindow>();
            foreach (SkillTreeEditorWindow window in windows)
            {
                window.Close();
            }

            SkillTreeEditorWindow newWindow = CreateWindow<SkillTreeEditorWindow>(typeof(SkillTreeEditorWindow), typeof(SceneView));
            newWindow.titleContent = new GUIContent($"Skill Tree Editor - {targetAsset.name}", EditorGUIUtility.ObjectContent(targetAsset, targetAsset.GetType()).image);
            newWindow.Load(targetAsset); 
        }

        private void Load(SkillTreeAsset targetAsset)
        {
            activeSkillTreeAsset = targetAsset;
            DrawGraph();
        }

        private void DrawGraph()
        {
            serializedObject = new SerializedObject(activeSkillTreeAsset);
            graphView = new SkillTreeGraphView(serializedObject, this);
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }
    }
}

using System;
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
    
        private SkillTreeGraphView graphView;
        SerializedObject serializedObject;

        private void OnGUI()
        {
            GUILayout.Label("Skill Tree Editor", EditorStyles.boldLabel);
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += RefreshWindow;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshWindow;
        }
        
        private void RefreshWindow()
        {
            if (activeSkillTreeAsset == null) return;
            
            Open(activeSkillTreeAsset);
            Repaint();
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
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
            InitGraph();
        }

        private void InitGraph()
        {
            serializedObject = new SerializedObject(activeSkillTreeAsset);
            graphView = new SkillTreeGraphView(serializedObject, this);
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
            graphView.Refresh();
        }
    }
}

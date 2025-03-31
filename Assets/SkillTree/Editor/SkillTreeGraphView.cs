using System;
using System.Collections.Generic;
using SkillTree.Runtime;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using SkillTree.Runtime.UI;

namespace SkillTree.Editor
{
    public class SkillTreeGraphView : GraphView
    {
        private SerializedObject _serializedObject;
        public SkillTreeAsset CurrentSkillTreeAsset { get; set; }

        public List<SkillTreeEditorNodeTransition> NodeTransitions { get; set; } =
            new List<SkillTreeEditorNodeTransition>();

        public SkillTreeEditorWindow EditorWindow { get; private set; }
        public Dictionary<string, SkillTreeEditorNode> NodeLookup { get; private set; }
        public List<SkillTreeEditorNode> SkillTreeNodes { get; private set; }

        public SkillTreeGraphView(SerializedObject serializedObject, SkillTreeEditorWindow editorWindow)
        {
            NodeLookup = new Dictionary<string, SkillTreeEditorNode>();
            SkillTreeNodes = new List<SkillTreeEditorNode>();
            _serializedObject = serializedObject;
            EditorWindow = editorWindow;
            CurrentSkillTreeAsset = serializedObject.targetObject as SkillTreeAsset;

            LoadStyleSheets();
            SetupBackground();
            AddManipulators();

            contentViewContainer.BringToFront();
            Undo.undoRedoPerformed += this.Refresh;
        }

        private void LoadStyleSheets()
        {
            List<string> paths = new List<string>()
            {
                "Assets/SkillTree/Editor/USS/SkillTreeEditor.uss",
                "Assets/SkillTree/Editor/USS/Node.uss"
            };

            foreach (string path in paths)
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                styleSheets.Add(styleSheet);
            }
        }

        private void SetupBackground()
        {
            GridBackground background = new GridBackground();
            background.name = "Grid";

            background.StretchToParentSize();
            background.SendToBack();

            Add(background);
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create new skill node", AddNodeAction);
            }));
        }
        
        public void TransitionCreated(SkillTreeEditorNodeTransition transition, bool registerObject = true)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Added Transition");

            if (registerObject)
            {
                SkillTreeEditorNode startTarget = (SkillTreeEditorNode)transition.StartTarget;
                SkillTreeEditorNode endTarget = (SkillTreeEditorNode)transition.EndTarget;

                int index = CurrentSkillTreeAsset.Nodes.FindIndex(x => x.ID == endTarget.ID);

                if (index != -1)
                {
                    CurrentSkillTreeAsset.Nodes[index].ParentGuids.Add(startTarget.ID);
                }

                _serializedObject.Update();
            }

            NodeTransitions.Add(transition);
        }

        public void TransitionRemoved(SkillTreeEditorNodeTransition transition, bool registerObject = true)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Removed Transition");

            if (registerObject)
            {
                SkillTreeEditorNode startTarget = (SkillTreeEditorNode)transition.StartTarget;
                SkillTreeEditorNode endTarget = (SkillTreeEditorNode)transition.EndTarget;

                int index = CurrentSkillTreeAsset.Nodes.FindIndex(x => x.ID == endTarget.ID);

                if (index != -1)
                {
                    CurrentSkillTreeAsset.Nodes[index].ParentGuids.Remove(startTarget.ID);
                }

                _serializedObject.Update();
            }
            
            _serializedObject.Update();

            NodeTransitions.Remove(transition);

            if (Contains(transition))
            {
                RemoveElement(transition);
            }
        }

        private void AddNodeAction(DropdownMenuAction dropdownMenuAction)
        {
            Vector2 mousePos = dropdownMenuAction.eventInfo.mousePosition;

            mousePos = contentViewContainer.WorldToLocal(mousePos);

            SkillTreeNodeData node = new SkillTreeNodeData();
            node.ID = Guid.NewGuid().ToString();
            node.SetPosition(new Rect(mousePos.x, mousePos.y, 64, 64));
            node.SetProperties(new NodeProperties()
            {
                Title = "New Skill Node",
                Description = "New Skill Node Description",
                Icon = null,
                Cost = 1,
            });

            AddNodeToGraph(node);
        }

        private void RegisterNodeData(SkillTreeNodeData nodeData, SkillTreeEditorNode node)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Added Node");
            
            CurrentSkillTreeAsset.Nodes.Add(nodeData);
            _serializedObject.Update();
        }

        public void AddNodeToGraph(SkillTreeNodeData node, bool registerToObject = true)
        {
            SkillTreeEditorNode newNode = new SkillTreeEditorNode(node.ID,this);
            newNode.title = node.Properties.Title;
            newNode.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SkillTree/Editor/USS/Node.uss"));
            node.typeName = node.GetType().AssemblyQualifiedName;
            newNode.SetPosition(node.Position);
            newNode.BringToFront();

            SkillTreeNodes.Add(newNode);
            NodeLookup.Add(node.ID, newNode);

            if (registerToObject)
            {
                RegisterNodeData(node, newNode);
            }

            AddElement(newNode);
            
            newNode.RegisterCallback<GeometryChangedEvent>(UpdateNodePosition);
        }

        private void UpdateNodePosition(GeometryChangedEvent evt)
        {
            if (evt.target is not SkillTreeEditorNode node) return;
            
            int index = CurrentSkillTreeAsset.Nodes.FindIndex(x => x.ID == node.ID);
            if (index != -1)
            {
                CurrentSkillTreeAsset.Nodes[index].SetPosition(node.layout);
            }

            _serializedObject.Update();
        }
    }
}
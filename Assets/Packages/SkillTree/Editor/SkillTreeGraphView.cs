using System;
using System.Collections.Generic;
using SkillTree.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public partial class SkillTreeGraphView : GraphView
    {
        private readonly SerializedObject _serializedObject;
        public SkillTreeAsset CurrentSkillTreeAsset { get; set; }

        public List<SkillTreeEditorNodeTransition> NodeTransitions { get; set; } = new();

        public SkillTreeEditorWindow EditorWindow { get; private set; }
        public Dictionary<string, SkillTreeEditorNode> NodeLookup { get; private set; }
        public List<SkillTreeEditorNode> SkillTreeNodes { get; private set; }
        public Label CentreLabel { get; private set; }

        private SkillTreeEditorNodeInspector _nodeInspector;

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
            
            InitCommands();

            AddNodeInspector();
        }

        private void AddNodeInspector()
        {
            _nodeInspector = new SkillTreeEditorNodeInspector(this);
            Add(_nodeInspector);
            _nodeInspector.BringToFront();
        }
        
        public void AddCentreLabel()
        {
            CentreLabel = new Label("+");
            CentreLabel.AddToClassList("CentreCross");
            CentreLabel.pickingMode = PickingMode.Ignore;
            CentreLabel.SendToBack();
            contentViewContainer.Insert(0, CentreLabel);
        }

        private void RemoveNode(SkillTreeEditorNode node)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Removed Node");

            SkillTreeNodes.Remove(node);
            if (this.GetSkillTreeNodeDataIndex(node.ID) is { } nodeData)
            {
                CurrentSkillTreeAsset.Nodes.Remove(nodeData);
            }

            for (var i = NodeTransitions.Count - 1; i >= 0; i--)
            {
                SkillTreeEditorNodeTransition transition = NodeTransitions[i];
                if (transition.StartTarget == node || transition.EndTarget == node)
                {
                    NodeTransitions.Remove(transition);
                    RemoveElement(transition);
                }
            }

            UpdateAsset();
        }

        private void LoadStyleSheets()
        {
            var paths = new List<string>()
            {
                "StyleSheets/SkillTreeEditor",
                "StyleSheets/SkillTreeEditorNode"
            };

            foreach (string path in paths)
            {
                StyleSheet styleSheet = Resources.Load<StyleSheet>(path);
                styleSheets.Add(styleSheet);
            }
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            _nodeInspector.SelectionUpdated(selection);
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            _nodeInspector.SelectionUpdated(selection);
        }

        private void SetupBackground()
        {
            GridBackground background = new GridBackground
            {
                name = "Grid"
            };

            background.StretchToParentSize();

            Insert(0, background);
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        public void TransitionCreated(SkillTreeEditorNodeTransition transition, bool registerObject = true)
        {
            transition.SendToBack();

            if (registerObject)
            {
                Undo.RecordObject(_serializedObject.targetObject, "Added Transition");

                SkillTreeEditorNode startTarget = (SkillTreeEditorNode)transition.StartTarget;
                SkillTreeEditorNode endTarget = (SkillTreeEditorNode)transition.EndTarget;

                if (this.GetSkillTreeNodeDataIndex(endTarget.ID) is { } nodeData)
                    nodeData.ParentGuids.Add(startTarget.ID);

                UpdateAsset();
            }

            NodeTransitions.Add(transition);
        }

        public void TransitionRemoved(SkillTreeEditorNodeTransition transition, bool registerObject = true)
        {
            if (registerObject)
            {
                Undo.RecordObject(_serializedObject.targetObject, "Removed Transition");

                SkillTreeEditorNode startTarget = (SkillTreeEditorNode)transition.StartTarget;
                SkillTreeEditorNode endTarget = (SkillTreeEditorNode)transition.EndTarget;

                if (this.GetSkillTreeNodeDataIndex(endTarget.ID) is { } nodeData)
                    nodeData.ParentGuids.Add(startTarget.ID);

                UpdateAsset();
            }

            NodeTransitions.Remove(transition);

            if (Contains(transition))
            {
                RemoveElement(transition);
            }
        }

        private void AddNodeAction()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = contentViewContainer.WorldToLocal(mousePos);
            
            SkillTreeNodeData node = new SkillTreeNodeData
            {
                ID = Guid.NewGuid().ToString()
            };

            node.SetPosition(new Rect(mousePos.x, mousePos.y, 64, 64));
            node.SetProperties(new NodeProperties()
            {
                title = "New Skill Node",
                description = "New Skill Node Description",
                icon = null,
                cost = 1,
            });

            AddNodeToGraph(node);
        }

        private void RegisterNodeData(SkillTreeNodeData nodeData, SkillTreeEditorNode node)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Added Node");

            CurrentSkillTreeAsset.Nodes.Add(nodeData);
            SkillTreeNodes.Add(node);

            UpdateAsset();
        }

        public void AddNodeToGraph(SkillTreeNodeData node, bool registerToObject = true)
        {
            SkillTreeEditorNode newNode = new SkillTreeEditorNode(node, this);
            newNode.styleSheets.Add(Resources.Load<StyleSheet>($"StyleSheets/SkillTreeEditorNode"));
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
            if (this.GetSkillTreeNodeDataIndex(node.ID) is not { } nodeData) return;

            nodeData.SetPosition(node.layout);
            UpdateAsset();
        }

        public void UpdateAsset()
        {
            _serializedObject.Update();
            EditorUtility.SetDirty(CurrentSkillTreeAsset);
        }
    }
}
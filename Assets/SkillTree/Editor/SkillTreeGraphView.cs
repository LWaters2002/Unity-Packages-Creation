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

        private SkillTreeEditorNodeInspector _nodeInspector;

        public SkillTreeGraphView(SerializedObject serializedObject, SkillTreeEditorWindow editorWindow)
        {
            NodeLookup = new Dictionary<string, SkillTreeEditorNode>();
            SkillTreeNodes = new List<SkillTreeEditorNode>();
            _serializedObject = serializedObject;
            EditorWindow = editorWindow;
            CurrentSkillTreeAsset = serializedObject.targetObject as SkillTreeAsset;

            _nodeInspector = new SkillTreeEditorNodeInspector(this);
            Add(_nodeInspector);
            _nodeInspector.BringToFront();

            LoadStyleSheets();
            SetupBackground();
            AddManipulators();
        }

        protected override void HandleEventBubbleUp(EventBase evt)
        {
            if (evt is ExecuteCommandEvent commandEvent && commandEvent.commandName == "SoftDelete")
            {
                foreach (var selected in selection)
                {
                    if (selected is not SkillTreeEditorNode node) continue;
                    
                    RemoveNode(node);
                    evt.StopPropagation();
                }
            }
            
            base.HandleEventBubbleUp(evt);
        }
        
        private void RemoveNode(SkillTreeEditorNode node)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Removed Node");
        
            SkillTreeNodes.Remove(node);
            if (this.GetSkillTreeNodeDataIndex(node.ID, out int index))
            {
                CurrentSkillTreeAsset.Nodes.RemoveAt(index);
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
            GridBackground background = new GridBackground();
            background.name = "Grid";

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
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create new skill node", AddNodeAction);
                evt.menu.AppendAction("Force Refresh", (_) => this.Refresh());
            }));
        }

        public void TransitionCreated(SkillTreeEditorNodeTransition transition, bool registerObject = true)
        {
            transition.SendToBack();
            
            if (registerObject)
            {
                Undo.RecordObject(_serializedObject.targetObject, "Added Transition");

                SkillTreeEditorNode startTarget = (SkillTreeEditorNode)transition.StartTarget;
                SkillTreeEditorNode endTarget = (SkillTreeEditorNode)transition.EndTarget;

                if (this.GetSkillTreeNodeDataIndex(endTarget.ID, out int index))
                    CurrentSkillTreeAsset.Nodes[index].ParentGuids.Add(startTarget.ID);

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

                if (this.GetSkillTreeNodeDataIndex(endTarget.ID, out int index))
                    CurrentSkillTreeAsset.Nodes[index].ParentGuids.Add(startTarget.ID);

                UpdateAsset();
            }

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
            UpdateAsset();
        }

        public void AddNodeToGraph(SkillTreeNodeData node, bool registerToObject = true)
        {
            SkillTreeEditorNode newNode = new SkillTreeEditorNode(node, this);
            newNode.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SkillTree/Editor/USS/Node.uss"));
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
            if (!this.GetSkillTreeNodeDataIndex(node.ID, out int index)) return;
        
            CurrentSkillTreeAsset.Nodes[index].SetPosition(node.layout);
            UpdateAsset();
        }

        public void UpdateAsset()
        {
            _serializedObject.Update();
            EditorUtility.SetDirty(CurrentSkillTreeAsset);
        }
    }
}
using System.Collections.Generic;
using SkillTree.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeGraphView : GraphView
    {
        private SerializedObject _serializedObject;
        private SkillTreeAsset _skillTreeAsset;

        public SkillTreeEditorWindow EditorWindow { get; private set; }
        public Dictionary<string, SkillTreeEditorNode> NodeLookup { get; private set; }
        public List<SkillTreeEditorNode> SkillTreeNodes { get; private set; }

        public SkillTreeGraphView(SerializedObject serializedObject, SkillTreeEditorWindow editorWindow)
        {
            NodeLookup = new Dictionary<string, SkillTreeEditorNode>();
            SkillTreeNodes = new List<SkillTreeEditorNode>();
            _serializedObject = serializedObject;
            EditorWindow = editorWindow;
            _skillTreeAsset = serializedObject.targetObject as SkillTreeAsset;

            LoadStyleSheets();
            SetupBackground();
            AddManipulators();

            contentViewContainer.BringToFront();
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

            this.AddManipulator(new TransitionHandleManipulator() // Needs to be first order matters
                {
                    target = this
                }
            );
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create new skill node", AddNodeAction);
                evt.menu.AppendAction("Create new transition", AddTransitionAction, CanCreateTransition());
            }));
        }

        private void AddNodeAction(DropdownMenuAction dropdownMenuAction)
        {
            Vector2 mousePos = dropdownMenuAction.eventInfo.mousePosition;

            mousePos = contentViewContainer.WorldToLocal(mousePos);

            SkillTreeNodeData node = new SkillTreeNodeData();
            node.SetPosition(new Rect(mousePos.x, mousePos.y, 64, 64));

            node.SetProperties(new NodeProperties()
            {
                Title = "New Skill Node",
                Description = "New Skill Node Description",
                Icon = null,
                Cost = 1
            });

            AddNodeToGraph(node);
        }

        private DropdownMenuAction.Status CanCreateTransition()
        {
            if (selection.Count > 1)
            {
                return DropdownMenuAction.Status.Disabled;
            }
        
            foreach (ISelectable selectable in selection)
            {
                if (selectable is SkillTreeEditorNode node)
                {
                    return DropdownMenuAction.Status.Normal;
                }
            }

            return DropdownMenuAction.Status.Hidden;
        }   

        private void AddTransitionAction(DropdownMenuAction action)
        {
            SkillTreeEditorNodeTransition transition = new SkillTreeEditorNodeTransition(Vector2.zero, Vector2.one * 200.0f);
            AddElement(transition);
        }

        private void RegisterNodeData(SkillTreeNodeData nodeData, SkillTreeEditorNode node)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Added Node");

            _skillTreeAsset.Nodes.Add(nodeData);
            _serializedObject.Update();

            NodeLookup.Add(nodeData.ID, node);
            SkillTreeNodes.Add(node);
        }

        public void AddNodeToGraph(SkillTreeNodeData node)
        {
            SkillTreeEditorNode newNode = new SkillTreeEditorNode(this);
            newNode.title = node.Properties.Title;
            newNode.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SkillTree/Editor/USS/Node.uss"));
            node.typeName = node.GetType().AssemblyQualifiedName;
            newNode.SetPosition(node.Position);
            newNode.BringToFront();

            RegisterNodeData(node, newNode);
            AddElement(newNode);
        }
    }
}
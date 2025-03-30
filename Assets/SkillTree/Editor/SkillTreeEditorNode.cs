using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace SkillTree.Editor
{
    public class SkillTreeEditorNode : GraphElement
    {
        VisualElement m_TitleContainer;
        Label m_TitleLabel;

        public SkillTreeEditorNode(GraphView graphView = null) : base()
        {
            capabilities |= Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable |
                            Capabilities.Selectable;
            AddToClassList("skill-node");

            m_TitleLabel = new Label();

            CreateContainers(graphView);
            m_TitleContainer.Add(m_TitleLabel);
        }

        private void CreateContainers(GraphView graphView = null)
        {
            m_TitleContainer = new VisualElement();
            m_TitleContainer.AddToClassList("title-container");
        }

        public override string title
        {
            get { return (m_TitleLabel != null) ? m_TitleLabel.text : string.Empty; }
            set
            {
                if (m_TitleLabel != null)
                {
                    m_TitleLabel.text = value;
                }
            }
        }
    }

    public class TransitionHandleManipulator : MouseManipulator
    {
        private SkillTreeEditorNodeTransition currentTransition = null;
        private SkillTreeEditorNode startNode = null;

        protected override void RegisterCallbacksOnTarget()
        {
            if (target == null) return;
            
            target.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown));
            target.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp));
            target.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (target == null) return;

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (currentTransition == null || evt.target is not SkillTreeEditorNode editorNode) return;

            if (evt.target is SkillTreeEditorNode endEditorNode && editorNode != startNode)
            {
                currentTransition.Init(editorNode, endEditorNode);
                Clear();
                evt.StopPropagation();
                return;
            }

            target.Remove(currentTransition);
            Clear();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (currentTransition == null) return;

            evt.StopPropagation();

            Vector2 pos = target.WorldToLocal(evt.mousePosition);
            Vector2 offset = new Vector2(16, 16);
            currentTransition.End = pos - offset / 2;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount != 2) return;
            if (evt.target is not SkillTreeEditorNode editorNode) return;
            
            startNode = editorNode;
            if (currentTransition != null)
            {
                target.Remove(currentTransition);
                evt.StopPropagation();
                return;
            }

            Vector2 startPos = new Vector2(startNode.layout.x, startNode.layout.y) + new Vector2(startNode.resolvedStyle.width, startNode.resolvedStyle.height) / 2;
            Vector2 endPos = target.WorldToLocal(evt.mousePosition);
            
            currentTransition = new SkillTreeEditorNodeTransition(startPos, endPos);
            target.Add(currentTransition);
            evt.StopPropagation();
        }

        private void Clear()
        {
            startNode = null;
            currentTransition = null;
        }
    }
}
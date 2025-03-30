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

        private TransitionHandleManipulator _transitionHandleManipulator;

        public SkillTreeEditorNode(GraphView graphView = null) : base()
        {
            capabilities |= Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable |
                            Capabilities.Selectable;
            AddToClassList("skill-node");

            m_TitleLabel = new Label();

            CreateContainers(graphView);
            m_TitleContainer.Add(m_TitleLabel);

            _transitionHandleManipulator = new TransitionHandleManipulator(this, graphView)
            {
                target = this
            };
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
        private SkillTreeEditorNodeTransition _currentTransition = null;
        private SkillTreeEditorNode _startNode = null;

        private GraphView _graphView = null;

        public TransitionHandleManipulator(SkillTreeEditorNode startNode = null, GraphView graphView = null)
        {
            _startNode = startNode;
            _graphView = graphView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target == null) return;

            target.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown));

            _graphView?.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp));
            _graphView?.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (target == null) return;

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);

            _graphView?.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            _graphView?.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (_currentTransition == null || _startNode == null) return;

            if (evt.target is SkillTreeEditorNode endEditorNode && endEditorNode != _startNode)
            {
                _currentTransition.Init(_startNode, endEditorNode);
                if (_graphView is SkillTreeGraphView skillTreeGraphView)
                {
                    skillTreeGraphView.NodeTransitions.Add(_currentTransition);
                }
                _currentTransition = null;
                evt.StopPropagation();
                return;
            }

            Clear();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_currentTransition == null) return;

            evt.StopImmediatePropagation();

            Vector2 pos = evt.mousePosition;
            Vector2 offset = Vector2.one * 8.0f;
            _currentTransition.End = _currentTransition.WorldToLocal(pos) - offset;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount != 2) return;

            if (_currentTransition != null)
            {
                Clear();
                evt.StopPropagation();
                return;
            }

            Vector2 startPos = new Vector2(_startNode.layout.x, _startNode.layout.y) +
                new Vector2(_startNode.resolvedStyle.width, _startNode.resolvedStyle.height) / 2 - Vector2.one * 8.0f;
            Vector2 endPos = evt.mousePosition;

            _currentTransition = new SkillTreeEditorNodeTransition(startPos, endPos);
            _graphView?.AddElement(_currentTransition);
            evt.StopPropagation();
        }

        private void Clear()
        {
            if (_currentTransition == null) return;

            _graphView?.RemoveElement(_currentTransition);
            _currentTransition = null;
        }
    }
}
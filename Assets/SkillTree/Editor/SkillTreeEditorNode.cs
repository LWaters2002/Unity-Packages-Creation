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

        public string ID { get; private set; }
        private TransitionHandleManipulator _transitionHandleManipulator;

        public SkillTreeEditorNode(string ID, GraphView graphView = null) : base()
        {
            this.ID = ID;

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
}
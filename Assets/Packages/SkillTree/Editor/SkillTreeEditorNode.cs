using SkillTree.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeEditorNode : GraphElement
    {
        VisualElement m_TitleContainer;
        Label m_TitleLabel;

        public string ID { get; private set; }
        private TransitionHandleManipulator _transitionHandleManipulator;

        private Image _imageIcon;
        
        private SkillTreeGraphView _graphView;
        
        public SkillTreeEditorNode(SkillTreeNodeData nodeData, SkillTreeGraphView graphView) : base()
        {
            _graphView = graphView;

            Init();
            
            ID = nodeData.ID;
            _imageIcon.sprite = nodeData.Properties.icon;
        }

        private void Init()
        {
            capabilities |= Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable |
                            Capabilities.Selectable;
            AddToClassList("skill-node");

            m_TitleLabel = new Label();
            
            _imageIcon = new Image();
            _imageIcon.AddToClassList("skillIcon");
            Add(_imageIcon);
            _imageIcon.StretchToParentSize();
            _imageIcon.pickingMode = PickingMode.Ignore;
            
            CreateContainers(_graphView);
            m_TitleContainer.Add(m_TitleLabel);

            _transitionHandleManipulator = new TransitionHandleManipulator(this, _graphView)
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
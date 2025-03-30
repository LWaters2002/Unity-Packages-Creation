using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeEditorNodeTransition : GraphElement
    {
        private Vector2 _end;
        private Vector2 _start;

        private VisualElement m_startTarget;
        private VisualElement m_endTarget;

        public Vector2 Start
        {
            get => _start;
            set
            {
                _start = value;
                MarkDirtyRepaint();
            }
        }

        public Vector2 End
        {
            get => _end;
            set
            {
                _end = value;
                MarkDirtyRepaint();
            }
        }

        public SkillTreeEditorNodeTransition(Vector2 start, Vector2 end)
        {
            AddToClassList("skillTreeEditorNodeTransition");

            Start = start;
            End = end;

            // Add a callback for custom drawing
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Init(VisualElement startTarget, VisualElement endTarget)
        {
            if (startTarget == null || endTarget == null) return;
            
            m_startTarget = startTarget;
            m_endTarget = endTarget;
            
            m_startTarget.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                UpdatePositions();
            });
            
            m_endTarget.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                UpdatePositions();
            });
        }

        public void UpdatePositions()
        {
            if (m_startTarget == null || m_endTarget == null) return;

            Vector2 handleOffset = new Vector2(m_endTarget.resolvedStyle.width, m_endTarget.resolvedStyle.height) - Vector2.one * 16.0f ;
            Vector2 targetPos = new Vector2(m_startTarget.layout.x, m_startTarget.layout.y);
            Vector2 nodePos = new Vector2(m_endTarget.layout.x, m_endTarget.layout.y);
            
            End = nodePos - targetPos +  handleOffset/2;
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            Vector2 offset = Vector2.zero;
            if (parent != null)
            {
                offset = new Vector2(parent.resolvedStyle.width, parent.resolvedStyle.height);
            }
            
            // Set line thickness and color
            painter.strokeColor = Color.black;
            painter.lineWidth = 2.0f;

            Vector2 startPoint = Start + offset / 2;
            
            // Draw the line for the arrow
            painter.BeginPath();
            painter.MoveTo(startPoint);
            painter.LineTo(End);
            painter.Stroke();

            // Draw arrowhead (triangle pointing to "end")
            Vector2 direction = (End - startPoint).normalized;
            Vector2 arrowTip = End;
            Vector2 arrowLeft = End - direction * 10 + new Vector2(-direction.y, direction.x) * 5;
            Vector2 arrowRight = End - direction * 10 + new Vector2(direction.y, -direction.x) * 5;

            painter.BeginPath();
            painter.MoveTo(arrowTip);
            painter.LineTo(arrowLeft);
            painter.LineTo(arrowRight);
            painter.LineTo(arrowTip);
            painter.Fill();
        }

    }
}
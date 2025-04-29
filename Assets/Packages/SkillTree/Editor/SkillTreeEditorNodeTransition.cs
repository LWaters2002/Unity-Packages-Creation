using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeEditorNodeTransition : GraphElement
    {
        private Vector2 _end;
        private Vector2 _start;

        public VisualElement StartTarget;
        public VisualElement EndTarget;

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


        public SkillTreeEditorNodeTransition()
        {
            style.position = Position.Absolute;
            AddToClassList("skillTreeEditorNodeTransition");

            // Add a callback for custom drawing
            generateVisualContent += OnGenerateVisualContent;
        }

        public SkillTreeEditorNodeTransition(Vector2 start, Vector2 end)
        {
            style.position = Position.Absolute;

            AddToClassList("skillTreeEditorNodeTransition");

            Start = start;
            End = end;

            // Add a callback for custom drawing
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Init(VisualElement startTarget, VisualElement endTarget)
        {
            if (startTarget == null || endTarget == null) return;

            StartTarget = startTarget;
            EndTarget = endTarget;

            StartTarget.RegisterCallback<GeometryChangedEvent>(evt => { UpdatePositions(); });

            EndTarget.RegisterCallback<GeometryChangedEvent>(evt => { UpdatePositions(); });

            UpdatePositions();
        }

        public void UpdatePositions()
        {
            if (StartTarget == null || EndTarget == null) return;

            Vector2 handleOffset = new Vector2(EndTarget.resolvedStyle.width, EndTarget.resolvedStyle.height) -
                                   Vector2.one * 16.0f;
            Vector2 startPos = new Vector2(StartTarget.layout.x, StartTarget.layout.y);
            Vector2 endPos = new Vector2(EndTarget.layout.x, EndTarget.layout.y);

            Start = startPos + handleOffset / 2;
            End = endPos + handleOffset / 2;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            Vector2 offset = Vector2.zero;

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
            Vector2 arrowTip = (Start + (End - Start) / 2);
            Vector2 direction = (arrowTip - startPoint).normalized;
            Vector2 arrowLeft = arrowTip - direction * 10 + new Vector2(-direction.y, direction.x) * 5;
            Vector2 arrowRight = arrowTip - direction * 10 + new Vector2(direction.y, -direction.x) * 5;

            painter.BeginPath();
            painter.MoveTo(arrowTip);
            painter.LineTo(arrowLeft);
            painter.LineTo(arrowRight);
            painter.LineTo(arrowTip);
            painter.Fill();
        }
    }
}
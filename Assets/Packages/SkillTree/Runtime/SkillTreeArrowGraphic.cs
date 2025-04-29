using UnityEngine;
using UnityEngine.UI;

public class SkillTreeArrowGraphic : MaskableGraphic
{
    [SerializeField, Range(0, 1)] private float arrowPosition = 0.5f;
    [SerializeField, Range(0, 5)] private float arrowWidth = 1.0f;
    [SerializeField] private float lineWidth = 5f;
    [SerializeField] private float arrowSize = 20f;
    [SerializeField] private Vector2 startPoint = new Vector2(0, 0);
    [SerializeField] private Vector2 endPoint = new Vector2(100, 100);

    public float ArrowPosition
    {
        get => arrowPosition;
        set { arrowPosition = Mathf.Clamp01(value); SetVerticesDirty(); }
    }
    
    public float ArrowWidth
    {
        get => arrowWidth;
        set { arrowWidth = value; SetVerticesDirty(); }
    }

    public float LineWidth
    {
        get => lineWidth;
        set { lineWidth = value; SetVerticesDirty(); }
    }

    public float ArrowSize
    {
        get => arrowSize;
        set { arrowSize = value; SetVerticesDirty(); }
    }

    public Vector2 StartPoint
    {
        get => startPoint;
        set { startPoint = value; SetVerticesDirty(); }
    }

    public Vector2 EndPoint
    {
        get => endPoint;
        set { endPoint = value; SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Calculate the arrow position along the line
        Vector2 arrowPoint = Vector2.Lerp(startPoint, endPoint, arrowPosition);

        // Draw the line segments
        DrawLineSegment(vh, startPoint, endPoint);
        
        // Draw the arrowhead
        DrawArrowHead(vh, arrowPoint, endPoint);
    }

    private void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * lineWidth * 0.5f;

        UIVertex[] vertices = new UIVertex[4];
        
        vertices[0].position = start - perpendicular;
        vertices[1].position = start + perpendicular;
        vertices[2].position = end + perpendicular;
        vertices[3].position = end - perpendicular;

        for (int i = 0; i < 4; i++)
        {
            vertices[i].color = color;
            vertices[i].uv0 = Vector2.zero;
        }

        vh.AddVert(vertices[0]);
        vh.AddVert(vertices[1]);
        vh.AddVert(vertices[2]);
        vh.AddVert(vertices[3]);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    private void DrawArrowHead(VertexHelper vh, Vector2 arrowBase, Vector2 lineEnd)
    {
        Vector2 direction = (lineEnd - arrowBase).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * arrowWidth;

        Vector2 arrowTip = arrowBase + direction * arrowSize;
        Vector2 arrowLeft = arrowBase - direction * (arrowSize * 0.5f) + perpendicular * (arrowSize * 0.5f);
        Vector2 arrowRight = arrowBase - direction * (arrowSize * 0.5f) - perpendicular * (arrowSize * 0.5f);

        UIVertex[] vertices = new UIVertex[3];
        
        vertices[0].position = arrowTip;
        vertices[1].position = arrowLeft;
        vertices[2].position = arrowRight;

        for (int i = 0; i < 3; i++)
        {
            vertices[i].color = color;
            vertices[i].uv0 = Vector2.zero;
        }

        vh.AddVert(vertices[0]);
        vh.AddVert(vertices[1]);
        vh.AddVert(vertices[2]);

        int vert = vh.currentVertCount; 
        vh.AddTriangle(vert - 1, vert -2 , vert - 3);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
    }
}

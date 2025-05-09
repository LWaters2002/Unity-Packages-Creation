using SkillTree.Runtime;
using SkillTree.Runtime.UI;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class SkillTreeArrowGraphic : MonoBehaviour
{
    public UnityEvent OnConnectedSkillLocked;
    public UnityEvent OnConnectedSkillUnlocked;
    
    [SerializeField] private bool shouldShowArrow = true;
    [SerializeField] private RectTransform lineTransform;
    [SerializeField] private RectTransform slidingImageTransform;
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;

    private void SetStartPoint(Vector2 point) => startPoint = point;
    private void SetEndPoint(Vector2 point) => endPoint = point;

    public void Init(SkillTreeNode startNode, SkillTreeNode parentNode)
    {
        SetStartPoint(startNode.transform.localPosition);
        SetEndPoint(parentNode.transform.localPosition);

        UpdateGraphics();

        slidingImageTransform.gameObject.SetActive(shouldShowArrow);
    
        if (parentNode.GetIsUnlocked())
        {
            ConnectedSkillUnlocked();
        }
        else
        {
            ConnectedSkillLocked();
        }
        
    }

    public void ConnectedSkillLocked()
    {
        OnConnectedSkillLocked.Invoke();
    }

    public void ConnectedSkillUnlocked()
    {
        OnConnectedSkillUnlocked.Invoke();
    }

    void UpdateGraphics()
    {
        if (!lineTransform) return;
        if (!slidingImageTransform) return;

        Vector2 startPos = startPoint;
        Vector2 endPos = endPoint;
        Vector2 midpoint = (startPos + endPos) / 2f;

        lineTransform.anchoredPosition = midpoint;
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        lineTransform.sizeDelta = new Vector2(distance, lineTransform.sizeDelta.y);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        slidingImageTransform.anchoredPosition = Vector2.Lerp(startPoint, endPoint, .5f);
        slidingImageTransform.rotation = lineTransform.rotation;
    }
}
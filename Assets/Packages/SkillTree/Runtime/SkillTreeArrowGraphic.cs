using System;
using System.Collections.Generic;
using SkillTree.Runtime;
using SkillTree.Runtime.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class SkillTreeArrowGraphic : MonoBehaviour
{
    [SerializeField] private bool shouldShowArrow = true;
    [SerializeField] private RectTransform lineTransform;
    [SerializeField] private RectTransform slidingImageTransform;
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;

    [SerializeField] private Color validParentColor;
    [SerializeField] private Color invalidParentColor;
    
    private void SetStartPoint(Vector2 point) => startPoint = point;
    private void SetEndPoint(Vector2 point) => endPoint = point;

    private SkillTreeNode startNode;
    private SkillTreeNode endNode;

    private System.Action<bool> UpdateColourDelegate;
    
    public void Init(SkillTreeNode startNode, SkillTreeNode endNode)
    {
        this.startNode = startNode;
        this.endNode = endNode;
        
        slidingImageTransform.gameObject.SetActive(shouldShowArrow);
        
        SetStartPoint(startNode.transform.localPosition);
        SetEndPoint(endNode.transform.localPosition);

        UpdateGraphics();

        UpdateColourDelegate = _ => UpdateColourFromUnlock();
        endNode.OnStateUpdated += UpdateColourDelegate;
        UpdateColourFromUnlock();
    }
    
    public void OnDestroy()
    {
        endNode.OnStateUpdated -= UpdateColourDelegate;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateColourFromUnlock()
    {
        Image[] childImages = GetComponentsInChildren<Image>();

        bool isValid = startNode.GetIsUnlocked() || endNode.GetIsUnlocked();
        foreach (Image childImage in childImages)
            childImage.color = isValid ? validParentColor : invalidParentColor;
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
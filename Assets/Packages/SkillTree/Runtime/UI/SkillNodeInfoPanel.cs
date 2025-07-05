using System;
using SkillTree.Runtime;
using SkillTree.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class SkillNodeInfoPanel : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private SkillTreeNode _currentNode; 
    private CanvasGroup _canvasGroup;
    
    void Awake()
    {
        SkillTreeEventBus<ESkillNodeSelected>.RegisterCallback(OnSkillSelected);
        SkillTreeEventBus<ESkillNodeLevelUp>.RegisterCallback(OnNodeLevelUp);

        button.onClick.AddListener(OnClick);
        
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    private void OnClick()
    {
        if (_currentNode == null) return;
        
        _currentNode.TryLevelUp();
    }

    void OnDestroy()
    {
        SkillTreeEventBus<ESkillNodeSelected>.UnregisterCallback(OnSkillSelected);
        SkillTreeEventBus<ESkillNodeLevelUp>.UnregisterCallback(OnNodeLevelUp);
    }

    private void OnNodeLevelUp(ESkillNodeLevelUp obj)
    {
        // Add logic to update skill level and stats if leveled up skills is the same as selected.
    }

    private void OnSkillSelected(ESkillNodeSelected skillNodeEvent)
    {
        if (skillNodeEvent.Node is not { } node) return;
        
        _canvasGroup.alpha = 1;
        button.interactable = true;

        _currentNode = node;
        titleText.text = node.data.Properties.title;
        descriptionText.text = node.data.Properties.description;
    }
}

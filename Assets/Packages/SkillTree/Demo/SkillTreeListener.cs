using SkillTree.Runtime;
using SkillTree.Runtime.UI;
using UnityEngine;

public class SkillTreeListener : MonoBehaviour
{
    void Awake()
    {
        SkillTreeEventBus<ESkillNodeLevelUp>.RegisterCallback(OnSkillLevelUp);
    }

    private void OnSkillLevelUp(ESkillNodeLevelUp skillNode)
    {
        Debug.Log($"Skill [{skillNode.identifier}] Leveled up to Level {skillNode.newLevel}");
    }
}
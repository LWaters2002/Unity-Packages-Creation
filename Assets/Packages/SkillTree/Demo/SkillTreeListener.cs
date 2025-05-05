using SkillTree.Runtime;
using SkillTree.Runtime.UI;
using UnityEngine;

public class SkillTreeListener : MonoBehaviour
{
    void Awake()
    {
        SkillTreeEventBus<UISkillLevelUp>.RegisterCallback(OnSkillLevelUp);
    }

    private void OnSkillLevelUp(UISkillLevelUp skill)
    {
        Debug.Log($"Skill [{skill.identifier}] Leveled up to Level {skill.newLevel}");
    }
}
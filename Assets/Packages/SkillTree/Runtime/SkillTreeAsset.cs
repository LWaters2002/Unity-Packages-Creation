using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Runtime
{
    [CreateAssetMenu(menuName = "Skill Tree/Skill Tree Asset")]
    public class SkillTreeAsset : ScriptableObject
    {
        [SerializeReference]
        private List<SkillTreeNodeData> _nodes = new();

        public List<SkillTreeNodeData> Nodes => _nodes;
    }
}
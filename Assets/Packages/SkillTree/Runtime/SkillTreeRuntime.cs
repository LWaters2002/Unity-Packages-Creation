using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Runtime
{
    public class SkillTreeRuntime : MonoBehaviour
    {
        public SkillTreeAsset skillTree;

        private List<SkillTreeNodeData> _nodes;

        public void Awake()
        {
            _nodes = skillTree.Nodes;
        }
    }
}
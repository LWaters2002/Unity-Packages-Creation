using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillTree.Runtime
{
    [CreateAssetMenu(menuName = "Skill Tree/Skill Tree Asset")]
    public class SkillTreeAsset : ScriptableObject
    {
        [SerializeReference] private List<SkillTreeNodeData> _nodes = new();

        public List<SkillTreeNodeData> Nodes => _nodes;

        public bool GetNodeData(string id, out SkillTreeNodeData data)
        {
            data = null;

            foreach (var node in _nodes.Where(node => node.ID == id))
            {
                data = node;            
                return true;
            }
            
            return false;
        }
        
        private void OnValidate()
        {
            foreach (SkillTreeNodeData node in _nodes)
            {
                for (var index = node.ParentGuids.Count - 1; index >= 0; --index)
                {
                    string parentID = node.ParentGuids[index];
                    if (CheckValidGuid(parentID) == false)
                        node.ParentGuids.RemoveAt(index);
                }
            }
        }

        private bool CheckValidGuid(string inGuid)
        {
            return _nodes.Any(node => inGuid == node.ID);
        }
    }
}
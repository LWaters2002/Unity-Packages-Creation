using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Runtime
{
    [Serializable]
    public class SkillTreeNodeData
    {
        [SerializeField] private string _guid = Guid.NewGuid().ToString();
        [SerializeField] private Rect _position;

        public string ID
        {
            get => _guid;
            set => _guid = value;
        }

        public Rect Position => _position;

        [field: SerializeField] public NodeProperties Properties { get; private set; }
        [field: SerializeField] public List<string> ParentGuids { get; private set; } = new();

        public void SetPosition(Rect position)
        {
            _position = position;
        }

        public void SetProperties(NodeProperties properties)
        {
            Properties = properties;
        }

        public SkillTreeNodeData CreateShallowCopy()
        {
            SkillTreeNodeData shallowCopy = new()
            {
                Properties = Properties,
                ParentGuids = new List<string>(ParentGuids),
                _position = Position
            };

            return shallowCopy;
        }
    }

    [Serializable]
    public struct NodeProperties
    {
        public string identifier;
        public string title;
        public string description;
        public Sprite icon;
        public int cost;
        public int maxLevel;
        public bool requiresFullyLevelledParentsToUnlock;
        public bool requiresAllParentNodesToUnlock;
    }

    [Serializable]
    public class RuntimeNodeData
    {
        public bool isUnlocked = true;
        public int level;
    }
}
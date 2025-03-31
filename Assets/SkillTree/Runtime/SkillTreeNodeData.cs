using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Runtime
{
    [Serializable]
    public class SkillTreeNodeData
    {
        [SerializeField]
        private string _guid = "";

        [SerializeField]
        private Rect _position;

        public string typeName;

        public string ID
        {
            get => _guid;
            set => _guid = value;
        }

        public Rect Position => _position;
        
        
        [field : SerializeField]
        public NodeProperties Properties { get; private set; }
        
        [field : SerializeField]
        public List<string> ParentGuids { get; private set; } = new List<string>();

        public SkillTreeNodeData()
        {
            NewGUID();
        }

        private void NewGUID()
        {
            _guid = Guid.NewGuid().ToString();
        }

        public void SetPosition(Rect position)
        {
            _position = position;
        }

        public void SetProperties(NodeProperties properties)
        {
            Properties = properties;
        }
    }

    [Serializable]
    public struct NodeProperties
    {
        public string Title;
        public string Description;
        public Sprite Icon;
        public int Cost;
        public int MaxLevel;
        public bool RequiresFullyLevelledParentsToUnlock;
        public bool RequiresBothParentNodesToUnlock;
    }
}
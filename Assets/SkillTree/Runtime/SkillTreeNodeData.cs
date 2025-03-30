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

        public string ID => _guid;
        public Rect Position => _position;

        public NodeProperties Properties { get; private set; }
        public List<string> ParentGuids { get; private set; }

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
    }
}
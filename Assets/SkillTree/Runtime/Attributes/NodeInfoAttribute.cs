
using System;

namespace SkillTree.Runtime.Attributes
{
    public class NodeInfoAttribute : Attribute
    {
        private string _nodeTitle;
        private string _menuTitle;

        public string title => _nodeTitle;
        public string menuTitle => _menuTitle;

        public NodeInfoAttribute(string nodeTitle, string menuTitle)
        {
            _nodeTitle = nodeTitle;
            _menuTitle = menuTitle;
        }
    }
}
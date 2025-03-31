using SkillTree.Runtime;
using UnityEngine.UIElements;
using System.Linq;

namespace SkillTree.Editor
{
    public static class SkillTreeEditorStatics
    {
        public static void Refresh(this SkillTreeGraphView graphView)
        {
            ClearGraph(graphView);
            
            foreach (SkillTreeNodeData node in graphView.CurrentSkillTreeAsset.Nodes)
            {
                graphView.AddNodeToGraph(node, false);
            }
            
            foreach (SkillTreeNodeData node in graphView.CurrentSkillTreeAsset.Nodes)
            {
                foreach (string guid in node.ParentGuids)
                {
                    if (!graphView.NodeLookup.ContainsKey(guid) || !graphView.NodeLookup.ContainsKey(node.ID)) continue;
                    
                    SkillTreeEditorNode endNode = graphView.NodeLookup[node.ID];
                    SkillTreeEditorNode startNode = graphView.NodeLookup[guid];

                    if (startNode == null || endNode == null) continue;
                    
                    SkillTreeEditorNodeTransition transition = new SkillTreeEditorNodeTransition();
                    graphView.AddElement(transition);
                    transition.Init(startNode, endNode);
                    
                    graphView.TransitionCreated(transition, false);
                }   
            }
        }

        public static void ClearGraph(this SkillTreeGraphView graphView)
        {
            graphView.NodeLookup.Clear();
            graphView.NodeTransitions.Clear();
            graphView.SkillTreeNodes.Clear();

            foreach (VisualElement child in graphView.contentViewContainer.Children())
            {
                if (child.childCount == 0) continue;
                child.Clear();
            }
        }
        public static bool GetSkillTreeNodeDataIndex(this SkillTreeGraphView graphView, string guid, out int foundIndex)
        {
            foundIndex = -1;

            if (graphView == null) return false;
            if (graphView.CurrentSkillTreeAsset == null) return false;

            foundIndex = graphView.CurrentSkillTreeAsset.Nodes.FindIndex(x => x.ID == guid);
            return foundIndex != -1;
        }
    }
}
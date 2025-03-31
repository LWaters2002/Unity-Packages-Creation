using SkillTree.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

namespace SkillTree.Editor
{
    public static class SkillTreeEditorStatics
    {
        public static void Refresh(this SkillTreeGraphView graphView)
        {
            graphView.NodeLookup.Clear();
            graphView.NodeTransitions.Clear();
            graphView.SkillTreeNodes.Clear();
            
            foreach (VisualElement child in graphView.Children())
            {
                if (child is SkillTreeEditorNode or SkillTreeEditorNodeTransition)
                {
                    graphView.RemoveElement((GraphElement)child);
                }
            }

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
    }
}
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillTree.Runtime.UI
{
    public class SkillTreeNode : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Image icon;

        [SerializeField] private Image border;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI levelText;

        [SerializeField] private SkillTreeNodeData data;
        [SerializeField] private RuntimeNodeData runtimeData;
        private SkillTreeViewer _skillTreeViewer;

        public void Init(SkillTreeViewer skillTreeViewer, SkillTreeNodeData inData)
        {
            data = inData;
            _skillTreeViewer = skillTreeViewer;
            _skillTreeViewer.OnNodeStateChanged += NodesUpdated;

            Vector2 pos = new Vector2(inData.Position.x, -inData.Position.y);
            transform.position = pos;
            icon.sprite = inData.Properties.icon;

            runtimeData = new RuntimeNodeData
            {
                isUnlocked = true,
                level = 0
            };
            
            button.onClick.AddListener(OnButtonClick);
            UpdateLevelText();
        }

        private void OnButtonClick()
        {
            LevelUp();
        }

        private void LevelUp()
        {
            if (runtimeData.level >= data.Properties.maxLevel)
                return;
            
            runtimeData.level++;
            
            UpdateLevelText();
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
        }

        private void UpdateLevelText() => levelText.text = $"{runtimeData.level}/{data.Properties.maxLevel}";

        private void NodesUpdated(string updatedGuid)
        {
            bool relatedGuidDirtied = data.ParentGuids.Any(guid => guid == updatedGuid);
            if (relatedGuidDirtied == false) return;
            
            UpdateUnlockState();
        }

        public void UpdateUnlockState()
        {
            int parentsMatchingCondition = 0;

            foreach (string guid in data.ParentGuids)
            {
                SkillTreeNode node = _skillTreeViewer.Nodes[guid];
                node.UpdateUnlockState();

                int targetLevel = data.Properties.requiresFullyLevelledParentsToUnlock
                    ? node.GetMaxLevel()
                    : 1;

                if (node.GetLevel() >= targetLevel)
                    parentsMatchingCondition++;
            }

            bool canUnlock = parentsMatchingCondition == data.ParentGuids.Count;

            if (parentsMatchingCondition > 0 && data.Properties.requiresAllParentNodesToUnlock == false)
                canUnlock = true;

            SetUnlock(canUnlock);
        }

        private void SetUnlock(bool setUnlock)
        {
            if (GetIsUnlocked() == setUnlock)
            {
                return;
            }

            button.interactable = setUnlock;
            runtimeData.isUnlocked = setUnlock;
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
        }

        public int GetLevel() => runtimeData.level;
        public int GetMaxLevel() => data.Properties.maxLevel;
        public bool GetIsUnlocked() => runtimeData.isUnlocked;
    }
}
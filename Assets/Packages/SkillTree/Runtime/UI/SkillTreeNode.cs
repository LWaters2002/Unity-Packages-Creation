using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace SkillTree.Runtime.UI
{
    public struct ESkillNodeLevelUp : ISkillTreeEvent
    {
        public string identifier;
        public int newLevel;

        public ESkillNodeLevelUp(string identifier, int newLevel)
        {
            this.identifier = identifier;
            this.newLevel = newLevel;
        }
    }

    public class SkillTreeNode : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Image icon;

        [SerializeField] private Image border;
        [SerializeField] private TextMeshProUGUI levelText;

        [SerializeField] private SkillTreeNodeData data;
        [SerializeField] private RuntimeNodeData runtimeData;

        private SkillTreeViewer _skillTreeViewer;
        private Button button;

        public System.Action<bool> OnStateUpdated;

        public void Init(SkillTreeViewer skillTreeViewer, SkillTreeNodeData inData)
        {
            data = inData;
            _skillTreeViewer = skillTreeViewer;
            _skillTreeViewer.OnNodeStateChanged += NodesUpdated;

            Vector2 pos = new Vector2(inData.Position.x, -inData.Position.y);
            transform.localPosition = pos;
            transform.localScale = Vector3.one;
            
            icon.sprite = inData.Properties.icon;

            runtimeData = new RuntimeNodeData
            {
                isUnlocked = true,
                level = 0
            };

            button = GetComponent<Button>();
            button?.onClick.AddListener(OnButtonClick);
            UpdateLevelText();
        }

        private void OnButtonClick()
        {
            bool hasEnoughSkillPoints = _skillTreeViewer.CanBuy(data.Properties.cost);
            bool notAtMaxLevel = data.Properties.maxLevel != runtimeData.level;
            
            if (hasEnoughSkillPoints && notAtMaxLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            if (runtimeData.level >= data.Properties.maxLevel)
                return;
            
            runtimeData.level++;
            
            UpdateLevelText();
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
            SkillTreeEventBus<ESkillNodeLevelUp>.Execute(new ESkillNodeLevelUp(data.Properties.identifier, runtimeData.level));
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

            if (button)
                button.interactable = setUnlock;
            
            runtimeData.isUnlocked = setUnlock;
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
            OnStateUpdated?.Invoke(setUnlock);
        }
        
        public int GetCost() => data.Properties.cost;
        public int GetLevel() => runtimeData.level;
        public int GetMaxLevel() => data.Properties.maxLevel;
        public bool GetIsUnlocked() => runtimeData.isUnlocked;
    }
}
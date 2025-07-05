using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace SkillTree.Runtime.UI
{
    public class SkillTreeNode : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [Header("References")] [SerializeField]
        private Image icon;

        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image frame;
        [SerializeField] private Image holdFillMask;

        public SkillTreeNodeData data;
        public RuntimeNodeData runtimeData;

        [Header("Cosmetics")] [SerializeField] private Color frameColor;
        [SerializeField] private Color frameSelectedColor;
        [SerializeField] private Color frameLockedColor;
        [SerializeField] private Color frameLockedSelectedColor;
        [SerializeField] private Color frameMaxedColor;

        [Header("Controls")] [SerializeField] private bool doubleClickQuickUnlockEnabled;
        [SerializeField] private float holdUnlockTime;

        private SkillTreeViewer _skillTreeViewer;
        private Button button;

        public System.Action<bool> OnStateUpdated;

        private bool isButtonDown = false;
        private float holdTime = 0.0f;

        private bool isSelected = false;

        private float lastClickedTime = 0.0f;
        private float buttonDoubleClickPeriod = 0.3f;

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
            SkillTreeEventBus<ESkillNodeSelected>.RegisterCallback(skillSelectedEvent =>
            {
                if (skillSelectedEvent.Node == this) return;

                isSelected = false;
                UpdateCosmeticState();
            });

            UpdateLevelText();
        }

        private void Update()
        {
            if (isButtonDown)
            {
                holdTime += Time.deltaTime;

                if (holdTime > buttonDoubleClickPeriod / 3.0f)
                    holdFillMask.fillAmount = holdTime / holdUnlockTime;

                if (holdTime >= holdUnlockTime)
                {
                    isButtonDown = false;
                    holdTime = 0.0f;
                    holdFillMask.fillAmount = 0.0f;

                    TryLevelUp();
                }
            }
        }

        public void TryLevelUp()
        {
            bool hasEnoughSkillPoints = _skillTreeViewer.CanBuy(data.Properties.cost);

            if (!hasEnoughSkillPoints || IsMaxLevel()) return;

            _skillTreeViewer.Buy(data.Properties.cost);
            LevelUp();
        }

        private void LevelUp()
        {
            if (runtimeData.level >= data.Properties.maxLevel)
                return;

            runtimeData.level++;

            UpdateLevelText();
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
            SkillTreeEventBus<ESkillNodeLevelUp>.Execute(new ESkillNodeLevelUp(data.Properties.identifier,
                runtimeData.level));

            UpdateCosmeticState();
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

            runtimeData.isUnlocked = setUnlock;
            _skillTreeViewer.OnNodeStateChanged?.Invoke(data.ID);
            OnStateUpdated?.Invoke(setUnlock);

            UpdateCosmeticState();
        }

        private void UpdateCosmeticState()
        {
            ESkillButtonCosmeticState cosmeticState;

            if (runtimeData.level == data.Properties.maxLevel)
            {
                SetCosmeticState(ESkillButtonCosmeticState.MaxedOut);
                return;
            }

            if (runtimeData.isUnlocked)
            {
                cosmeticState = ESkillButtonCosmeticState.Unlocked;

                if (isSelected)
                    cosmeticState = ESkillButtonCosmeticState.Selected;
            }
            else
            {
                cosmeticState = ESkillButtonCosmeticState.Locked;

                if (isSelected)
                    cosmeticState = ESkillButtonCosmeticState.LockedSelected;
            }


            SetCosmeticState(cosmeticState);
        }

        private void SetCosmeticState(ESkillButtonCosmeticState inState)
        {
            if (frame is null) return;

            var states = new Dictionary<ESkillButtonCosmeticState, Color>()
            {
                { ESkillButtonCosmeticState.Unlocked, frameColor },
                { ESkillButtonCosmeticState.Locked, frameLockedColor },
                { ESkillButtonCosmeticState.Selected, frameSelectedColor },
                { ESkillButtonCosmeticState.LockedSelected, frameLockedSelectedColor },
                { ESkillButtonCosmeticState.MaxedOut, frameMaxedColor },
            };

            if (frame)
            {
                frame.color = states[inState];
            }
        }

        private bool IsMaxLevel() => data.Properties.maxLevel == runtimeData.level;

        public int GetCost() => data.Properties.cost;
        public int GetLevel() => runtimeData.level;
        public int GetMaxLevel() => data.Properties.maxLevel;
        public bool GetIsUnlocked() => runtimeData.isUnlocked;

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isButtonDown == false) return;

            bool isWithinDoubleClickPeriod = Time.time - lastClickedTime < .3f;
            if (doubleClickQuickUnlockEnabled && isWithinDoubleClickPeriod)
                TryLevelUp();

            lastClickedTime = Time.time;

            holdFillMask.fillAmount = 0.0f;

            if (holdTime >= holdUnlockTime)
            {
                TryLevelUp();
            }

            isButtonDown = false;
            holdTime = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SkillTreeEventBus<ESkillNodeSelected>.Execute(new ESkillNodeSelected(this));
            isSelected = true;

            UpdateCosmeticState();

            if (runtimeData.isUnlocked == false || IsMaxLevel()) return;

            holdFillMask.fillAmount = 0.0f;
            isButtonDown = true;
        }
    }

    [System.Serializable]
    public enum ESkillButtonCosmeticState
    {
        Unlocked,
        Locked,
        Selected,
        LockedSelected,
        MaxedOut
    }

    public struct ESkillNodeLevelUp : ISkillTreeEvent
    {
        public readonly string Identifier;
        public int NewLevel;

        public ESkillNodeLevelUp(string identifier, int newLevel)
        {
            this.Identifier = identifier;
            this.NewLevel = newLevel;
        }
    }

    public struct ESkillNodeSelected : ISkillTreeEvent
    {
        public SkillTreeNode Node;

        public ESkillNodeSelected(SkillTreeNode skillTreeNode)
        {
            Node = skillTreeNode;
        }
    }
}
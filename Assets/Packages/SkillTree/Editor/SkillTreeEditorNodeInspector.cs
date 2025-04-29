using System.Collections.Generic;
using SkillTree.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeEditorNodeInspector : VisualElement
    {
        public VisualElement _spriteSelectorContainer;
        private ObjectField _spriteObjectField;
        private Image _spriteImage;

        private List<SkillTreeEditorNode> SelectedNodes = new List<SkillTreeEditorNode>();

        private SkillTreeGraphView _graphView;

        private TextField _skillTitleTextField;
        private TextField _skillDescriptionTextField;
        private Toggle _fullLevelRequiredToggle;
        private Toggle _bothParentsRequiredToggle;
        private IntegerField _maxLevelField;
        private IntegerField _costField;
        private TextField _skillIdentifierTextField;

        public SkillTreeEditorNodeInspector(SkillTreeGraphView graphView)
        {
            _graphView = graphView;
            AddToClassList("skillTreeEditorNodeInspector");

            StyleSheet styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/SkillTree/Editor/USS/SkillTreeEditorNodeInspector.uss");
            styleSheets.Add(styleSheet);

            CreateSpriteSelector();
            CreateTextFields();
            AddAllFields();
            SetupEvents();
        }

        private void AddItemVisualElement(VisualElement visualElement)
        {
            visualElement.AddToClassList("inspectorItems");
            Add(visualElement);
        }

        private void AddAllFields()
        {
            AddItemVisualElement(_spriteSelectorContainer);
            _spriteSelectorContainer.Add(_spriteImage);
            _spriteSelectorContainer.Add(_spriteObjectField);

            AddItemVisualElement(_skillIdentifierTextField);
            AddItemVisualElement(_skillTitleTextField);
            AddItemVisualElement(_skillDescriptionTextField);
            AddItemVisualElement(_fullLevelRequiredToggle);
            AddItemVisualElement(_bothParentsRequiredToggle);
            AddItemVisualElement(_maxLevelField);
            AddItemVisualElement(_costField);
        }

        private void SetupEvents()
        {
            _spriteObjectField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _fullLevelRequiredToggle.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _bothParentsRequiredToggle.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _skillTitleTextField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _skillDescriptionTextField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _fullLevelRequiredToggle.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _bothParentsRequiredToggle.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _maxLevelField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _costField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
            _skillIdentifierTextField.RegisterValueChangedCallback(_ => UpdateNodeWithInspectorDetails());
        }

        private void CreateTextFields()
        {
            _skillIdentifierTextField = new TextField("Skill Identifier");
            _skillTitleTextField = new TextField("Skill Title");
            _skillDescriptionTextField = new TextField("Skill Description")
            {
                multiline = true,
                autoCorrection = true
            };

            _skillDescriptionTextField.AddToClassList("multiline");

            _fullLevelRequiredToggle = new Toggle("Need parents max level");
            _bothParentsRequiredToggle = new Toggle("Need both parents?");
            _maxLevelField = new IntegerField("Max Level");
            _costField = new IntegerField("Cost");
        }

        private void CreateSpriteSelector()
        {
            _spriteObjectField = new ObjectField()
            {
                objectType = typeof(Sprite),
            };

            _spriteObjectField.AddToClassList("spriteObjectField");

            _spriteImage = new Image();
            _spriteImage.AddToClassList("spriteImage");
            _spriteImage.AddToClassList("inspectorItems");

            _spriteSelectorContainer = new VisualElement();
            _spriteSelectorContainer.AddToClassList("spriteSelectorContainer");
            _spriteSelectorContainer.AddToClassList("inspectorItems");
        }

        public void SelectionUpdated(List<ISelectable> selectables)
        {
            SelectedNodes.Clear();

            foreach (ISelectable selectable in selectables)
            {
                if (selectable is SkillTreeEditorNode editorNode)
                {
                    SelectedNodes.Add(editorNode);
                }
            }

            UpdateInspectorWithNodeDetails();
        }

        private void UpdateNodeWithInspectorDetails()
        {
            if (SelectedNodes.Count == 0) return;
            if (_graphView.GetSkillTreeNodeDataIndex(SelectedNodes[0].ID) is not { } nodeData) return;

            _spriteImage.sprite = _spriteObjectField.value as Sprite;

            NodeProperties properties = new NodeProperties()
            {
                identifier = _skillIdentifierTextField.text,
                icon = _spriteImage.sprite,
                title = _skillTitleTextField.value,
                description = _skillDescriptionTextField.value,
                requiresAllParentNodesToUnlock = _bothParentsRequiredToggle.value,
                requiresFullyLevelledParentsToUnlock = _fullLevelRequiredToggle.value,
                cost = _costField.value,
                maxLevel = _maxLevelField.value
            };

            nodeData.SetProperties(properties);

            _graphView.UpdateAsset();
            _graphView.Refresh();
        }

        private void UpdateInspectorWithNodeDetails()
        {
            if (SelectedNodes.Count == 0) return;
            if (_graphView.GetSkillTreeNodeDataIndex(SelectedNodes[0].ID) is not { } nodeData) return;

            NodeProperties properties = nodeData.Properties;

            _skillIdentifierTextField.SetValueWithoutNotify(properties.identifier);
            _spriteImage.sprite = properties.icon;
            _spriteObjectField.SetValueWithoutNotify(nodeData.Properties.icon);
            _costField.SetValueWithoutNotify(nodeData.Properties.cost);
            _maxLevelField.SetValueWithoutNotify(properties.maxLevel);
            _skillTitleTextField.SetValueWithoutNotify(nodeData.Properties.title);
            _skillDescriptionTextField.SetValueWithoutNotify(nodeData.Properties.description);
            _fullLevelRequiredToggle.SetValueWithoutNotify(nodeData.Properties.requiresAllParentNodesToUnlock);
            _bothParentsRequiredToggle.SetValueWithoutNotify(nodeData.Properties.requiresFullyLevelledParentsToUnlock);
        }
    }
}
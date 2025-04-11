using System.Collections.Generic;
using StatSystem;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class StatSystemEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset = default;

    private ScrollView _statDefinitionList = null;
    private VisualElement _editingContainer = null;
    private TextField _displayNameField = null;
    private TextField _descriptionField = null;
    private ObjectField _spriteField = null;

    private List<StatDefinition> _statDefinitions = new();
    private StatDefinition _selectedStatDefinition = null;

    private Dictionary<StatDefinition, Button> _definitionToButtons = new Dictionary<StatDefinition, Button>();

    [MenuItem("Window/UI Toolkit/Stat Definition Editor")]
    public static void ShowExample()
    {
        StatSystemEditor wnd = GetWindow<StatSystemEditor>();
        wnd.titleContent = new GUIContent("Stat Definition Editor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUxml = visualTreeAsset.Instantiate();
        root.Add(labelFromUxml);

        CacheCommonVisualElements();
        LoadStatDefinitions();
        PopulateStatListView();
        CreateSpriteField();
        BindCallbacks();

        _editingContainer.visible = false;
    }

    private void CreateSpriteField()
    {
        _spriteField = new ObjectField
        {
            objectType = typeof(Sprite),
            label = "Icon"
        };

        _spriteField.AddToClassList("StatFields");
        _editingContainer.Insert(0, _spriteField);
    }

    private void BindCallbacks()
    {
        _displayNameField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
        _descriptionField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
        _spriteField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
    }

    private void LoadStatDefinitions()
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(StatDefinition)}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.LoadAssetAtPath<StatDefinition>(path) is { } statDefinition)
                _statDefinitions.Add(statDefinition);
        }
    }

    private void CacheCommonVisualElements()
    {
        _statDefinitionList = rootVisualElement.Q<ScrollView>("StatList");
        _editingContainer = rootVisualElement.Q<VisualElement>("EditingContainer");
        _displayNameField = rootVisualElement.Q<TextField>("DisplayNameField");
        _descriptionField = rootVisualElement.Q<TextField>("DescriptionField");
    }

    private void PopulateStatListView()
    {
        if (_statDefinitionList == null) return;

        _statDefinitionList.contentContainer.Clear();

        foreach (StatDefinition statDefinition in _statDefinitions)
        {
            string statDefinitionDisplayName = statDefinition.displayName;

            Button button = new Button
            {
                text = statDefinitionDisplayName
            };

            button.clicked += () => OnStatDefinitionClicked(statDefinitionDisplayName);
            button.iconImage = new Background { sprite = statDefinition.icon };

            _definitionToButtons.Add(statDefinition, button);
            _statDefinitionList.Add(button);
        }
    }

    private void UpdateStatDefinitionToReflectFields()
    {
        if (_selectedStatDefinition == null) return;

        _selectedStatDefinition.displayName = _displayNameField.text;
        _selectedStatDefinition.description = _descriptionField.text;
        _selectedStatDefinition.icon = (Sprite)_spriteField.value;

        EditorUtility.SetDirty(_selectedStatDefinition);

        if (_definitionToButtons.TryGetValue(_selectedStatDefinition, out var button))
        {
            button.text = _selectedStatDefinition.displayName;
            button.iconImage = new Background { sprite = _selectedStatDefinition.icon };
        }
    }

    private void OnStatDefinitionClicked(string statButton)
    {
        if (_selectedStatDefinition != null)
        {
            _definitionToButtons[_selectedStatDefinition].RemoveFromClassList("selected");
        }

        _selectedStatDefinition = _statDefinitions.Find(x => x.displayName == statButton);
        if (_selectedStatDefinition == null) return;

        _definitionToButtons[_selectedStatDefinition].AddToClassList("selected");

        _editingContainer.visible = true;

        _spriteField.SetValueWithoutNotify(_selectedStatDefinition.icon);
        _displayNameField.SetValueWithoutNotify(_selectedStatDefinition.displayName);
        _descriptionField.SetValueWithoutNotify(_selectedStatDefinition.description);
    }
}
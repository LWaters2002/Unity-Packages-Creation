using System.Collections.Generic;
using StatSystem;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class StatSystemEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    private ScrollView statDefinitionList = null;
    private VisualElement editingContainer = null;
    private TextField displayNameField = null;
    private TextField descriptionField = null;
    private ObjectField spriteField = null;
    
    private List<StatDefinition> _statDefinitions = new();
    private StatDefinition _selectedStatDefinition = null;
    
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
        VisualElement labelFromUxml = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUxml);

        CacheCommonVisualElements();
        LoadStatDefinitions();
        PopulateStatListView();
        CreateSpriteField();
        BindCallbacks();
        
        editingContainer.visible = false;
    }

    private void CreateSpriteField()
    {
        spriteField = new ObjectField
        {
            objectType = typeof(Sprite),
            label = "Icon"
        };

        editingContainer.Add(spriteField);
    }

    private void BindCallbacks()
    {
        displayNameField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
        descriptionField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
        spriteField.RegisterValueChangedCallback(_ => UpdateStatDefinitionToReflectFields());
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
        statDefinitionList = rootVisualElement.Q<ScrollView>("StatList");
        editingContainer = rootVisualElement.Q<VisualElement>("EditingContainer");
        displayNameField = rootVisualElement.Q<TextField>("DisplayNameField");
        descriptionField = rootVisualElement.Q<TextField>("DescriptionField");
    }

    private void PopulateStatListView()
    {
        if (statDefinitionList == null) return;
        
        statDefinitionList.contentContainer.Clear();
        
        foreach (StatDefinition statDefinition in _statDefinitions)
        {
            string statDefinitionDisplayName = statDefinition.displayName;

            Button button = new Button
            {
                text = statDefinitionDisplayName
            };

            button.clicked += () => OnStatDefinitionClicked(statDefinitionDisplayName);

            statDefinitionList.Add(button);
        }
    }

    private void UpdateStatDefinitionToReflectFields()
    {
        if (_selectedStatDefinition == null) return;
        
        _selectedStatDefinition.displayName = displayNameField.text;
        _selectedStatDefinition.description = descriptionField.text;
        _selectedStatDefinition.icon = (Sprite)spriteField.value;
        
        EditorUtility.SetDirty(_selectedStatDefinition);
    }
    
    private void OnStatDefinitionClicked(string statButton)
    {
        _selectedStatDefinition = _statDefinitions.Find(x => x.displayName == statButton);
        if (_selectedStatDefinition == null) return;
        
        editingContainer.visible = true;
        
        spriteField.SetValueWithoutNotify(_selectedStatDefinition .icon);
        displayNameField.SetValueWithoutNotify(_selectedStatDefinition .displayName);
        descriptionField.SetValueWithoutNotify(_selectedStatDefinition .description);
    }
}
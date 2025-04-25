using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StatSystem.Editor
{
    public class StatTableEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private List<StatTable> _statTables = new();
        private List<StatDefinition> _statDefinitions = new();
        
        private StatTable _selectedTable;
        private Button _lastSelectedTableButton;
        private Button _lastSelectedStatButton;

        [MenuItem("Window/UI Toolkit/Stat Table Editor")]
        public static void ShowExample()
        {
            StatTableEditorWindow wnd = GetWindow<StatTableEditorWindow>();
            wnd.titleContent = new GUIContent("Stat Table Editor");
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Open(StatTable targetAsset)
        {
            StatTableEditorWindow[] windows = Resources.FindObjectsOfTypeAll<StatTableEditorWindow>();
            foreach (StatTableEditorWindow window in windows)
                window.Close();

            StatTableEditorWindow newWindow =
                CreateWindow<StatTableEditorWindow>(typeof(StatTableEditorWindow), typeof(SceneView));
            newWindow.SelectStatTable(targetAsset);
        }

        public void CreateGUI()
        {
            VisualElement visualTree = visualTreeAsset.Instantiate();
            rootVisualElement.Add(visualTree);
            
            GenerateStatTables();
            GenerateNewButtons();
            PopulateStatDropDown();

            if (_statTables.Count == 0) return;
            
            SelectStatTable(_statTables[0]);

            var statLookup = _statTables[0].StatLookup.ToArray();
            if (statLookup.Length == 0) return;

            SelectStat(statLookup[0].Key);
        }

        private void PopulateStatDropDown()
        {
            _statDefinitions.Clear();
            
            DropdownField dropdownField = rootVisualElement.Q<DropdownField>("NewStatDropdown");

            if (dropdownField == null) return;
            dropdownField.choices = new List<string>();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(StatDefinition)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadAssetAtPath<StatDefinition>(path) is not { } statDefinition) continue;

                dropdownField.choices.Add(statDefinition.displayName);
                _statDefinitions.Add(statDefinition);
            }
        }

        private void GenerateNewButtons()
        {
            Button newStatTableButton = rootVisualElement.Q<Button>("NewStatTable");
            Button newStatButton = rootVisualElement.Q<Button>("NewStat");

            if (newStatButton == null || newStatTableButton == null) return;

            newStatButton.clicked += CreateNewStat;
            newStatTableButton.clicked += CreateNewStatTable;
        }

        private void CreateNewStat()
        {
            DropdownField dropdownField = rootVisualElement.Q<DropdownField>("NewStatDropdown");
            StatDefinition statDefinition = _statDefinitions.Find(x => x.displayName == dropdownField.value);
            
            if (statDefinition == null) return;
            if (_selectedTable == null) return;

            if (_selectedTable.StatLookup.ContainsKey(statDefinition)) return;
            
            _selectedTable.StatLookup.Add(statDefinition, new Stat(0.0f));
            
            EditorUtility.SetDirty(_selectedTable);
            AssetDatabase.SaveAssets();
            
            SelectStatTable(_selectedTable);
        }

        private void CreateNewStatTable()
        {
            TextField textField = rootVisualElement.Q<TextField>("NewStatTableName");
            if (textField == null) return;

            string newName = textField.text;
            if (newName.Length == 0) return;
            
            string rootFolder = "Assets/StatTables/";
            
            if (AssetDatabase.IsValidFolder(rootFolder) == false)
                AssetDatabase.CreateFolder("Assets", "StatTables");
            
            AssetDatabase.CreateAsset(CreateInstance<StatTable>(), rootFolder + newName + ".asset");
            AssetDatabase.SaveAssets();
            
            GenerateStatTables();
        }

        private void SelectStat(StatDefinition statDefinition, Button button = null)
        {
            _lastSelectedStatButton?.RemoveFromClassList("Selected");
            _lastSelectedStatButton = button;
            _lastSelectedStatButton?.AddToClassList("Selected");
            
            
        }

        private void GenerateStatTables()
        {
            LoadAllStatTables();

            ScrollView scrollView = rootVisualElement.Q<ScrollView>("TableList");
            if (scrollView == null) return;

            scrollView.contentContainer.Clear();

            foreach (StatTable statTable in _statTables)
            {
                Button button = new Button
                {
                    text = statTable.name,
                };

                button.clicked += () => SelectStatTable(statTable, button);
                scrollView.Add(button);
            }
        }

        private void LoadAllStatTables()
        {
            _statTables.Clear();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(StatTable)}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.LoadAssetAtPath<StatTable>(path) is { } statTable)
                    _statTables.Add(statTable);
            }
        }

        private void SelectStatTable(StatTable table, Button button = null)
        {
            if (table == null) return;

            _lastSelectedTableButton?.RemoveFromClassList("selected");
            _lastSelectedStatButton?.RemoveFromClassList("selected");

            _lastSelectedTableButton = button;
            _lastSelectedTableButton?.AddToClassList("selected");

            _selectedTable = table;

            UpdateStatList(table);
        }

        private void UpdateStatList(StatTable table)
        {
            ScrollView scrollView = rootVisualElement.Q<ScrollView>("StatList");

            if (scrollView == null) return;

            scrollView.contentContainer.Clear();
            foreach (var statPair in table.StatLookup)
            {
                StatDefinition statDefinition = statPair.Key;

                Button button = new Button
                {
                    text = statDefinition.displayName,
                };

                scrollView.Add(button);
            }
        }
    }
}
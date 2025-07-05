using System;
using System.Collections.Generic;
using SkillTree.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public partial class SkillTreeGraphView
    {
        private static List<SkillTreeNodeData> _nodeClipboard = new();
        private Dictionary<Tuple<KeyCode, EventModifiers>, Tuple<Action, string>> _shortcuts;

        private void InitCommands()
        {
            RegisterShortcuts();
            SetupRightClickMenu();

            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void RegisterShortcuts()
        {
            _shortcuts = new Dictionary<Tuple<KeyCode, EventModifiers>, Tuple<Action, string>>()
            {
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.E, EventModifiers.None),
                    new Tuple<Action, string>(this.AddNodeAction, "Create new skill node")
                },
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.R, EventModifiers.None),
                    new Tuple<Action, string>(this.Refresh, "Refresh Nodes")
                },
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.Q, EventModifiers.None),
                    new Tuple<Action, string>(this.StraightenNodes, "Straighten Nodes")
                },
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.Q, EventModifiers.Shift),
                    new Tuple<Action, string>(this.SpaceEquidistant, "Space Equidistant")
                },
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.C, EventModifiers.Control),
                    new Tuple<Action, string>(this.Copy, "Copy")
                },
                {
                    new Tuple<KeyCode, EventModifiers>(KeyCode.V, EventModifiers.Control),
                    new Tuple<Action, string>(this.Paste, "Paste")
                },
            };
        }

        private void SetupRightClickMenu()
        {
            var contextManipulator = new ContextualMenuManipulator(evt =>
            {
                foreach (var shortcut in _shortcuts)
                {
                    string shortcutLabel =
                        shortcut.Key.Item2 == EventModifiers.None
                            ? ""
                            : shortcut.Key.Item2 + " + ";
                    shortcutLabel += $"{shortcut.Key.Item1.ToString()}";

                    evt.menu.AppendAction
                    (
                        $"[{shortcutLabel}] {shortcut.Value.Item2}",
                        (_) => shortcut.Value.Item1.Invoke()
                    );
                }
            });

            this.AddManipulator(contextManipulator);
        }

        protected override void HandleEventBubbleUp(EventBase evt)
        {
            if (evt is ICommandEvent command)
            {
                bool commandSuccess = ProcessCommands(command.commandName);

                if (commandSuccess)
                {
                    evt.StopPropagation();
                }

                return;
            }

            base.HandleEventBubbleUp(evt);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            Tuple<KeyCode, EventModifiers> keyEvent = new(evt.keyCode, evt.modifiers);

            if (_shortcuts.ContainsKey(keyEvent) == false) return;
            _shortcuts[keyEvent].Item1.Invoke();
        }

        private bool ProcessCommands(string command)
        {
            Debug.Log("Bubble up : [" + command + "]");

            var commandMap = new Dictionary<string, Action>()
            {
                { "SoftDelete", SoftDelete },
                { "SelectAll", SelectAll }
            };

            if (!commandMap.ContainsKey(command)) return false;

            commandMap[command].Invoke();
            return true;
        }

        private void SelectAll()
        {
            selection.Clear();

            foreach (var node in SkillTreeNodes)
            {
                AddToSelection(node);
            }
        }

        private void SoftDelete()
        {
            foreach (var selected in selection)
            {
                if (selected is not SkillTreeEditorNode node) continue;
                RemoveNode(node);
            }
        }

        private void Copy()
        {
            if (selection.Count == 0) return;

            _nodeClipboard.Clear();
            Vector2 firstPosition = Vector2.zero;

            for (var index = 0; index < selection.Count; index++)
            {
                var selected = selection[index];
                if (selected is not SkillTreeEditorNode node) continue;

                if (index == 0)
                {
                    firstPosition = node.GetPosition().position;
                }

                if (!CurrentSkillTreeAsset.GetNodeData(node.ID, out var data)) continue;

                SkillTreeNodeData newNode = data.CreateShallowCopy();
                newNode.ID = node.ID;

                Rect newPos = newNode.Position;
                newPos.position -= firstPosition;
                newNode.SetPosition(newPos);

                _nodeClipboard.Add(newNode);
            }
        }

        private void Paste()
        {
            var oldToNewGuidMap = new Dictionary<string, string>();

            List<SkillTreeNodeData> nodesToPaste = new();

            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = contentViewContainer.WorldToLocal(mousePos);

            // Regenerate GUIDS and Update Relative Position
            foreach (var node in _nodeClipboard)
            {
                var newNode = node.CreateShallowCopy();
                nodesToPaste.Add(newNode);

                string originalID = node.ID;
                newNode.ID = Guid.NewGuid().ToString();
                oldToNewGuidMap.Add(originalID, newNode.ID);

                Rect newPos = newNode.Position;
                newPos.position += mousePos;
                newNode.SetPosition(newPos);
            }

            //Replace GUID references with new GUID
            foreach (var node in nodesToPaste)
            {
                for (var index = node.ParentGuids.Count - 1; index >= 0; index--)
                {
                    var parentGuid = node.ParentGuids[index];
                    if (oldToNewGuidMap.TryGetValue(parentGuid, out var newGuid))
                    {
                        node.ParentGuids[index] = newGuid;
                    }
                }
            }

            selection.Clear();

            // Add the sanitised nodes back
            foreach (var node in nodesToPaste)
            {
                CurrentSkillTreeAsset.Nodes.Add(node);
            }

            this.Refresh();

            foreach (var node in nodesToPaste)
            {
                if (NodeLookup.ContainsKey(node.ID))
                {
                    AddToSelection(NodeLookup[node.ID]);
                }
            }
        }

        private void SpaceEquidistant()
        {
            if (selection.Count == 0) return;

            Vector2 positionStep = Vector2.zero;

            foreach (var selected in selection)
            {
                if (selected is not SkillTreeEditorNode node) continue;
                positionStep += node.layout.position;
                positionStep -= ((VisualElement)selection[0]).layout.position;
            }

            positionStep /= selection.Count;

            bool horizontalOrVertical = AreNodesHorizontalOrVertical();
            for (int index = 0; index < selection.Count; index++)
            {
                var selected = selection[index];
                if (selected is not SkillTreeEditorNode node) continue;

                Rect rect = node.GetPosition();
                node.SetPosition(rect);
            }
        }

        private void StraightenNodes()
        {
            if (selection.Count == 0) return;

            Vector2 averagePosition = GetAveragePositionOfNodes();

            bool horizontalOrVertical = AreNodesHorizontalOrVertical();
            foreach (ISelectable selected in selection)
            {
                if (selected is not SkillTreeEditorNode node) continue;
                Rect rect = node.GetPosition();

                if (horizontalOrVertical)
                {
                    rect.x = averagePosition.x;
                }
                else
                {
                    rect.y = averagePosition.y;
                }

                node.SetPosition(rect);
            }
        }

        private bool AreNodesHorizontalOrVertical()
        {
            Vector2 averagePosition = GetAveragePositionOfNodes();
            Vector2 relativeAveragePosition = averagePosition - ((VisualElement)selection[0]).layout.position;
            return Mathf.Abs(relativeAveragePosition.x) < MathF.Abs(relativeAveragePosition.y);
        }

        private Vector2 GetAveragePositionOfNodes()
        {
            Vector2 averagePosition = Vector2.zero;

            foreach (var selected in selection)
            {
                if (selected is not SkillTreeEditorNode node) continue;
                averagePosition += node.layout.position;
            }

            averagePosition /= selection.Count;

            return averagePosition;
        }
    }
}
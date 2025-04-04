using System.Collections.Generic;
using SkillTree.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace SkillTree.Runtime
{
    public class SkillTreeViewer : MonoBehaviour
    {
        public float ZoomSensitivity = .2f;

        [SerializeField] public SkillTreeAsset skillTreeAsset;

        [Header("Prefabs")] [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject arrowPrefab;

        [Header("References")] [SerializeField]
        private GameObject contentContainer;

        [SerializeField] private Canvas canvas;

        private SkillTreeControls _controls;
        private bool _isDragging = false;

        public Dictionary<string, SkillTreeNode> Nodes { get; private set; } = new Dictionary<string, SkillTreeNode>();
        
        private float _targetZoom = 1.0f;

        public System.Action<string> OnNodeStateChanged;
        
        private void Start()
        {
            GenerateUI();
            
            _controls = new SkillTreeControls();
            _controls.Enable();
            _controls.UI.Drag.performed += _ => _isDragging = true;
            _controls.UI.Drag.canceled += _ => _isDragging = false;
            _controls.UI.Zoom.performed += OnZoom;
        }

        private void OnZoom(InputAction.CallbackContext obj)
        {
            float zoomValue = obj.ReadValue<float>();

            _targetZoom += zoomValue * ZoomSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom, 0.4f, 2.5f);
        }

        private void GenerateUI()
        {
            GameObject arrowContainer = new GameObject("Arrows");
            
            arrowContainer.transform.SetParent(contentContainer.transform);
            arrowContainer.transform.localPosition = Vector3.zero;
            arrowContainer.transform.localScale = Vector3.one;

            foreach (SkillTreeNodeData nodeData in skillTreeAsset.Nodes)
            {
                SkillTreeNode newNode =
                    Instantiate(nodePrefab, contentContainer.transform).GetComponent<SkillTreeNode>();
                newNode.Init(this, nodeData);
                Nodes.Add(nodeData.ID, newNode);
            }
            
            // Update all unlock states
            foreach (KeyValuePair<string, SkillTreeNode> node in Nodes)
            {
                node.Value.UpdateUnlockState();
            }

            foreach (SkillTreeNodeData nodeData in skillTreeAsset.Nodes)
            {
                foreach (string guid in nodeData.ParentGuids)
                {
                    if (!Nodes.ContainsKey(guid)) continue;
                    if (!Nodes.ContainsKey(nodeData.ID)) continue;

                    SkillTreeNode startNode = Nodes[nodeData.ID];
                    SkillTreeNode parentNode = Nodes[guid];

                    SkillTreeArrowGraphic arrow = Instantiate(arrowPrefab, arrowContainer.transform)
                        .GetComponent<SkillTreeArrowGraphic>();
                    
                    arrow.StartPoint = parentNode.transform.localPosition;
                    arrow.EndPoint = startNode.transform.localPosition;
                }
            }
        }

        private void Update()
        {
            if (_isDragging)
            {
                Vector2 pos = _controls.UI.MouseDelta.ReadValue<Vector2>();
                contentContainer.transform.position = (Vector2)contentContainer.transform.position + pos;
            }

            canvas.scaleFactor = Mathf.Lerp(canvas.scaleFactor, _targetZoom, Time.deltaTime * 5.0f);
        }
    }
}
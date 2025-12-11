using System;
using System.Collections.Generic;
using System.Linq;
using Misc;
using UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Object = UnityEngine.Object;

public class HudController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Transform uiFragmentsContainer;
    
    [Header("Camera Options")]
    [SerializeField] private HoverPanToCinemachine pan;

    private VisualElement root;
    
    public bool IsPointerOverUI;
    public string PickedElementName;
    
    private List<VisualElement> _fragments = new();
    
    
    private void Awake()
    {
        root = uiDocument.rootVisualElement.Q("hudRoot");
        foreach (var componentsInChild in uiFragmentsContainer.GetComponentsInChildren<UIFragment>())
        {
            try
            {
                componentsInChild.Create(root);
                _fragments.Add(componentsInChild.RootElement);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        var mouseArea = uiDocument.rootVisualElement.Q("mouseAreas");
        foreach (var ma in mouseArea.Children())
        {
            Debug.Log(ma.name);
            ma.RegisterCallback<PointerEnterEvent>(_ => pan.SetActive(true));
            ma.RegisterCallback<PointerLeaveEvent>(_ => pan.SetActive(false));
            _fragments.Add(ma);
        }
        
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    // Update is called once per frame
    void Update()
    {
        IsPointerOverUI = _IsPointerOverUI();
    }
    
    private bool _IsPointerOverUI()
    {
        var panel = root.panel;
        if (panel == null)
            return false;

        var screenPos = Mouse.current.position.ReadValue();
        var panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);
        panelPos.y = panel.visualTree.worldBound.height - panelPos.y;

        foreach (var child in _fragments)
        {
            if (!child.visible || child.pickingMode == PickingMode.Ignore)
                continue;

            var bounds = child.worldBound;

            // Debug.Log($"{child.name} | {bounds} | ContainsPanel={bounds.Contains(panelPos)} | Mouse={panelPos}");

            if (bounds.Contains(panelPos))
            {
                PickedElementName = child.name;
                return true;
            }
        }

        PickedElementName = null;
        return false;
    }
}

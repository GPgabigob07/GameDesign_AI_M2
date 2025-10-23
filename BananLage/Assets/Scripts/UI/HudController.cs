using System;
using System.Collections.Generic;
using Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HudController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private ScrollView structuresScroll;
    
    [Header("Buildings Container")]
    [SerializeField] private VisualTreeAsset buildingItemTemplate;

    private List<StructureData> Structures;
    private static string StructuresDataPath = "Data/Structures";

    private void Awake()
    {
        Structures = new List<StructureData>();
        Structures.AddRange(Resources.LoadAll<StructureData>(StructuresDataPath));
        
        root = uiDocument.rootVisualElement;
        structuresScroll = root.Q<ScrollView>("structureScrollView");

        var e = root;
        while (e.parent != null)
        {
            e.pickingMode = PickingMode.Ignore;
            e = e.parent;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupBuildingsList();
    }

    private void SetupBuildingsList()
    {
        foreach (var structureData in Structures)
        {
            var item = buildingItemTemplate.CloneTree();
            item.dataSource = structureData;
            
            structuresScroll.contentContainer.Add(item);
        }

        structuresScroll.RegisterCallback<PointerEnterEvent>(e => structuresScroll.ToggleInClassList("expanded"));
        structuresScroll.RegisterCallback<PointerLeaveEvent>(e => structuresScroll.RemoveFromClassList("expanded"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public bool IsPointerOverUI()
    {
        var panel = uiDocument.rootVisualElement.panel;
        var pos = Mouse.current.position.ReadValue();
        var pick = panel.Pick(pos);
        return pick != null;
    }
}

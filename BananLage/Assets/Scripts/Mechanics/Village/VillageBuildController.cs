using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Mechanics.Village
{
    public class VillageBuildController: MonoBehaviour
    {
        public Transform mouseAnchor;
        public HudController hudController;
        public PlayerInput playerInput;
        public Camera camera;
        
        private void Update()
        {
            var inUI = hudController.IsPointerOverUI();
            playerInput.SwitchCurrentActionMap(inUI ? "UI" : "Player");

            if (!inUI) MoveAnchor();
        }

        private void MoveAnchor()
        {
            var mouse = Mouse.current.position.ReadValue();
            var pos = camera.ScreenToWorldPoint(mouse);
            pos.z = 0;
            mouseAnchor.position = pos;
        }
    }
}
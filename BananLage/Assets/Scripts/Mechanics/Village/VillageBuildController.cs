using Mechanics.Building;
using Structures.Types;
using UI;
using UI.Controllers.Structure_List;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mechanics.Village
{
    public class VillageBuildController : MonoBehaviour
    {
        public BuildStructureController original;
        public MouseActor mouseActor;
        public HudController hudController;
        public StructureListController structureListController;
        public BuildStructureController previewStructureController;
        public PlayerInput playerInput;

        private Transform MouseAnchor => mouseActor.transform;

        [Header("Camera settings"), SerializeField]
        private Camera camera;

        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] float minZoom, maxZoom;

        private void Update()
        {
            var inUI = hudController.IsPointerOverUI;
            playerInput.SwitchCurrentActionMap(inUI ? "UI" : "Player");

            if (!inUI) MoveAnchor();

            if (previewStructureController)
            {
                previewStructureController.PreviewAt(SnapToGrid(MouseAnchor.position),
                    structureListController.SelectedStructure);
            }
        }

        private void MoveAnchor()
        {
            var mouse = Mouse.current.position.ReadValue();
            var pos = camera.ScreenToWorldPoint(mouse);
            pos.z = 0;
            MouseAnchor.position = structureListController.SelectedStructure ? SnapToGrid(pos).To3D() : pos;
        }

        private Vector2 SnapToGrid(Vector3 pos)
        {
            if (structureListController.SelectedStructure)
            {
                return structureListController.SelectedStructure.GetBoundsAt(pos.To2D()).center;
            }

            return pos.ToInt().To2D();
        }

        private void OnConfirmBuilding()
        {
            var data = structureListController.SelectedStructure;

            if (!data) //not building
            {
                mouseActor.SelectNearestObject();
            }

            var pos = MouseAnchor.position.ToInt();
            if (!original || !previewStructureController) return;
            if (!previewStructureController.CanPlaceAt(pos.To2D())) return;

            if (!VillageManager.VillageInventory.Consume(data.buildCosts)) return;

            var bounds = data.GetBoundsAt(pos.To2D());

            var controller = Instantiate(original, bounds.center, Quaternion.identity);
            controller.StructureData = data;
            previewStructureController.PreviewAt(pos.To2D(), null);
        }

        private void OnZoom(InputValue value)
        {
            var newZoom = -value.Get<float>();
            cinemachineCamera.Lens.OrthographicSize =
                Mathf.Clamp(cinemachineCamera.Lens.OrthographicSize + newZoom, minZoom, maxZoom);
        }

        private void OnCancelBuilding()
        {
            structureListController.ClearSelection();
        }
    }
}
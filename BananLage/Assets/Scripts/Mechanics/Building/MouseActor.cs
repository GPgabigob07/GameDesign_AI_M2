using System;
using System.Linq;
using AI.Monkey;
using Mechanics.Village;
using Structures;
using UI.Controllers.Object_Details;
using UI.Controllers.Object_Details.Types;
using Unity.Cinemachine;
using UnityEngine;

namespace Mechanics.Building
{
    public class MouseActor: MonoBehaviour
    {
        [SerializeField] private LayerMask detectionLayers;
        [SerializeField] private float detectionRangePerCameraZoom, checkTime;
        [SerializeField] private CinemachineCamera mainCamera;
        [SerializeField] private GameObject[] objectsInRange;
        [SerializeField] private ObjectDetailsController detailsController;

        private float throttle = 0f;
        
        private void Awake()
        {
            objectsInRange = Array.Empty<GameObject>();
        }

        private void FixedUpdate()
        {
            throttle += Time.fixedDeltaTime;

            if (throttle <= checkTime) return;
            throttle = 0f;
            
            var hits = Physics2D.OverlapCircleAll(transform.position, mainCamera.Lens.OrthographicSize * detectionRangePerCameraZoom, detectionLayers);
            objectsInRange = hits.Select(e => e.gameObject).ToArray();
        }

        public void SelectNearestObject()
        {
            if (!objectsInRange.Any())
            {
                VillageManager.RenderMonkeyPath(new());
                detailsController.SetObject(null);
                return;
            }
            
            var pos = transform.position;
            var nearest = objectsInRange
                .OrderBy(e => Vector3.Distance(pos, e.transform.position))
                .First();
            
            if (!nearest) return;

            if (nearest.TryGetComponent<MonkeyCharacterBT>(out var monkey))
            {
                monkey.Agent.Preview();
                detailsController.ConfigureMonkey(monkey.CycleData);
            }
            else if (nearest.TryGetComponent<BaseStructure>(out var str)) detailsController.ConfigureStructure(str);
            
            SoundEngine.PlaySFX(SoundEngine.bus.Sounds.sfxSelectWorldObj, transform);
        }

        private void OnDrawGizmosSelected()
        {
            if (!mainCamera) return;
            
            var v = mainCamera.Lens.OrthographicSize *  detectionRangePerCameraZoom;
            Gizmos.DrawWireSphere(transform.position, v);
        }
    }
}
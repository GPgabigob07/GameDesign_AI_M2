using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mechanics
{
    public class MapController : MonoBehaviour
    {
        [Header("Map Settings")] [SerializeField]
        private bool debug;

        [Header("Tilemaps")] [SerializeField] private List<BiomeEntry> terrainTiles;
        [SerializeField] private TileBase[] streetTiles;
        [SerializeField] private Tilemap terrain, streets, structures;
        [SerializeField] private List<BaseStructure> forestStructures;

        private MapMatrix Map;

        private void Start()
        {
            var bake = MapStorage.LoadBake();
            Map = new MapMatrix(bake);

            Map.Render(terrain, streets, GetForestStructure, terrainTiles.ToDictionary(e => e.type, e => e.tiles),
                streetTiles.RandomEntry());
        }

        private BaseStructure GetForestStructure()
        {
            return Instantiate(forestStructures.RandomEntry());
        }

        public bool IsAreaAvailable(Bounds bounds, object source)
        {
            if (debug)
                Debug.Log($"#MapController# Checking bounds {bounds}");

            foreach (var pos in bounds.Points())
            {
                var c = pos.ToInt();
                if (!Map.CanPlaceAt(c))
                {
                    Debug.Log($"#MapController# Position occupied {c}");
                    return false;
                }
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!debug) return;
            
            
        }

        public static void Assign(BaseStructure structure)
        {
            var b = structure.StructureData.GetBoundsAt(structure.transform.position);
            MapMatrix.GetInstance().PlaceStructure(b, structure.JobType == TaskType.Build);
        }

        public void Unassign(BaseStructure baseStructure)
        {
        }
    }

    [Serializable]
    public struct BiomeEntry
    {
        public Biome type;
        public TileBase[] tiles;
    }
}
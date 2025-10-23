using System;
using Mechanics;
using Mechanics.Jobs.Types;
using Mechanics.Village;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Structures.Types
{
    public class BuildStructureController : StructureController<BuildJobContext>
    {
        #region Components

        [Header("Components")]
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TilemapRenderer tilemapRenderer;
        [SerializeField] private SpriteRenderer structurePreview;
        [SerializeField] private Transform structureProgress;

        [SerializeField] private TileBase leftTop,
            topEdge,
            rightTop,
            leftEdge,
            centerFill,
            rightEdge,
            bottomLeft,
            bottomEdge,
            bottomRight;

        #endregion
        
        private TileBase GetTileFor(int x, int y, out bool middle)
        {
            var size = StructureData.worldSize;

            var left = x == 0;
            var right = x == size.x - 1;
            var bottom = y == 0;
            var top = y == size.y - 1;
            
            var edgeX = left || right;
            var edgeY = top || bottom;

            middle = !edgeY && !edgeX;
            if (middle) return centerFill;
            
            if (left && top) return leftTop;
            if (left && bottom) return bottomLeft;
            if (right && top) return rightTop;
            if (right && bottom) return bottomRight;

            if (left) return leftEdge;
            if (right) return rightEdge;
            if (top) return topEdge;
            if (bottom) return bottomEdge;

            return null;
        }

        [ContextMenu("Fill Area")]
        private void FillArea()
        {
            tilemap.ClearAllTiles();
            var size = StructureData.worldSize;
            var offsetX = -size.x / 2;
            var offsetY = -size.y / 2;
            for (var x = 0; x < size.x; x++)
            {
                for (int i = 0; i < size.y; i++)
                {
                    var pos = new Vector3Int(x + offsetX, i + offsetY, 0);
                    var tile = GetTileFor(x, i, out var middle);
                    tilemap.SetTile(pos, tile);

                    if (middle && Random.value < 0.5f)
                    {
                        tilemap.SetTileFlags(pos, TileFlags.None);
                        tilemap.SetTransformMatrix(pos, Matrix4x4.Scale(new Vector3(-1, 1, 1)));
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            FillArea();
            structureProgress.localScale = StructureData.worldSize.ToFloat();
            structureProgress.localPosition = new Vector3(0, -StructureData.worldSize.y, 0);
            structurePreview.sprite = StructureData.uiSprite;
        }

        private void Update()
        {
            if (CurrentJob == null) return;
            structureProgress.localPosition = new Vector3(0, -(StructureData.worldSize.y * (1 -CurrentJob.Progress)), 0);
        }

        protected override BuildJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            var wasNull = CurrentJob == null;
            CurrentJob ??= new BuildJobContext(monkey, this, TaskType.Build);
            
            if (!wasNull)
            {
                CurrentJob.AddMonkey(monkey);
            }
            
            return CurrentJob;
        }

        public void FinishBuild()
        {
            _currentJob = null;
            VillageManager.Place(StructureData, transform.position);
            Destroy(gameObject);
        }
    }
}
using System;
using System.Collections;
using System.Linq;
using Map;
using Mechanics;
using Mechanics.ItemManagement;
using Mechanics.Jobs.Types;
using Mechanics.Village;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Structures.Types
{
    public class BuildStructureController : StructureController<BuildJobContext>
    {
        private static readonly int Progress1 = Shader.PropertyToID("_Progress");

        #region Components

        [SerializeField] private bool debubPlacement;

        [Header("Components")] [SerializeField]
        private Tilemap tilemap;

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


        private MaterialPropertyBlock progressBlock;

        #endregion

        #region Preview

        [Header("Building Preview")] [SerializeField]
        private Color previewError;

        [SerializeField] private Color previewColor = Color.white;

        [SerializeField] private bool previewPlacement = false;
        [SerializeField] private Color currentPreviewColor;

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
            progressBlock = new MaterialPropertyBlock();
            if (!StructureData) return;

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

            var anchor = new Vector3(0, 0, 0);
            if (size.x % 2 == 0) anchor.x = 0.5f;
            if (size.y % 2 == 0) anchor.y = 0.5f;

            tilemap.tileAnchor = anchor;
        }

        protected override IEnumerator Start()
        {
            yield return base.Start();
            currentPreviewColor = previewColor;
            _currentJob = new BuildJobContext(this);
            FillArea();

            structureProgress.localScale = StructureData.worldSize.To3D();
            structurePreview.sprite = StructureData.uiSprite;
        }

        public override int CalculateCost()
        {
            return StructureData.buildConfiguration.maxAvPerWorker;
        }

        protected override void Update()
        {
            base.Update();
            if (CurrentJob == null) return;

            progressBlock.SetFloat(Progress1, (1 - CurrentJob.Progress));
            SpriteRenderer.SetPropertyBlock(progressBlock);

            structureProgress.localScale =
                new Vector3(1, Mathf.Lerp(0, StructureData.worldSize.y, CurrentJob.Progress), 1);

            tilemap.color = currentPreviewColor;
            if (previewPlacement && transform.parent)
            {
                var pos = transform.parent.position;
                transform.position = pos.ToInt();
            }
        }

        protected override BuildJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            return _currentJob is { HasEnded: true } ? null : _currentJob?.Add<BuildJobContext>(monkey);
        }

        public void FinishBuild()
        {
            VillageManager.Place(_currentJob, StructureData, transform.position);
            _currentJob = null;
            Destroy(gameObject);
        }

        public override TaskType JobType => TaskType.Build;

        public override bool IsAvailable =>
            !previewPlacement && _currentJob is null or { HasEnded: false, HasSpace: true };

        public void PreviewAt(Vector2 position, StructureData structureData)
        {
            if (StructureData != structureData)
            {
                StructureData = structureData;
                FillArea();
            }

            if (!structureData) return;

            /*var b = StructureData.worldSize;
            var x = b.x % 2 == 0 ? 0.5f : 0;
            var y = b.y % 2 == 0 ? 0.5f : 0;
            transform.localPosition = new Vector3(x, y, 0);*/

            tilemap.color = CanPlaceAt(position) ? previewColor : previewError;
            currentPreviewColor = previewColor;
            previewPlacement = true;
        }

        private void OnDrawGizmos()
        {
            if (previewPlacement && debubPlacement && StructureData)
            {
                var bb = StructureData.GetBoundsAt(transform.parent.position.ToInt().To2D());
                Debug.DrawLine(new Vector2(bb.min.x, bb.max.y), new Vector2(bb.max.x, bb.max.y), previewColor);
                Debug.DrawLine(new Vector2(bb.min.x, bb.min.y), new Vector2(bb.max.x, bb.min.y), previewColor);
                Debug.DrawLine(new Vector2(bb.min.x, bb.min.y), new Vector2(bb.min.x, bb.max.y), previewColor);
                Debug.DrawLine(new Vector2(bb.max.x, bb.max.y), new Vector2(bb.max.x, bb.max.y), previewColor);
            }
        }

        public bool CanPlaceAt(Vector2 pos)
        {
            if (!StructureData) return false;

            var hasResources = true;
            foreach (var cost in StructureData.buildCosts)
            {
                hasResources = hasResources && VillageManager.VillageInventory.Has(cost.resource, cost.amount);
            }
            

            var map = VillageManager.Map;
            return hasResources 
                   && !Physics2D.OverlapBox(pos, StructureData.worldSize.ToFloat(), 0, 1 << 6)
                   && map.IsAreaAvailable(StructureData.GetBoundsAt(pos), StructureData);
        }
    }
}
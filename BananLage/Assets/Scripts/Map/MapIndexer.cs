using UnityEngine;

namespace Map
{
    public class MapIndexer
    {
        private readonly int width, height;
        private readonly int halfW, halfH;

        public MapIndexer(int width, int height)
        {
            this.width = width;
            this.height = height;
            halfW = width / 2;
            halfH = height / 2;
        }

        public Vector2Int IndexToPoint(int i, int j)
        {
            return new Vector2Int(
                i - halfW,
                j - halfH
            );
        }

        public (int i, int j) PointToIndex(Vector2Int p)
        {
            return (
                Mathf.Clamp(p.x + halfW, 0, width - 1),
                Mathf.Clamp(p.y + halfH, 0, height - 1)
            );
        }

        public bool IsValidPoint(Vector2Int pos)
        {
            var x = pos.x + halfW;
            var y = pos.y + halfH;
            
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}
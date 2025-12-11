using System;
using System.Collections.Generic;
using Mechanics.Village;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics.PathFinding
{
    public class PathAgent : MonoBehaviour
    {
        private Rigidbody2D rb;
        public float speed = 4f;
        public float mag = 0.5f;

        public Action OnPathFound;

        private List<Vector2Int> _activePath = new List<Vector2Int>();
        public PriorityQueue<PathNode> queue;
        public Bounds targetArea;

        private bool preview;

        private int _progress;
        private int Progress
        {
            get => _progress;
            set  {
                _progress = value;
                OnProgressChanged();
            }
        }

        private void OnProgressChanged()
        {
            if (!preview) return; 
            VillageManager.RenderMonkeyPath(Path);
        }

        [field: SerializeField] public bool Finished { get; private set; } = true;

        [field: SerializeField] public bool Fetching { get; internal set; } = false;
        public List<Vector2Int> Path => _activePath.GetRange(Progress > 0 ? Progress -1 : 0, _activePath.Count - _progress);

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void FixedUpdate()
        {
            if (_activePath.Count == 0) return;
            
            Finished = Progress >= _activePath.Count;
            if (Finished || _activePath.Count == 0) return;

            var t = _activePath[Progress];

            var dir = t - rb.position;
            dir.Normalize();

            if (Vector2.Distance(t, rb.position) <= mag) Progress++;

            rb.MovePosition(transform.position.To2D() + dir * (speed * Time.fixedDeltaTime * VillageManager.Speed));
        }

        public void NewPath(List<Vector2Int> path)
        {
            //notify listeners
            OnPathFound?.Invoke();
            _activePath = path;
            
            Progress = 0;

            //begin synchronized movement
            Finished = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (queue == null) return;

            Gizmos.color = Color.cyan;
            foreach (var pathNode in queue)
            {
                Gizmos.DrawCube(pathNode.item.Position.To3D(), Vector3.one * 0.5f);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(targetArea.center, Vector3.one * 0.5f);

            if (_activePath.Count == 0) return;
            var current = _activePath[Progress % _activePath.Count];
            foreach (var vector2Int in _activePath)
            {
                if (current == vector2Int)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.blue;

                Gizmos.DrawCube(vector2Int.To3D(), Vector3.one * 0.5f);
            }
        }

        public void Preview()
        {
            preview = true;
        }

        public void RemovePreview()
        {
            preview = false;
        }
    }
}
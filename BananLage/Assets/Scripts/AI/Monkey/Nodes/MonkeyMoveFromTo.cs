using System;
using Behaviour_Tree;
using Mechanics;
using Mechanics.PathFinding;
using Mechanics.Village;
using Structures;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.Monkey.Nodes
{
    //success = arrived
    //running = going
    //failure = never?
    public class MonkeyMoveTo : LeafNode<MonkeyCharacterBT>
    {
        private PathAgent _moveBehaviour;

        public override NodeResult Process()
        {
            var ctx = Manager.CurrentJob;
            if (ctx == null) return NodeResult.Failure;

            if (Manager.debug)
                Debug.Log($"[MoveTo] Has Job Context:{ctx}...");

            var str = ctx.Structure;
            if (!str)
            {
                if (Manager.debug)
                    Debug.Log($"[MoveTo] Has No Structure, Bail Out");
                return NodeResult.Failure;
            }

            var inArea = str.InWorkingArea(Manager.transform.position);
            if (Manager.debug)
                Debug.Log($"[MoveTo] InArea:{inArea}");

            if (inArea)
                return NodeResult.Success;

            var wasNull = !_moveBehaviour;
            if (wasNull)
            {
                _moveBehaviour = Manager.gameObject.GetComponent<PathAgent>();
                if (!_moveBehaviour)
                    throw new Exception("Something went wrong => cannot add MonkeyMovingAB to Manager: " + Manager);
                
                _moveBehaviour.OnPathFound = () =>
                {
                    Manager.Animations.Walk();
                };
            }

            if (!_moveBehaviour) Debug.LogError("[MoveTo] Could not create move behaviour");

            if (_moveBehaviour.Finished)
            {
                var data = ctx.Structure.StructureData;
                var center = ctx.Structure.MonkeyWorkingArea;
                var b = new Bounds(center, data.worldSize.ToFloat() / 2f);
                PathManager.FindPathAsync(Manager.transform.position, b, _moveBehaviour);    
            }
            
            var goingLeft = _moveBehaviour.targetArea.center.x < Manager.transform.position.x;
            Manager.Sprite.flipX = goingLeft ? !Manager.IsMichelJackson : Manager.IsMichelJackson;

            if (_moveBehaviour.Fetching)
                Manager.Animations.Wandering();
            
            return _moveBehaviour && !_moveBehaviour.Finished ? NodeResult.Running : NodeResult.Success;
        }
    }

    public class MonkeyMovingAB : MonoBehaviour
    {
        public BaseStructure Destination { get; set; }
        public float Speed { get; set; } = 4f;

        //replace with nav mesh 
        private Rigidbody2D _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (Finished) return;

            var dir = Destination.MonkeyWorkingArea - _rigidbody.transform.position;
            dir.Normalize();

            _rigidbody.MovePosition(transform.position + dir * (Speed * Time.fixedDeltaTime * VillageManager.Speed));
        }

        private void LateUpdate()
        {
            if (Finished) Destroy(this);
        }

        public bool Finished => didStart && Destination.InWorkingArea(_rigidbody.position);
    }
}
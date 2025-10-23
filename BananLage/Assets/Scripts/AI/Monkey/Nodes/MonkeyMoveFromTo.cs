using System;
using Behaviour_Tree;
using Mechanics;
using Structures;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.Monkey.Nodes
{
    public class MonkeyMoveTo : LeafNode<MonkeyCharacterBT>
    {
        public static float Leniency => 0.05f;
        
        private MonkeyMovingAB _moveBehaviour;
        public override NodeResult Process()
        {
            var ctx = Manager.CurrentJob;
            if (ctx == null) return NodeResult.Failure;
            
            Debug.Log($"[MoveTo] Has Job Context:{ctx}...");
            
            var str = ctx.Structure;
            if (!str)
            {
                Debug.Log($"[MoveTo] Has No Structure, Bail Out");
                return NodeResult.Failure;
            }
            
            if (str.InWorkingArea(Manager.transform.position))
                return NodeResult.Failure;
            
            var wasNull = !_moveBehaviour;
            if (wasNull)
            {
                _moveBehaviour = Manager.gameObject.AddComponent<MonkeyMovingAB>();
                if (!_moveBehaviour) throw new Exception("Something went wrong => cannot add MonkeyMovingAB to Manager: " + Manager);
                _moveBehaviour.Destination = str;
            }
            
            if (!_moveBehaviour) Debug.LogError("[MoveTo] Could not create move behaviour");
            
            return _moveBehaviour && !_moveBehaviour.Finished ? NodeResult.Success : NodeResult.Running;
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
            
            _rigidbody.MovePosition(transform.position + dir * (Speed * Time.fixedDeltaTime));
        }

        private void LateUpdate()
        {
            if (Finished) Destroy(this);
        }

        public bool Finished => didStart && Destination.InWorkingArea(_rigidbody.position);
    }
}
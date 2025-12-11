using System;
using System.Linq;
using Mechanics;
using Mechanics.Village;
using Structures;
using UnityEngine;
using UnityEngine.Assertions;

namespace AI.Monkey
{
    public class MonkeyAnimationController : MonoBehaviour
    {
        private static readonly int VillageSpeed = Animator.StringToHash("VillageSpeed");
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip defaultState, walk, build, rest, idling, farm, gather, wandering, mining, dead;

        public void Default() => _PlayClip(defaultState);
        public void Walk() => _PlayClip(walk);
        public void Build() => _PlayClip(build);
        public void Rest() => _PlayClip(rest);
        public void Idling() => _PlayClip(idling);
        public void Farm() => _PlayClip(farm);
        public void Gather() => _PlayClip(gather);
        public void Wandering() => _PlayClip(wandering);
        
        public void Mining() => _PlayClip(mining);

        private void LateUpdate()
        {
            animator.SetFloat(VillageSpeed, VillageManager.Speed);
        }

        private void _PlayClip(AnimationClip clip)
        {
            if (!clip)
            {
                Debug.LogWarning("[MonkeyAnimationController] Animation clip is null]");
                return;
            }

            var current = animator.GetCurrentAnimatorClipInfo(0);
            if (current.Any(e => e.clip.name == clip.name)) return;

            animator.Play(clip.name);
        }

        public void PlayTask(TaskType taskType)
        {
            switch (taskType)
            {
                case TaskType.Combat: break;
                case TaskType.Build: Build(); break;
                case TaskType.Farm: Farm(); break;
                case TaskType.Gather: Gather(); break;
                case TaskType.Mate: break;
                case TaskType.Idle: Idling(); break;
                case TaskType.Rest: Rest(); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null);
            }
        }

        public void PlayTask(TaskAnimation structureDataTaskAnimation, TaskType taskType)
        {
            switch (structureDataTaskAnimation)
            {
                case TaskAnimation.Default:
                    PlayTask(taskType);
                    break;
                case TaskAnimation.Mining:
                    Mining();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(structureDataTaskAnimation), structureDataTaskAnimation, null);
            }
        }

        public void PlayDead()
        {
            _PlayClip(dead);
        }
    }
}
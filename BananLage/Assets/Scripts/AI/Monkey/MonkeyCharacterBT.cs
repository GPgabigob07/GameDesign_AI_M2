using AI.Monkey.Job;
using AI.Monkey.Nodes;
using Behaviour_Tree;
using Behaviour_Tree.Nodes;
using JetBrains.Annotations;
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Village;
using UnityEngine;

namespace AI.Monkey
{
    public class MonkeyCharacterBT : BehaviourTreeManager<MonkeyCharacterBT>
    {
        public override Node<MonkeyCharacterBT> Root { get; protected set; } = new MonkeyAI();

        [field: SerializeField] public MonkeyData CycleData { get; set; }
        [field: SerializeField] public MonkeyData OriginalData { get; set; }

        protected override void Start()
        {
            base.Start();
            if (CycleData != null)
            {
                CycleData.Self = this;
                VillageManager.RegisterWorker(CycleData);
            }
        }

        [CanBeNull] private JobContext _currentJob;
        [CanBeNull]
        public JobContext CurrentJob
        {
            get => _currentJob;
            set
            {
                if (debug)
                {
                    Debug.Log($"[Monkey: {CycleData.Name}] chose new job: {value}");
                }
                
                _currentJob = value;
            }
        }
    }

    public sealed class MonkeyAI : SequenceNode<MonkeyCharacterBT>
    {
        protected override void CreateChildren()
        {
            AddChild(MonkeyBooleans.IsDead().Invert());
            //combat checks, as it's priority

            AddChild(MonkeyBooleans.CanMonkeyWork());
            AddChild(new MonkeyJobSelectorNode());
            AddChild(new MonkeyCollectOutputNode());
        }
    }
}
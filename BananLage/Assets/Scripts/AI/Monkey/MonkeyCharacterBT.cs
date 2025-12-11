using System.Collections.Generic;
using AI.Monkey.Job;
using AI.Monkey.Nodes;
using Behaviour_Tree;
using Behaviour_Tree.Nodes;
using JetBrains.Annotations;
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Jobs.Types;
using Mechanics.PathFinding;
using Mechanics.Village;
using UnityEngine;
using UnityEngine.Assertions;

namespace AI.Monkey
{
    public class MonkeyCharacterBT : BehaviourTreeManager<MonkeyCharacterBT>, IJobContainer
    {
        public override Node<MonkeyCharacterBT> Root { get; protected set; } = new MonkeyAI();

        [field: SerializeField] public MonkeyData CycleData { get; set; }
        [field: SerializeField] public MonkeyData OriginalData { get; set; }
        [field: SerializeField] public SpriteRenderer Sprite { get; private set; }
        [field: SerializeField] public MonkeyAnimationController Animations { get; private set; }
        [field: SerializeField] public PathAgent Agent { get; private set; }
        public bool IsResting => CurrentJob is RestingJobContext job && job.IsMonkeyResting(this); 
        [field: SerializeField] public bool PutToRest { get; set; }

        protected override void Start()
        {
            base.Start();
            Assert.IsNotNull(OriginalData);
            OriginalData.Self = this;
            
            CycleData = OriginalData.Clone();

            name = OriginalData.Name;
            Agent = GetComponent<PathAgent>();
            VillageManager.RegisterWorker(CycleData);
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
                
                if (value is null) 
                    Animations.Default();
                
                _currentJob = value;
            }
        }

        JobContext IJobContainer.JobContext => _currentJob;
        public bool IsMichelJackson => CycleData.Name == "Michel Jackson";
        public bool GoDie { get; set; }
    }

    public sealed class MonkeyAI : SequenceNode<MonkeyCharacterBT>
    {
        protected override void CreateChildren()
        {
            //combat checks, as it's priority
            //no combat :(

            AddChild(new MonkeyGoingToDie().Invert());
            AddChild(new MonkeyJobSelectorNode());
            AddChild(new MonkeyCollectOutputNode());
        }
    }
}
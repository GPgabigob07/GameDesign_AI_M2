using System.Collections.Generic;
using System.Linq;
using AI.Monkey.Nodes;
using Mechanics.Village;
using Structures;
using Structures.Types;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mechanics.Jobs.Types
{
    public class BuildJobContext: JobContext
    {
        #if UNITY_EDITOR
        public float _progressInternal = 0f;
        #else
        private  float _progressInternal = 0f;  
        #endif
        
        public float Progress => Mathf.Clamp01(_progressInternal / Structure.StructureData.TotalBuildTime);
        private List<MonkeyData> Monkeys { get; set; }
        public int RemainingAVCost { get; private set; }

        public BuildJobContext(MonkeyData monkey, BaseStructure structure, TaskType taskType) : base(monkey, structure, taskType)
        {
            Monkeys = new List<MonkeyData> { monkey };
            _progressInternal = 0;
        }

        protected override void OnBegin()
        {
            RemainingAVCost = Structure.StructureData.buildAV;
        }

        protected override void OnTick()
        {
            var last = Step();
            _progressInternal += VillageManager.ComputeBuildingProgressFor(this);

            if (Step() > last)
            {
                //deduce costs
                RemainingAVCost -= Monkeys.Count;
                Monkeys.ForEach(e => e.ActionValue--);
            }
            
            IsFinished = RemainingAVCost <= 0;
        }

        public void Deconstruct(out List<MonkeyData> monkeys, out BuildStructureController structure)
        {
            monkeys = Monkeys;
            structure = Structure as BuildStructureController;
        }

        public void AddMonkey(MonkeyData monkey)
        {
            if (Monkeys.All(e => e.UUID != monkey.UUID))
            {
                Monkeys.Add(monkey);
            }
        }

        protected override void OnFinish()
        {
            var structure = Structure as BuildStructureController;
            structure.FinishBuild();
            
            Monkeys.ForEach(e => e.Self.CurrentJob = null);
        }

        public int Step()
        {
            return (_progressInternal / Structure.StructureData.buildTimePerAv).Floor();
        }
    }
}
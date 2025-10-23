using System.Collections.Generic;
using System.Linq;
using Mechanics.Production;
using Mechanics.Village;
using Structures;
using Structures.Types;

namespace Mechanics.Jobs.Types
{
    public class ProductionJobContext: JobContext
    {
        public List<ResourceOutput> Output { get; private set; }
        
        public ProductionJobContext(MonkeyData monkey, StructureController<ProductionJobContext> structure, TaskType taskType) : base(monkey, structure, taskType)
        {
            
        }
        
        public void AddOutput(ResourceOutput output) => (Output ??= new List<ResourceOutput>()).Add(output);

        protected override void OnBegin()
        {
            JobLibrary.FillOutputs(this);
            if (Structure is YieldStructureController yielder)
                yielder.NotifyJobProgress();
        }

        protected override void OnFinish()
        {
            if (Structure is YieldStructureController yielder)
                yielder.NotifyJobProgress();
        }

        protected override void OnTick()
        {
            VillageManager.ComputeProgressForJob(this);
            IsFinished = Output.Aggregate(true, (acc, e) => acc && e.Progress >= 100);
        }
    }
}
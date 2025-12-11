using System.Collections.Generic;
using System.Linq;
using Mechanics.Village;
using Structures;
using Structures.Types;
using UnityEngine;

namespace Mechanics.Jobs.Types
{
    public class BuildJobContext : MultiWorkerJobContext
    {
        public float _progressInternal = 0f;

        public float Progress => Mathf.Clamp01(_progressInternal / _config.aVCost);
        public int RemainingAVCost { get; private set; }

        private CycleConfiguration _config;

        private List<MonkeyData> _monkeysToRemove = new List<MonkeyData>();

        public BuildJobContext(BaseStructure structure) : base(structure, TaskType.Build)
        {
            _config = structure.StructureData.buildConfiguration;
            _progressInternal = 0;
        }

        protected override void OnBegin()
        {
            RemainingAVCost = _config.aVCost;
            foreach (var m in Monkeys.Values)
            {
                m.Self.Animations.Build();
            }
        }

        protected override void OnTick()
        {
            PlaySounds();
            _progressInternal += VillageManager.ComputeBuildingProgressFor(this);

            if (VillageManager.ImmediateResolve)
            {
                foreach (var monkeyData in Monkeys.Values.ToList())
                {
                    FireMonkey(monkeyData);
                }
            }
            else
            {
                _monkeysToRemove.AddRange(
                    Monkeys.Values
                        .Where(e => WorkersTime.ContainsKey(e.Id) && (WorkersTime[e.Id] -= Time.deltaTime) <= 0));
                
                _monkeysToRemove.ForEach(FireMonkey);
                _monkeysToRemove.Clear();
                
                foreach (var (_, m) in Monkeys)
                {
                    if (Structure.InWorkingArea(m.Self.transform.position))
                        m.Self.Animations.Build();
                }
            }


            IsFinished = Progress >= 1;
        }


        private float soundDebunce = 0f;
        private void PlaySounds()
        {
            if ((soundDebunce -= Time.deltaTime) <= 0)
            {
                var sounds = SoundEngine.bus.Sounds;

                var sound = Monkeys.Count switch
                {
                    1 => sounds.singleMonkeyBuild,
                    _ => sounds.multiMonkeyBuild
                };

                soundDebunce = sound.length;
                SoundEngine.PlaySFX(sound, Structure.transform, d3: true);
            }
        }

        private void FireMonkey(MonkeyData monkeyData)
        {
            //TODO Validate this
            Debug.Log($"[Job] Firing {monkeyData.Id}: {monkeyData.Name}");
            var consumed =
                VillageManager.ConsumeActionValue(monkeyData, GetAvailableAvFor(monkeyData), TaskType);
            WorkerAVs[monkeyData.Id] -= consumed;
            ReleaseMonkey(monkeyData);
        }

        public void Deconstruct(out List<MonkeyData> monkeys, out BuildStructureController structure,
            out CycleConfiguration config)
        {
            monkeys = Monkeys.Values.ToList();
            structure = Structure as BuildStructureController;
            config = _config;
        }

        protected override void OnFinish()
        {
            var structure = Structure as BuildStructureController;
            structure?.FinishBuild();
            SoundEngine.PlaySFX(SoundEngine.bus.Sounds.sfxFinishBuild, Structure.transform);
        }

        public int Step()
        {
            return Mathf.FloorToInt(_progressInternal);
        }

        public override bool HasSpace => Monkeys.Count < _config.maxConcurrentWorkers;
        protected override int AvPerWorker => _config.maxAvPerWorker;

        protected override void OnMonkeyAdded(MonkeyData monkey, int remainingAv)
        {
            WorkersTime[monkey.Id] = _config.timePerAv * remainingAv;
        }
    }
}
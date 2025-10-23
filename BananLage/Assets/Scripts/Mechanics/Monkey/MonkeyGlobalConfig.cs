using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Village;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    [CreateAssetMenu(menuName = "Create MonkeyGlobalConfig", fileName = "MonkeyGlobalConfig", order = 0)]
    public class MonkeyGlobalConfig : ScriptableObject
    {
        public bool debug = false;
        public float ProwessBoostUnit = 0.5f;
        public float AgeProwessBoostUnit = 0.75f;
        public float MinimalCost = 0;
        
        [Tooltip("% per prowess point")]
        public float NominalOutputBoost = 0.025f;
        public int startProwessPoints = 10;

        [Range(0f, 1f)] public float GenderRatio = 0.6f;

        [Header("Male Config")] public MonkeyGenderConfiguration MaleConfiguration = new() {
            likelyProwess = new()
            {
                Build = 3,
                Combat = 3,
                Farm = 2,
                Gather = 1,
                Appeal = 1,
            },
            minMatingCycles = 3,
            maxMatingCycles = 5,
            minStartHp = 5,
            maxStartHp = 10,
            baseActionValue = 6
        };

        [Header("Female Config")] public MonkeyGenderConfiguration FemaleConfiguration = new() {
            likelyProwess = new()
            {
                Build = 1,
                Combat = 1,
                Farm = 2,
                Gather = 4,
                Appeal = 3,
            },
            minMatingCycles = 3,
            maxMatingCycles = 5,
            minStartHp = 5,
            maxStartHp = 10,
            baseActionValue = 5
        };

        [Header("Prowess Configuration")] public MonkeyProwess NoExecutionCostProwess = new()
        {
            Combat = 100,
            Build = 90,
            Farm = 90,
            Gather = 80,
            Appeal = 100,
        };

        [SerializeField] private List<ProwessProductionAmplification> amplificationConfig = new();

        public Dictionary<TaskType, List<PPA_Ranges>> taskAmpRange = new();

        private void OnValidate()
        {
            GenderRatio = Mathf.Clamp(GenderRatio, 0.1f, 0.9f);

            (taskAmpRange ??= new Dictionary<TaskType, List<PPA_Ranges>>()).Clear();
            
            taskAmpRange = amplificationConfig
                .Select<ProwessProductionAmplification, (TaskType, List<PPA_Ranges>)>(e => (e.task, e.ranges))
                .ToDictionary(e => e.Item1, e => e.Item2);
        }

        public MonkeyData CreateMonkey(int points)
        {
            var gender = Random.value <= GenderRatio ? MonkeyGender.M : MonkeyGender.F;
            var config = gender == MonkeyGender.M ? MaleConfiguration : FemaleConfiguration;

            return new MonkeyData
            {
                Gender = gender,
                MatingCycle = config.NewCycle(),
                Prowess = config.NewProwess(points),
                ActionValue = config.baseActionValue,
                Hp = config.NewHp(),
                Name = gender.NewName(),
                Age = 1,
                UUID = Guid.NewGuid(),
            };
        }
    }
}
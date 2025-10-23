using System;
using System.Collections.Generic;
using AI.Monkey;
using JetBrains.Annotations;
using UnityEngine;

namespace Mechanics
{
    public static class MonkeyUtils
    {

    }

    [Serializable]
    public class MonkeyData
    {
        [Header("Basic data")]
        public int Hp;
        public int Age;
        public int ActionValue;
        public string Name;
        public int MatingCycle;
        public Guid UUID;
        public MonkeyProwess Prowess;
        public MonkeyGender Gender;
        
        public MonkeyCharacterBT Self {get;set;}

        public MonkeyData()
        {

        }

        [Header("Task data")]
        public TaskType? CurrentTask = null;
        
        public bool CanPerform(TaskType task) => !Prowess.Equals(null) &&  Prowess.Can(task);
        public void Enable(TaskType task, bool enable) => Prowess.Allow(task, enable);

        public bool IsMale => Gender == MonkeyGender.M;
        public bool IsFemale => Gender == MonkeyGender.F;

        public TaskType MostProwessTask => Prowess.Most();
        public TaskType LeastProwessTask => Prowess.Least();

        public override string ToString()
        {
            return
                $"{nameof(Hp)}: {Hp}, {nameof(Age)}: {Age}, {nameof(ActionValue)}: {ActionValue}, {nameof(Name)}: {Name}, {nameof(UUID)}: {UUID}, {nameof(Prowess)}: {Prowess}, {nameof(Gender)}: {Gender}, {nameof(MatingCycle)}: {MatingCycle}, {nameof(IsMale)}: {IsMale}, {nameof(IsFemale)}: {IsFemale}, {nameof(MostProwessTask)}: {MostProwessTask}, {nameof(LeastProwessTask)}: {LeastProwessTask}";
        }
    }

    public enum TaskType
    {
        Combat,
        Build,
        Farm,
        Gather,
        Mate
    }

    public enum MonkeyGender
    {
        M,
        F
    }
}
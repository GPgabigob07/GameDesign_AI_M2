using System;
using AI.Monkey;
using Structures;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Mechanics
{
    public static class MonkeyUtils
    {
    }

    [Serializable]
    public class MonkeyData
    {
        [Header("Basic data")] public int Hp;
        public int Age;
        public int ActionValue;
        public string Name;
        public int MatingCycle;
        public string Id;
        public MonkeyProwess Prowess;
        public MonkeyGender Gender;

        public MonkeyCharacterBT Self { get; set; }

        public MonkeyData()
        {
        }

        [Header("Task data")] public TaskType CurrentTask = TaskType.Idle;

        public BaseStructure lastWorkedStructure;

        public bool CanPerform(TaskType task) => !Prowess.Equals(null) && Prowess.Can(task);
        public void Enable(TaskType task, bool enable) => Prowess.Allow(task, enable);

        public bool IsMale => Gender == MonkeyGender.M;
        public bool IsFemale => Gender == MonkeyGender.F;

        public TaskType MostProwessTask => Prowess.Most();
        public TaskType LeastProwessTask => Prowess.Least();

        public override string ToString()
        {
            return
                $"{nameof(Hp)}: {Hp}, {nameof(Age)}: {Age}, {nameof(ActionValue)}: {ActionValue}, {nameof(Name)}: {Name}, {nameof(Id)}: {Id}, {nameof(Prowess)}: {Prowess}, {nameof(Gender)}: {Gender}, {nameof(MatingCycle)}: {MatingCycle}, {nameof(IsMale)}: {IsMale}, {nameof(IsFemale)}: {IsFemale}, {nameof(MostProwessTask)}: {MostProwessTask}, {nameof(LeastProwessTask)}: {LeastProwessTask}";
        }

        public MonkeyData Refresh(MonkeyData cycleData)
        {
            return new MonkeyData
            {
                Hp = cycleData.Hp,
                Age = cycleData.Age + 1,
                ActionValue = ActionValue, //consume bananas? + increase with prowess?
                CurrentTask = TaskType.Idle,
                Gender = Gender,
                lastWorkedStructure = cycleData.lastWorkedStructure,
                Prowess = cycleData.Prowess.Refresh(),
                MatingCycle = MatingCycle,
                Self = cycleData.Self,
                Name = Name,
                Id = Id
            };
        }

        public MonkeyData Clone()
        {
            return new MonkeyData
            {
                Hp = Hp,
                Age = Age,
                ActionValue = ActionValue,
                CurrentTask = CurrentTask,
                Gender = Gender,
                lastWorkedStructure = lastWorkedStructure,
                Prowess = Prowess.Clone(),
                MatingCycle = MatingCycle,
                Self = Self,
                Name = Name,
                Id = Id,
            };
        }
    }

    public enum TaskType
    {
        Combat,
        Build,
        Farm,
        Gather,
        Mate,
        Idle,
        Rest
    }

    public enum MonkeyGender
    {
        M,
        F
    }
}
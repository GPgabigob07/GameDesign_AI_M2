using System;
using UnityEngine;

namespace Mechanics
{
    public class CycleController: MonoBehaviour
    {
        private static CycleController instance;
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            
            instance ??= this;
            DontDestroyOnLoad(this);
        }
        
        public int CurrentCycle { get; private set; } = 0;
        public bool IsCycleRunning { get; private set; } = false;
        public float CycleSpeed { get; private set; } = 1f;


        public static void SkipCycle() => instance._SkipCycle();
        
        private void _SkipCycle()
        {
            IsCycleRunning = false;
            //computar todas as ações necessárias imediatamente
            //ou
            //encerrar imediatamente as ações
            NextCycle();
            IsCycleRunning = true;
            
        }

        private void NextCycle()
        {
            CurrentCycle++;
        }
    }
}
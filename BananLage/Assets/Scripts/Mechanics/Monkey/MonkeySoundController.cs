using System;
using AI.Monkey;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics.Monkey
{
    public class MonkeySoundController: MonoBehaviour
    {
        [SerializeField] private MonkeyCharacterBT manager;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private float minOffset, offsetRange;
        private float emissionTime = 0f;
        
        

        private void Update()
        {
            if (manager.CycleData.CurrentTask == TaskType.Rest)
            {
                NewRange();
                return;
            }

            if ((emissionTime -= Time.deltaTime) <= 0 && !audioSource.isPlaying)
            {
                audioSource.Play();
                NewRange();
            }
        }

        private void NewRange()
        {
            emissionTime = minOffset + Mathf.Lerp(-offsetRange, offsetRange, Random.value);
        }
    }
}
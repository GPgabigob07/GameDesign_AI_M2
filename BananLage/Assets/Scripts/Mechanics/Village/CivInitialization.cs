using System;
using System.Collections;
using Structures;
using UnityEngine;

namespace Mechanics.Village
{
    public class CivInitialization : MonoBehaviour
    {
        private BaseStructure parent;
        public int minPairs = 2;
        public MonkeyGlobalConfig config;

        private IEnumerator Start()
        {
            parent = GetComponent<BaseStructure>();

            var total = minPairs * 2;
            var ancestors = new MonkeyData[total];
            for (var i = 0; i < total; i++)
            {
                ancestors[i] = config.CreateMonkey(15, (MonkeyGender)(i % 2));
            }

            yield return new WaitForSeconds(1f);
            VillageManager.InstantiateNewMonkeys(parent, ancestors);
            VillageManager.VillageInventory["Banana"] = total * 5;
            
            Destroy(this);
        }
    }
}
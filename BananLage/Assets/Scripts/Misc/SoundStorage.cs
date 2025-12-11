using UnityEngine;

namespace Misc
{
    [CreateAssetMenu(fileName = "SoundStorage", menuName = "Misc/SoundStorage")]
    public class SoundStorage: ScriptableObject
    {
        public AudioClip uiClick,
            singleMonkeyBuild,
            multiMonkeyBuild,
            sfxGather,
            sfxMining,
            sfxSelectWorldObj,
            sfxFinishBuild;
    }
}
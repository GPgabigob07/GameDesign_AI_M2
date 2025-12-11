using System;
using System.Diagnostics.Tracing;
using Misc;
using UnityEngine;
using UnityEngine.Audio;

public class SoundEngine : MonoBehaviour
{
    public static SoundEngine bus;
    public static float sfxVolume = 1, musicVolume = 1;

    [field: SerializeField] public SoundStorage Sounds { get; private set; }
    private void Awake()
    {
        if (bus == null)
        {
            bus = this;
            DontDestroyOnLoad(bus);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    [SerializeField] private AudioSource music, sfx;

    public static AudioSource PlaySFX(AudioResource clip, Transform positionRef, float volume = 1, bool d3 = false)
    {
        return !bus ? null : PlaySound(bus.sfx, clip, positionRef, volume * sfxVolume, randomPitch: false, d3: d3);
    }

    public static AudioSource PlayMusic(AudioResource clip, Transform positionRef, float volume = 1f)
    {
        return !bus ? null : PlaySound(bus.music, clip, positionRef, volume, loop: true);
    }

    private static AudioSource PlaySound(AudioSource audioSource, AudioResource clip, Transform position, float volume = 1f,
        bool randomPitch = false, bool loop = false, bool d3 = false)
    {
        if (!clip) return null;
        
        var source = Instantiate(audioSource, position.position, Quaternion.identity);
        if (!source) return null;
        
        source.pitch = randomPitch ? UnityEngine.Random.Range(.85f, 1.15f) : 1;

        source.resource = clip;
        source.volume = volume;

        if (d3)
        {
            source.spatialBlend = 1;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 3;
            source.maxDistance = 6;
        }

        source.Play();

        if (!loop)
        {
            Destroy(source.gameObject, audioSource.clip ? source.clip.length : 5f);
        }
        else
        {
            source.loop = true;
        }
        
        return source;
    }
    
    
}
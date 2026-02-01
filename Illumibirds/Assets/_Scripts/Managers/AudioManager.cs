using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private Transform _sfxParent;

    // Music
    [SerializeField] private List<AudioPlayer> musicPlayer;
    [SerializeField] private float fadeAmount = 1;
    private MusicTrack nextMusic;

    // Pitch
    [SerializeField] private float pitchIncrease = 0.1f;
    [SerializeField] private float randomPitchRange = 0.2f;
    [SerializeField] private float pitchResetTime = 1f;
    private float stdPitch = 1;
    private float currentPitch = 1;
    private FixedTimer pitchTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        pitchTimer = new();
    }

    private void Update()
    {
        if (pitchTimer.GetTime() > pitchResetTime)
        {
            currentPitch = stdPitch;
        }

        foreach (var player in musicPlayer)
        {
            player.Update();
        }
        
    }

    public void PlayMusic(MusicTrack audioClip)
    {
        nextMusic = audioClip;
        for (int i = 0; i < musicPlayer.Count; i++)
        {
            musicPlayer[i].PlayMusic(nextMusic, i, i != 0); // All but first layer silent
        }
    }

    public void FadeInLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= musicPlayer.Count) return;
        musicPlayer[layerIndex].currentMusicTargetVolume = nextMusic.volumes[layerIndex];
        musicPlayer[layerIndex].fadeIn = true;
    }

    public void FadeOutLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= musicPlayer.Count) return;
        musicPlayer[layerIndex].currentMusicTargetVolume = 0;
        musicPlayer[layerIndex].fadeOut = true;
    }

    public void PlayOncePitched(AudioClip clip, float volume)
    {
        PlayOnce(clip, volume, currentPitch);
        currentPitch += pitchIncrease;
        pitchTimer.Start();
    }

    public void PlayOncePitchedRandom(AudioClip clip, float volume)
    {
        float pitch = UnityEngine.Random.Range(stdPitch - randomPitchRange, stdPitch + randomPitchRange);
        PlayOnce(clip, volume, pitch);
    }

    public void PlayOnce(AudioClip clip, float volume)
    {
        PlayOnce(clip, volume, stdPitch);
    }

    public void PlayOnce(AudioClip clip, float volume, float pitch)
    {
        GameObject sfx = new GameObject("SFX");
        sfx.transform.parent = _sfxParent;
        AudioSource source = sfx.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        float destructionTime = clip.length + 0.1f;
        StartCoroutine(DestroyAudioSource(sfx, destructionTime));
    }

    private IEnumerator DestroyAudioSource(GameObject audioSouce, float destructionTime)
    {
        yield return new WaitForSeconds(destructionTime);
        Destroy(audioSouce);
    }
}

[Serializable]
public class MusicTrack
{
    public List<AudioClip> clipLayers;
    public List<float> volumes;

    public MusicTrack(List<AudioClip> _clips, List<float> _volumes)
    {
        clipLayers = _clips;
        volumes = _volumes;
    }
}

[Serializable]
public class AudioPlayer
{
    public AudioSource musicPlayer;
    public AudioClip nextMusic;
    public float currentMusicTargetVolume = 0;
    private float nextMusicTargetVolume = 0;
    public bool fadeIn = false;
    public bool fadeOut = false;
    public float fadeAmount = 1;

    public void Update() {
        if (fadeIn && musicPlayer.volume < currentMusicTargetVolume)
        {
            musicPlayer.volume += fadeAmount * Time.deltaTime;
        }
        else if (fadeIn)
        {
            fadeIn = false;
        }

        if (fadeOut && musicPlayer.volume > 0)
        {
            musicPlayer.volume -= fadeAmount * Time.deltaTime;
        }
        else if (fadeOut)
        {
            fadeOut = false;
            fadeIn = true;
            currentMusicTargetVolume = nextMusicTargetVolume;
        }
    }
    
    public void PlayMusic(MusicTrack musicTrack, int index, bool isSilent = false)
    {
        if (musicPlayer.clip == null)
        {
            // No music is playing, start new music immediately
            musicPlayer.clip = musicTrack.clipLayers[index];
            musicPlayer.volume = 0;
            currentMusicTargetVolume = isSilent ? 0 : musicTrack.volumes[index];
            musicPlayer.Play();
            fadeIn = true;
        }
        else
        {
            // Music is playing, fade out current and prepare next
            nextMusic = musicTrack.clipLayers[index]; // Single layer
            nextMusicTargetVolume = isSilent ? 0 : musicTrack.volumes[index];
            fadeOut = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour, IManager
{
    public static AudioManager Instance;
    [SerializeField] private Transform _sfxParent;

    // Music
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private float fadeAmount = 1;
    private AudioClip nextMusic;

    private float currentMusicTargetVolume;
    private float nextMusicTargetVolume;
    private bool fadeIn = false;
    private bool fadeOut = false;

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
            musicPlayer.Stop();
            musicPlayer.clip = nextMusic;
            currentMusicTargetVolume = nextMusicTargetVolume;
            musicPlayer.Play();
        }
    }

    public void PlayMusic(AudioClip audioClip, float volume)
    {
        nextMusic = audioClip;
        nextMusicTargetVolume = volume;
        fadeOut = true;
    }

    public void PlayOncePitched(AudioClip clip, float volume)
    {
        PlayOnce(clip, volume, currentPitch);
        currentPitch += pitchIncrease;
        pitchTimer.Start();
    }

    public void PlayOncePitchedRandom(AudioClip clip, float volume)
    {
        float pitch = Random.Range(stdPitch + randomPitchRange, stdPitch - randomPitchRange);
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

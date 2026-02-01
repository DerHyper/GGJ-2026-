using System.Collections.Generic;
using UnityEngine;

public class AudioManagerTest : MonoBehaviour
{
    public MusicTrack track;
    public void StartMusicTest()
    {
        AudioManager.Instance.PlayMusic(track);
    }

    public void FadeInLayer(int index)
    {
        AudioManager.Instance.FadeInLayer(index);
    }

    public void FadeOutLayer(int index)
    {
        AudioManager.Instance.FadeOutLayer(index);
    }
}

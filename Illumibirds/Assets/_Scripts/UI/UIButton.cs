using System;
using UnityEngine;


public class UIButton : MonoBehaviour
{
    [SerializeField] AudioClip hoverSound, clickSound;

    public void PlayHoverSound()
    {
        if(TryGetComponent<AudioSource>(out AudioSource source))
        {
            if(hoverSound)
            source.PlayOneShot(hoverSound);
        }
    }

    
    public void PlayClickSound()
    {
        if(TryGetComponent<AudioSource>(out AudioSource source))
        {
            if(clickSound)
            source.PlayOneShot(clickSound);
        }
    }
}

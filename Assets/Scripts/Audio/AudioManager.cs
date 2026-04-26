using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource audioSource;

    public AudioClip menu;
    public AudioClip normalLvl;
    public AudioClip normalFinalLvl;
    public AudioClip hardLvl;
    public AudioClip hardFinalLvl;
    public AudioClip winLvl;
    public AudioClip loseLvl;

    public void PlayMenu()           => PlayClip(menu);
    public void PlayNormalLvl()      => PlayClip(normalLvl);
    public void PlayNormalFinalLvl() => PlayClip(normalFinalLvl);
    public void PlayHardLvl()        => PlayClip(hardLvl);
    public void PlayHardFinalLvl()   => PlayClip(hardFinalLvl);
    public void PlayWinLvl()         => PlayClip(winLvl);
    public void PlayLoseLvl()        => PlayClip(loseLvl);

    // cambia a la musica "final" segun si el nivel es hard o no
    public void PlayFinalLvl(bool isHard)
    {
        if (isHard) PlayHardFinalLvl();
        else        PlayNormalFinalLvl();
    }


    void PlayClip(AudioClip clip)
    {
        audioSource.Pause();
        audioSource.clip = clip;
        audioSource.Play();
    }
}

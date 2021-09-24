using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingMenuVolume : MonoBehaviour
{
    public AudioMixer audio1;
    public AudioMixer audio2;
    public void SetVol(float volume)
    {
        audio1.SetFloat("Volume",volume);
    }
    public void SetVolMusic(float volume)
    {
        audio2.SetFloat("Volume", volume);
    }
}

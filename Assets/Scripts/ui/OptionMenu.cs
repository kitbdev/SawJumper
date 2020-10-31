using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public struct Settings {
    public float musicVol;
    public float sfxVol;
    public bool camAuto;
    public bool fullscreen;
    public bool invertY;
}
public class OptionMenu : MonoBehaviour {

    public Slider musicVolSlider;
    public Slider sfxVolSlider;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    Settings settings;
    public bool mute = false;

    void Start() {

    }
    public void SetMusicVol(float volume) {
        volume = volume * 100 - 80;
        // settings.musicVol = volume;
        // also actually set it
        musicGroup.audioMixer.SetFloat("musicVol", volume);
    }
    public void SetSfxVol(float volume) {
        volume = volume * 100 - 80;
        sfxGroup.audioMixer.SetFloat("sfxVol", volume);
    }
    public void Mute() {
        mute = !mute;
        if (mute) {
            sfxGroup.audioMixer.SetFloat("sfxVol", -80);
            musicGroup.audioMixer.SetFloat("musicVol", -80);
        } else {
            sfxGroup.audioMixer.SetFloat("sfxVol", 0);
            musicGroup.audioMixer.SetFloat("musicVol", 0);
        }
    }
    public void SetCamAuto(bool enable) {
        // todo pass values to gamemanager somehow
        // have a global options ddd obj
        // comes with menu? - that can be a prefab
        // xtemp ddd object?
        // todo also want this function for the pause menu..
        // can we share the option menu?
    }
    public void Backbtn() {}
}
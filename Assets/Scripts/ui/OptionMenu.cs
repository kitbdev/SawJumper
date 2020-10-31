using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct Settings {
    public float musicVol;
    public float sfxVol;
    public bool camAuto;
    public bool fullscreen;
    public bool invertY;
}
public class OptionMenu : MonoBehaviour {

    Settings settings;

    void Start() {

    }
    public void SetMusicVol(float volume) {
        volume = Mathf.Clamp(volume,0,2);
        settings.musicVol = volume;
        // also actually set it
    }
    public void SetSfxVol(float volume) {

    }
    public void Mute() {
        
    }
    public void SetCamAuto(bool enable) {
        // todo pass values to gamemanager somehow
        // have a global options ddd obj
        // comes with menu? - that can be a prefab
        // xtemp ddd object?
        // todo also want this function for the pause menu..
        // can we share the option menu?
    }
    public void Exitbtn() {
        Application.Quit();
    }
}
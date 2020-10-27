using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    public GameObject pauseMenu;
    public bool paused = false;

    private void Awake() {
        SetPaused(paused);
    }
    private void Update() {
        if (Keyboard.current.mKey.wasPressedThisFrame) {
            TogglePaused();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            SetPaused(true);
        }
        if (Keyboard.current.pKey.wasPressedThisFrame) {
            SetPaused(false);
        }
    }

    public void TogglePaused() {
        SetPaused(!paused);
    }
    public void SetPaused(bool enabled = true) {
        paused = enabled;
        if (paused) {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseMenu.SetActive(true);
        } else {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pauseMenu.SetActive(false);
        }
    }

}
﻿using System.Collections;
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
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            TogglePaused();
        }
        if (Keyboard.current.mKey.wasPressedThisFrame) {
            SetPaused(false);
        }
        if (Keyboard.current.pKey.wasPressedThisFrame) {
            SetPaused(true);
        }
    }

    public void TogglePaused() {
        SetPaused(!paused);
    }
    public void SetPaused(bool enabled = true) {
        paused = enabled;
        if (paused) {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
        } else {
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            pauseMenu.SetActive(false);
        }
    }

}
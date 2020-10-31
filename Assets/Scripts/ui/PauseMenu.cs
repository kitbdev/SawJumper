using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    public GameObject pauseMenu;
    public GameObject skipButton;
    public bool paused = false;

    public InputActionReference pausebtn;
    // GameManager gm;

    void Awake() {
        pausebtn.action.Enable();
        pausebtn.action.performed += c => TogglePaused();
        // pausebtn.action.canceled += c => ();
        SetPaused(paused);
        skipButton.SetActive(false);
    }
    private void Update() {
        // if (Keyboard.current.escapeKey.wasPressedThisFrame) {
        //     TogglePaused();
        // }
        // if (Keyboard.current.mKey.wasPressedThisFrame) {
        //     SetPaused(false);
        // }
        // if (Keyboard.current.pKey.wasPressedThisFrame) {
        //     SetPaused(true);
        // }
        if (Keyboard.current.endKey.wasPressedThisFrame) {
            skipButton.SetActive(!skipButton.activeSelf);
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
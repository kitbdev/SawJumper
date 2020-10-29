using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamRecenter : MonoBehaviour {

    public InputActionReference recenterButton;
    CinemachineFreeLook freeLook;
    // GameManager gm;
    InputAction action;
    Controls controls;

    void Awake() {
        freeLook = GetComponent<CinemachineFreeLook>();
        controls = new Controls();
        // action = recenterButton.ToInputAction();
        // controls.Enable();
        controls.Player.Recenter.performed += c => Recenter();
        // action.actionMap.Enable();
        // Debug.Log("rac:"+action+" "+action.id+" ."+recenterButton);
        // action.Enable();
        // action.performed += c => Recenter();
    }
    private void OnEnable() {
        controls.Enable();
    }
    private void OnDisable() {
        controls.Disable();
    }
    public void Recenter() {
        // Debug.Log("Recentering! .");
        freeLook.m_RecenterToTargetHeading.m_enabled = true;
        freeLook.m_RecenterToTargetHeading.RecenterNow();
        Invoke("UnRecenter", freeLook.m_RecenterToTargetHeading.m_RecenteringTime + 0.1f);
    }
    void UnRecenter() {
        freeLook.m_RecenterToTargetHeading.m_enabled = false;
    }
}
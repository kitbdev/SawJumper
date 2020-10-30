using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamRecenterV2 : MonoBehaviour {

    public InputActionReference recenterButton;
    public Transform PlayerT;
    // CinemachineFreeLook freeLook;
    // GameManager gm;
    InputAction action;
    Controls controls;
    public bool recentering = false;

    void Awake() {
        recentering = false;
        // freeLook = GetComponent<CinemachineFreeLook>();
        // controls = new Controls();
        // action = recenterButton.ToInputAction();
        // controls.Enable();
        // controls.Player.Recenter.performed += c => Recenter();
        // controls.Player.Recenter.cancelled += c => RecenterStop();
        // action.actionMap.Enable();
        // Debug.Log("rac:"+action+" "+action.id+" ."+recenterButton);
        recenterButton.action.Enable();
        recenterButton.action.performed += c => Recenter();
        recenterButton.action.canceled += c => RecenterStop();
    }
    // private void OnEnable() {
    //     controls.Enable();
    // }
    // private void OnDisable() {
    //     controls.Disable();
    // }
    private void LateUpdate() {
        if (recentering) {
            transform.rotation = PlayerT.transform.rotation;
        }
    }
    public void Recenter() {
        recentering = true;
        // Debug.Log("Recentering! .");
        // freeLook.m_RecenterToTargetHeading.m_enabled = true;
        // freeLook.m_RecenterToTargetHeading.RecenterNow();
        // Invoke("UnRecenter", freeLook.m_RecenterToTargetHeading.m_RecenteringTime + 0.1f);
    }
    void UnRecenter() {
        // freeLook.m_RecenterToTargetHeading.m_enabled = false;
    }
    void RecenterStop() {
        recentering = false;
    }
}
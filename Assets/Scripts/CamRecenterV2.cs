using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamRecenterV2 : MonoBehaviour {

    public InputActionReference recenterButton;
    public Transform PlayerT;
    public CinemachineFreeLook freeLook;
    // GameManager gm;
    float recenterTime = 0;
    InputAction action;
    Controls controls;
    public bool recentering = false;

    void Awake() {
        recentering = false;
        recenterButton.action.Enable();
        recenterButton.action.performed += c => Recenter();
        recenterButton.action.canceled += c => RecenterStop();
    }
    private void LateUpdate() {
        if (recentering) {
            freeLook.m_XAxis.Value = 0;
            transform.rotation = PlayerT.transform.rotation;
        }
    }
    public void Recenter() {
        recentering = true;
        // Debug.Log("Recentering! .");
        // freeLook.m_RecenterToTargetHeading.m_enabled = true;
        // freeLook.m_RecenterToTargetHeading.RecenterNow();
        // Invoke("UnRecenter", freeLook.m_RecenterToTargetHeading.m_RecenteringTime + 0.1f);
        recenterTime = Time.time;
    }
    void UnRecenter() {
        // freeLook.m_RecenterToTargetHeading.m_enabled = false;
        recentering = false;
    }
    void RecenterStop() {
        float tdif = Time.time - recenterTime;
        if (tdif < 0.1f && tdif > 0) {
            Invoke("Unrecenter",tdif);
        } else {
            UnRecenter();
        }
    }
}
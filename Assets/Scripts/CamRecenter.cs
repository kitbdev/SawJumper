using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamRecenter : MonoBehaviour {

    public InputActionReference recenterButton;
    CinemachineFreeLook freeLook;
    // GameManager gm;

    void Awake() {
        freeLook = GetComponent<CinemachineFreeLook>();
        recenterButton.action.Enable();
        recenterButton.action.performed += c => Recenter();
        recenterButton.action.canceled += c => RecenterStop();
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
    void RecenterStop() {
        freeLook.m_RecenterToTargetHeading.m_enabled = false;
    }
}
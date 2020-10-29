using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class CamRecenter : MonoBehaviour {

    public InputActionReference recenterButton;
    CinemachineFreeLook freeLook;
    // GameManager gm;

    void Start() {
        freeLook = GetComponent<CinemachineFreeLook>();
        // InputAction action = recenterButton.action;
        // Debug.Log("rac:"+action+" "+action.id+" ."+recenterButton);
        // action.Enable();
        recenterButton.action.performed += c => Recenter();
    }
    public void Recenter() {
        Debug.Log("Recentering!");
        freeLook.m_RecenterToTargetHeading.m_enabled = true;
        freeLook.m_RecenterToTargetHeading.RecenterNow();
        freeLook.m_RecenterToTargetHeading.m_enabled = false;
    }
}
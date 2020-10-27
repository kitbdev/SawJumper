using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamRecenter : MonoBehaviour {

    CinemachineFreeLook freeLook;

    void Awake() {
        freeLook = GetComponent<CinemachineFreeLook>();
    }
    public void Recenter() {
        // freeLook.m_XAxis.HasRecentering = true;
    }
}
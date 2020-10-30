using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawbladeFxer : MonoBehaviour {

    public float volume = 1;
    public GameObject fxgo;
    public LayerMask collLayer;

    void Awake() {
    }
    public void StartEffect(Vector3 spawnPos) {
        GameObject fx =  Instantiate(fxgo);
        fx.transform.position = spawnPos;
    }
    private void OnTriggerEnter(Collider other) {
        // Debug.Log("hit "+other.name);
        if ((collLayer & (1 << other.gameObject.layer)) != 0) {
            StartEffect(transform.position);
        }
    }

}
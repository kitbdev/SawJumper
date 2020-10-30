using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RespawnPoint : MonoBehaviour {
    public Transform respawnPoint;
    private void Awake() {
        if (!respawnPoint) {
            respawnPoint = transform;
        }
    }
    private void OnTriggerEnter(Collider other) {
        var parent = other.transform.parent;
        if (parent && parent.CompareTag("Player")) {
            parent.GetComponent<PlayerMove>().SetRespawnPoint(respawnPoint);
        }
    }
}
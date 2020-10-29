using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CompleteLevel : MonoBehaviour {
    
    public Transform target;

    GameManager gm;

    void Awake() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        
    }
    
    private void OnTriggerEnter(Collider other) {
        // ready to be interacted with, show prompt
        if (other.CompareTag("Player")) {
            gm.NextLevel();
            // other.GetComponentInParent<PlayerMove>().SetInteractable(this);
        }
    }
}
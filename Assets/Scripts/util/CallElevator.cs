using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CallElevator : MonoBehaviour {

    public Transform toT;
    [Space]
    public LayerMask layerInteract = 0;
    Animation anim;
    GameManager gm;
    Elevator elev;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        elev = GameObject.FindGameObjectWithTag("Elevator").GetComponent<Elevator>();
        anim = GetComponent<Animation>();
    }

    public void Interacted() {
        elev.Call(toT);
    }
    private void OnTriggerEnter(Collider other) {
        if ((layerInteract & (1 << other.gameObject.layer)) != 0) {
            Interacted();
        }
    }
}
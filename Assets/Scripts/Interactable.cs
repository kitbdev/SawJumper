﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour {
    
    public bool oneTime = true;
    public float cooldownDur = 0.5f;
    [Space]
    public bool beenHit = false;
    float lastInteractTime = 0;

    public UnityEvent interactedEvent;
    Transform interactableUI;
    Animation anim;
    GameManager gm;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        anim = GetComponent<Animation>();
        Restart();
    }

    void Restart() {
        beenHit = false;
        lastInteractTime = 0;
    }

    public void Interacted() {
        if (oneTime) {
            if (beenHit) {
                return;
            }
            beenHit = true;
        } else if (Time.time < lastInteractTime + cooldownDur) {
            return;
        }
        interactedEvent.Invoke();
        lastInteractTime = Time.time;
        if (anim) anim.Play();
    }
    private void OnTriggerEnter(Collider other) {
        // ready to be interacted with, show prompt
    }
}
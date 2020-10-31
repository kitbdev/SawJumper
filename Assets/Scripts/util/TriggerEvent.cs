using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour {

    public bool oneTime = false;
    public float repeatCooldownDur = 0.2f;
    [Space]
    public bool beenHit = false;
    float lastInteractTime = 0;
    public string tagInteract = "Player";
    public LayerMask layerInteract = 1;
    public int addScore = 0;
    public string gameManagerFunctionCall = "";
    // public int gameManagerFunctionCallInt = -2;
    // public GameObject spawnObject;
    // public string spawnObjectCall = "";
    // public string playerCall = "";

    public UnityEvent triggeredEnterEvent;
    public UnityEvent triggeredRepEvent;
    public UnityEvent triggeredExitEvent;
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
        } else if (Time.time < lastInteractTime + repeatCooldownDur) {
            return;
        }
        triggeredRepEvent.Invoke();
        lastInteractTime = Time.time;
    }
    private void OnTriggerEnter(Collider other) {
        if (!(oneTime && beenHit) && (layerInteract & (1 << other.gameObject.layer)) != 0 && (tagInteract != "" ? other.CompareTag(tagInteract) : true)) {
            triggeredEnterEvent.Invoke();
            if (gameManagerFunctionCall != "") {
                // if (gameManagerFunctionCallInt != -2) {
                //     gm.SendMessage(gameManagerFunctionCall, gameManagerFunctionCallInt);
                // } else {
                    gm.SendMessage(gameManagerFunctionCall);
                // }
            }
            if (addScore != 0) {
                gm.AddScore(addScore);
            }
            beenHit = true;
            lastInteractTime = Time.time;
            if (anim) anim.Play();
        }
    }
    private void OnTriggerStay(Collider other) {
        if ((layerInteract & (1 << other.gameObject.layer)) != 0 && (tagInteract != "" ? other.CompareTag(tagInteract) : true)) {
            Interacted();
        }
    }
    private void OnTriggerExit(Collider other) {
        if (!(oneTime && beenHit) && (layerInteract & (1 << other.gameObject.layer)) != 0 && (tagInteract != "" ? other.CompareTag(tagInteract) : true)) {
            triggeredExitEvent.Invoke();
            beenHit = true;
        }
    }
}
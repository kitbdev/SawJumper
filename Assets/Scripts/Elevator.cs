using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Elevator : MonoBehaviour {

    public float callSpeed = 1;
    public float callHeight = 10;
    public AnimationClip openDoorClip;
    public AnimationClip closeDoorClip;
    public AnimationClip moveSimClip;
    public AudioClip doorMoveSfx;
    public AudioClip dingSfx;
    public AudioClip moveSfx;
    Vector3 curPos;
    Transform player;

    AudioSource audioS;
    Animation anim;
    GameManager gm;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        anim = GetComponent<Animation>();
        audioS = GetComponent<AudioSource>();
        UpdateUI();
    }
    public void UpdateUI() {
        // gm.sceneLoadProgress
    }
    public void Call(Transform tot) {
        if (tot.position == curPos) {
            // already here
            return;
        }
        transform.position = tot.position + Vector3.up * callHeight;
        transform.rotation = tot.rotation;
        // do anim
        Tween t = transform.DOMoveY(transform.position.y, callSpeed);
        t.SetLoops(0);
        t.SetEase(Ease.OutCubic);
        t.Play();
        t.onComplete += () => LandComplete();
    }
    public void Land() {
        Transform newT = GameObject.FindGameObjectWithTag("StartPos").transform;
        if (!newT) {
            Debug.LogWarning("Can't find where to start!");
            newT = gm.transform;
        }
        transform.position = newT.position + Vector3.down * callHeight/2;
        transform.rotation = newT.rotation;
        // do anim
        Tween t = transform.DOMoveY(transform.position.y, callSpeed/2);
        t.SetLoops(0);
        t.SetEase(Ease.OutCubic);
        t.Play();
        t.onComplete += () => LandComplete();
    }
    void LandComplete() {
        OpenDoors();
    }
    void PlayerIn() {
        CloseDoors();
        gm.NextLevel();
    }
    public void HoldPlayer() {
        if (Vector3.Distance(transform.position, player.transform.position) > 10) {
            player.position = transform.position;
        }
    }
    public void SimMovement() {
        anim.clip = moveSimClip;
        anim.Play();
    }
    public void StopSimMovement() {
        anim.clip = moveSimClip;
        anim.Stop();
    }
    public void OpenDoors() {
        // anim
        anim.clip = openDoorClip;
        anim.Play();
    }
    public void CloseDoors() {
        anim.clip = closeDoorClip;
        anim.Play();
    }
    public void OpenFinish() {
        // todo sfx
    }
    public void CloseFinish() {
        HoldPlayer();
        SimMovement();
    }
    private void OnTriggerEnter(Collider other) {
        if ((LayerMask.NameToLayer("Player") & (1 << other.gameObject.layer)) != 0) {
            player = other.transform.parent;
            PlayerIn();
        }
    }
}
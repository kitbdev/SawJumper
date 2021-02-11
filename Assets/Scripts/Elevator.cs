using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// controls elevator animation and movement
/// </summary>
public class Elevator : MonoBehaviour {

    public float callSpeed = 1;
    public float callHeight = 10;
    public LayerMask movingLayer = 1 << 14;
    LayerMask defLayer;
    public AnimationClip openDoorClip;
    public AnimationClip closeDoorClip;
    public AnimationClip moveSimClip;
    public AudioClip doorMoveSfx;
    public AudioClip dingSfx;
    public AudioClip moveSfx;
    Vector3 curPos;
    Transform player;
    bool isMoving = false;
    bool isAtFinish = false;

    AudioSource audioSource;
    Animation anim;
    GameManager gm;

    void Awake() {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        anim = GetComponent<Animation>();
        audioSource = GetComponent<AudioSource>();
        defLayer = gameObject.layer;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        UpdateUI();
    }
    private void Update() {
        if (isMoving) {
            UpdateUI();
        }
    }
    public void UpdateUI() {
        // gm.sceneLoadProgress
    }
    public void Call(Transform tot) {
        if (tot.position == curPos) {
            // already here
            return;
        }
        curPos = tot.position;
        transform.position = tot.position + Vector3.up * callHeight;
        transform.rotation = tot.rotation;
        // do anim
        anim.clip = closeDoorClip;
        anim.Play();
        Tween t = transform.DOMoveY(curPos.y, callSpeed);
        t.SetLoops(0);
        t.SetEase(Ease.OutCubic);
        t.Play();
        t.onComplete += () => LandComplete();
        isMoving = true;
        // gameObject.layer = movingLayer;
        isAtFinish = true;
    }
    public void Land() {
        Transform newT = GameObject.FindGameObjectWithTag("StartPos").transform;
        if (!newT) {
            Debug.LogWarning("Can't find where to start!");
            newT = gm.transform;
        }
        curPos = newT.position;
        var poff = player.position - transform.position;
        transform.position = newT.position;
        player.position = transform.position + poff;
        // transform.position = newT.position + Vector3.down * callHeight/2;
        transform.rotation = newT.rotation;
        // do anim
        // Tween t = transform.DOMoveY(transform.position.y, callSpeed/2);
        // t.SetLoops(0);
        // t.SetEase(Ease.OutCubic);
        // t.Play();
        // t.onComplete += () => LandComplete();
        // isMoving = true;
        // gameObject.layer = movingLayer;
        Invoke("LandComplete", 4);
        LandComplete();
    }
    void LandComplete() {
        // audioSource.PlayOneShot(dingSfx);
        transform.position = curPos;
        OpenDoors();
        isMoving = false;
        gameObject.layer = defLayer;
    }
    void PlayerIn() {
        if (isAtFinish) {
            CloseDoors();
            gm.NextLevel();
            isAtFinish = false;
        }
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
        audioSource.PlayOneShot(doorMoveSfx);
    }
    public void CloseDoors() {
        anim.clip = closeDoorClip;
        anim.Play();
        audioSource.PlayOneShot(doorMoveSfx);
    }
    public void OpenFinish() {
        audioSource.PlayOneShot(dingSfx);
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
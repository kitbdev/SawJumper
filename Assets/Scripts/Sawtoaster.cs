using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class Sawtoaster : MonoBehaviour {

    public bool autoStart = true;
    public float startDelay = 0;
    public bool autoRelaunch = true;
    public float relaunchDelayDur = 5;
    public float launchSpeed = 10;
    public float grabDur = 4;
    float lastLaunchTime = 0;

    public GameObject sawbladep;
    public Transform grabberT;

    GameObject grabbedSawblade;
    // GameManager gm;

    void Start() {
        // gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (autoStart) {
            LaunchIn(startDelay);
        }
    }
    public void LaunchIn(float t = 0) {
        if (grabbedSawblade != null) {
            // already launching!
            Debug.Log("Already launching!");
            return;
        }
        StartCoroutine(Launch(t));
    }
    IEnumerator Launch(float t) {
        // spawn sawblade then launch it
        if (t >= grabDur) {
            yield return new WaitForSeconds(t - grabDur);
            SpawnBlade();
            grabbedSawblade.transform.DOMove(grabberT.position, 1f);
            yield return new WaitForSeconds(grabDur);
            ReleaseBlade();
        } else {
            SpawnBlade();
            grabbedSawblade.transform.DOMove(grabberT.position, 0.1f);
            yield return new WaitForSeconds(t);
            ReleaseBlade();
        }
        yield return null;
        if (autoRelaunch) {
            LaunchIn(relaunchDelayDur);
        }
    }
    void SpawnBlade() {
        grabbedSawblade = Instantiate(sawbladep, transform.position, Quaternion.identity);
        grabbedSawblade.transform.parent = transform;
        var sb = grabbedSawblade.GetComponent<Sawblade>();
        sb.speed = 0;
    }
    void ReleaseBlade() {
        grabbedSawblade.transform.parent = null;
        grabbedSawblade.transform.position = grabberT.position;
        grabbedSawblade.transform.forward = transform.forward;
        var sb = grabbedSawblade.GetComponent<Sawblade>();
        sb.speed = launchSpeed;

        grabbedSawblade = null;
        lastLaunchTime = Time.time;
    }

}
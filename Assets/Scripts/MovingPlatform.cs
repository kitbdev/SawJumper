using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    public Vector3[] path = new Vector3[0];
    [Range(0, 20)]
    public float duration = 5;
    [Range(0, 20)]
    public float startPercent = 0;
    public bool syncDist = true;
    public bool isPaused = false;
    public bool isOpen = true;
    public Ease easeType = Ease.InOutCubic;
    // public float spinSpeed = 0;
    public Vector3 spinEuler = Vector3.zero;

    public Tween moveTween;
    public Tween spinTween;

    void Start() {
        DOTween.defaultUpdateType = UpdateType.Fixed;
        if (path.Length > 1) {
            if (!isOpen) {
                var v1 = new List<Vector3>(path);
                v1.Add(v1[0]);
                path = v1.ToArray();
            }
            // path[0] = transform.position;
            moveTween = transform.DOPath(path, duration);
            if (syncDist) {
                // moveTween.PathLength
            }
            // mainTween = transform.DOLocalPath(path, duration);
            moveTween.SetLoops(-1, isOpen? LoopType.Yoyo : LoopType.Restart);
            if (!isOpen) {
                moveTween.SetEase(Ease.Linear);
            } else {
                // moveTween.SetEase(Ease.InOutCubic);
                moveTween.SetEase(easeType);
            }
            moveTween.Goto(startPercent);
        }
        if (spinEuler.sqrMagnitude != 0) {
            // Debug.Log("starting spin! " + gameObject.name);
            spinTween = transform.DOLocalRotate(spinEuler, duration);
            spinTween.SetLoops(-1, LoopType.Incremental);
            spinTween.SetEase(Ease.Linear);
        }
        if (isPaused) {
            PauseMovement();
        } else {
            ResumeMovement();
        }
    }

    public void PauseMovement() {
        isPaused = true;
        if (moveTween != null) moveTween.Pause();
        if (spinTween != null) spinTween.Pause();
    }
    public void ResumeMovement() {
        isPaused = false;
        if (moveTween != null) moveTween.Play();
        if (spinTween != null) spinTween.Play();
    }
}
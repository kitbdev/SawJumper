using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class MovingPlatform : MonoBehaviour {

    [SerializeField]
    public List<Vector3> path = new List<Vector3>();
    [Range(0, 20)]
    public float duration = 5;
    [Range(0, 1)]
    public float startPercent = 0;
    public bool syncDist = true;
    public bool isPaused = false;
    public bool isOpen = true;
    public Ease easeType = Ease.InOutCubic;
    // public float spinSpeed = 0;
    public Vector3 spinEuler = Vector3.zero;

    [Space]
    public Tween moveTween;
    public Tween spinTween;


    void Start() {
        DOTween.defaultUpdateType = UpdateType.Fixed;
        if (path.Count > 1 && transform.childCount > 0) {
            if (path.Count == 2) {
                isOpen = true;
            }
            if (!isOpen) {
                path.Add(path[0]);
            }
            // path[0] = transform.position;
            // Transform newparent = new GameObject(name+" holder").transform;
            // newparent.parent = transform.parent;
            // newparent.position = transform.position;
            // newparent.rotation = transform.rotation;
            // transform.parent = newparent;
            // transform.localPosition = Vector3.zero;
            // transform.localRotation = Quaternion.identity;

            moveTween = transform.GetChild(0).DOLocalPath(path.ToArray(), duration);
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
            moveTween.Goto(startPercent * duration);
        }
        if (spinEuler.sqrMagnitude != 0) {
            // Debug.Log("starting spin! " + gameObject.name);
            spinTween = transform.GetChild(0).DOLocalRotate(spinEuler, duration);
            spinTween.SetLoops(-1, LoopType.Incremental);
            spinTween.SetEase(Ease.Linear);
        }
        if (isPaused) {
            PauseMovement();
        } else {
            ResumeMovement();
        }
    }
    private void OnDisable() {
        if (moveTween != null) moveTween.Kill();
        if (spinTween != null) spinTween.Kill();
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
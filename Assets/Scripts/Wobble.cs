using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class Wobble : MonoBehaviour {
    
    public float duration = 2;
    public float intensity = 30;
    public float randomness = 80;

    void Start() {
        // Tween tween = transform.DOShakeRotation(duration,intensity,10,randomness,false);
        transform.GetChild(0).Rotate(Vector3.forward * randomness);
        Tween tween = transform.DOLocalRotate(Vector3.up * intensity,duration,RotateMode.Fast);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.SetEase(Ease.Linear);
        tween.Play();
    }
}
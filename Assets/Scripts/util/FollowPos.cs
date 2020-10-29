using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowPos : MonoBehaviour {
    
    public Transform target;

    void Awake() {
        
    }
    private void FixedUpdate() {
        transform.position = target.position;
    }
}
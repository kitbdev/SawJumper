using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIn : MonoBehaviour {

    public float timeToLive = 0;

    void Start() {
        if (timeToLive > 0) {
            Destroy(gameObject, timeToLive);
        }
    }
    public void DestroyMe() {
        Destroy(gameObject);
    }

}
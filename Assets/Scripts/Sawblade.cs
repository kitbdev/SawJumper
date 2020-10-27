using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawblade : MonoBehaviour {

    public float speed = 10;
    public bool bouncy = true;

    void Start() {

    }
    private void Update() {
        transform.Translate(transform.forward * speed * Time.deltaTime);
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Sawblade : MonoBehaviour {

    public float speed = 10;
    public bool bouncy = true;
    Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate() {
        // transform.Translate(transform.forward * speed * Time.deltaTime);
        rb.velocity = transform.forward * speed;
    }

}
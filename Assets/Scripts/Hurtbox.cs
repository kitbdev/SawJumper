using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour {

    public int damage = 1;
    public LayerMask layerMask = 1;
    public float hitRepeatCooldown = 1;
    float lastHurtTime = 0;

    void Start() {

    }

    private void OnTriggerEnter(Collider other) {
        Hit(other);
    }
    private void OnTriggerStay(Collider other) {
        if (Time.time > lastHurtTime + hitRepeatCooldown) {
            Hit(other);
        }
    }
    void Hit(Collider other) {
        if ((layerMask & (1 << other.gameObject.layer)) > 0) {
            Health hitme = other.GetComponent<Health>();
            if (!hitme) {
                hitme = other.GetComponentInParent<Health>();
            }
            lastHurtTime = Time.time;
            if (!hitme) {
                return;
            }
            hitme.TakeDamage(damage);
        }
    }
}
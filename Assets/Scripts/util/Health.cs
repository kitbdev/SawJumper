using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {

    public int maxHealth = 3;
    public float regenSpeed = 5;
    public float regenDelay = 5;
    [Space]
    public int curHealth = 0;
    public float lastHurtTime = 0;
    public float regenTickTime = 0;
    // shield?

    public UnityEvent healthChangedEvent;
    public UnityEvent deadEvent;

    void Start() {
        Restart();
    }

    public void Restart() {
        curHealth = maxHealth;
        healthChangedEvent.Invoke();
    }
    private void Update() {
        if (regenSpeed > 0 && curHealth < maxHealth) {
            if (Time.time > lastHurtTime + regenDelay) {
                if (Time.time > regenTickTime + regenSpeed) {
                    curHealth += 1;
                    regenTickTime = Time.time;
                    // - * Time.deltaTime * regenSpeed;
                    // DOTween.To(() => curHealth, x => curHealth = x, maxHealth, regenSpeed);
                }
            }
        }
    }
    public void SetHealth(int amount = -1) {
        if (amount == -1) {
            amount = maxHealth;
        }
        curHealth = amount;
        healthChangedEvent.Invoke();
    }
    public void TakeDamage(int amount = 1) {
        // Debug.Log("Ow! "+amount);
        if (curHealth <= 0) {
            healthChangedEvent.Invoke();
            return;
        }
        curHealth -= amount;
        healthChangedEvent.Invoke();
        lastHurtTime = Time.time;
        if (curHealth <= 0) {
            // die
            deadEvent.Invoke();
            curHealth = 0;
        }
    }
}
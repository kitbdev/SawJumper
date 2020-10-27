using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
    
    public int maxHealth = 3;
    [Space]
    public int curHealth = 0;
    // shield?

    public UnityEvent healthChangedEvent;
    public UnityEvent deadEvent;

    void Start() {
        Restart();
    }

    void Restart() {
        curHealth = maxHealth;
        healthChangedEvent.Invoke();
    }

    public void TakeDamage(int amount) {
        curHealth -= amount;
        healthChangedEvent.Invoke();
        if (curHealth <= 0) {
            // die
            deadEvent.Invoke();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // stores gamestate and stuff
    Transform playert;
    public int points = 0;
    public int curScene = 0;

    void Start() {

    }

    void Update() {

    }
    public void NextLevel() {
        if (curScene < SceneManager.sceneCountInBuildSettings) {
            // load next scene
            curScene++;
            SceneManager.LoadScene(curScene);
        } else {
            Debug.LogWarning("Last Scene Reached! "+curScene);
        }
    }
    public void AddScore(int amount) {
        points += amount;
    }
}
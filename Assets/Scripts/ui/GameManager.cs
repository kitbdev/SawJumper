using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // stores gamestate and stuff
    Elevator elev;
    PlayerMove player;
    [Space]
    public int level = 0;
    public int curScene = 1;
    public int points = 0;
    public bool loadingScene = false;
    public float sceneLoadProgress = 0;

    void Start() {
        elev = GameObject.FindGameObjectWithTag("Elevator").GetComponent<Elevator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        // loadingScene = false;
        if (SceneManager.sceneCount < 2) {
            // Scene Load order:
            //  main menu(0) -> play loads keepscene(1)
            //  keepscene(1) this -> additivly loads the respective level (2+,l0+)
            // SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(LoadNewScene(curScene, true));
        }
    }

    void Update() {

    }
    public void BackToMainMenu() {
        SceneManager.LoadScene(0);
    }
    public void NextLevel() {
        if (curScene < SceneManager.sceneCountInBuildSettings) {
            // load next scene
            StartCoroutine(LoadNewScene(curScene + 1));
        } else {
            Debug.LogWarning("Last Scene Reached! " + curScene);
        }
    }
    public void RestartLevel() {
        elev.CloseDoors();
        player.SetRespawnPoint(elev.transform);
        player.Respawn();
        StartCoroutine(LoadNewScene(curScene));
    }
    public UnityEvent loadStartEvent;
    IEnumerator LoadNewScene(int newScene, bool onlyLoad = false) {
        if (newScene < SceneManager.sceneCountInBuildSettings || newScene <= 0) {
            Debug.LogWarning("Invalid scene to load " + newScene);
        } else if (loadingScene) {
            Debug.Log("Already loading a scene! tried:" + newScene);
        } else {
            loadingScene = true;
            loadStartEvent.Invoke();
            level = newScene - 2;
            if (!onlyLoad) {
                Debug.Log("Unloading scene " + curScene + "...");
                AsyncOperation unload = SceneManager.UnloadSceneAsync(curScene);
                while (!unload.isDone) {
                    sceneLoadProgress = unload.progress;
                    yield return null;
                }
            }
            Debug.Log("Loading scene " + newScene + "...");
            AsyncOperation load = SceneManager.LoadSceneAsync(newScene);
            while (!load.isDone) {
                sceneLoadProgress = load.progress;
                yield return null;
            }
            curScene = newScene;
            loadingScene = false;
            player.SetRespawnPoint(elev.transform);
            elev.Land();
        }
    }
    public void AddScore(int amount) {
        points += amount;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public bool hasSave = false;
    public int levelId = 0;
    public bool canQuit = false;

    void Start() {
        canQuit = false;
#if UNITY_PLAYER
        canQuit = true;
#endif
        TryLoadGame();
    }
    void SaveGame() {

    }
    void TryLoadGame() {
        hasSave = false;
    }
    public void SetLevelId(int nlevelid) {
        levelId = nlevelid;
    }
    public void NewPlaybtn() {
        SceneManager.LoadScene(1);
    }
    public void ContinuePlaybtn() {
        // SceneManager.LoadScene(2+levelId);

    }
    public void Optionbtn() {
        // ? or in unity event
    }
    public void Exitbtn() {
        Application.Quit();
    }
}
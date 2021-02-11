using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu controller
/// buttons to toggle fullscreen, quit, or start game
/// </summary>
public class MainMenu : MonoBehaviour {

    public GameObject quitbtn;
    public GameObject loadbtn;
    public Text levelPicker;
    public CanvasGroup mainMenuG;
    public CanvasGroup optionsMenuG;
    public CanvasGroup pauseMenuG;
    [Space]
    public CanvasGroup curMenuG;
    public int levelId = 0;
    public bool canQuit = false;
    public bool hasSave = false;

    void Start() {
        canQuit = false;
        // #if UNITY_PLAYER
        //         canQuit = true;
        // #elif UNITY_EDITOR
        canQuit = true;
        // #endif
        // if (!canQuit) {
        //     quitbtn.SetActive(false);
        // }
        curMenuG = mainMenuG;
        SwitchTo(mainMenuG);
        levelPicker.text = levelId + "";
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
    public void LoadLevelbtn() {
        SceneManager.LoadScene(2 + levelId);
    }
    public void IncLevelbtn(bool neg = false) {
        levelId += neg? - 1 : 1;
        levelId = Mathf.Clamp(levelId, 0, SceneManager.sceneCountInBuildSettings - 2);
        levelPicker.text = levelId + "";
    }
    public void SwitchTo(CanvasGroup menu) {
        curMenuG.alpha = 0;
        curMenuG.interactable = false;
        curMenuG.blocksRaycasts = false;
        curMenuG = menu;
        // todo animate
        curMenuG.alpha = 1;
        curMenuG.interactable = true;
        curMenuG.blocksRaycasts = true;
    }
    public void Optionbtn() {
        // ? or in unity event
    }
    bool isFullScreen = true;
    public void ToggleFullscreen() {
        isFullScreen = !isFullScreen;
        if (isFullScreen) {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        } else {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
    }
    public void Exitbtn() {
        Application.Quit();
    }
}
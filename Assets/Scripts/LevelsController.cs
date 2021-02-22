using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelsController : MonoBehaviour {
    public int loadLevelNumber = 1;
    public CanvasGroup gamePassedScreen;
    public List<Map> levels;
    //public static int staticLoadLevelNumber = 0;
    Map _loadedLevel;
    private void Awake() {
        foreach (var level in levels) {
            level.gameObject.SetActive(false);
            level.Passed += OnLevelPassed;
            level.Failed += OnLevelFailed;
        }
        if (PlayerPrefs.HasKey("level")) {
            loadLevelNumber = PlayerPrefs.GetInt("level");
        }
        //if (staticLoadLevelNumber > 0) {
        //    loadLevelNumber = staticLoadLevelNumber;
        //    staticLoadLevelNumber = 0;
        //} else 
        StartLevel(loadLevelNumber);
    }
    private void Update() {
        var keyboard = Keyboard.current;
        if (keyboard.escapeKey.IsActuated()) Application.Quit();
    }
    void StartLevel(int number) {
        if (number <= 0 || number > levels.Count) return;
        _loadedLevel = levels[number - 1];
        PlayerPrefs.SetInt("level", number);
        _loadedLevel.gameObject.SetActive(true);
        _loadedLevel.Init();
    }
    void DisableCurrentLevel() {
        _loadedLevel.gameObject.SetActive(false);
    }
    async void OnLevelPassed() {
        if(levels.IndexOf(_loadedLevel) + 2 > levels.Count) {
            gamePassedScreen.DOFade(1, 1f);
            PlayerPrefs.SetInt("level", 1);
            return;
        }
        await LoadingScreen.I.Open();
        DisableCurrentLevel();
        StartLevel(levels.IndexOf(_loadedLevel) + 2);
        _ = LoadingScreen.I.Close();
    }
    void OnLevelFailed() {
        //staticLoadLevelNumber = PlayerPrefs.GetInt("level");// levels.IndexOf(_loadedLevel) + 1;
        LoadingScreen.I.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

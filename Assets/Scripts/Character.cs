using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Character : MovableObject {
    public bool isGirl;
    public static bool IsGameOver { get; private set; } = false;
    public static Character Boy { get; private set; }
    public static Character Girl { get; private set; }
    public override void Init(Map map) {
        base.Init(map);
        IsGameOver = false;
        if (isGirl) Girl = this;
        else Boy = this;
    }
    private void Update() {
        if (IsGameOver) return;
        if (OnWrongLight() && !_moveOnCooldown) GameOver();
        if (_moveOnCooldown) return;
        var keyboard = Keyboard.current;
        Vector3Int direction = new Vector3Int();
        if (isGirl) {
            if (keyboard.aKey.IsActuated()) direction = Vector3Int.left;
            if (keyboard.dKey.IsActuated()) direction = Vector3Int.right;
            if (keyboard.wKey.IsActuated()) direction = Vector3Int.up;
            if (keyboard.sKey.IsActuated()) direction = Vector3Int.down;
        } else {
            if (keyboard.leftArrowKey.IsActuated()) direction = Vector3Int.left;
            if (keyboard.rightArrowKey.IsActuated()) direction = Vector3Int.right;
            if (keyboard.upArrowKey.IsActuated()) direction = Vector3Int.up;
            if (keyboard.downArrowKey.IsActuated()) direction = Vector3Int.down;
        }
        if (direction.sqrMagnitude > 0) _ = Move(direction);
    }
    bool OnWrongLight() {
        if (isGirl && !_map.IsLit(Position)) return true;
        else if (!isGirl && _map.IsLit(Position)) return true;
        return false;
    }
    void GameOver() {
        Debug.Log("Game over");
        LoadingScreen.I.LoadScene(SceneManager.GetActiveScene().buildIndex);
        IsGameOver = true;
    }
    public async override Task<bool> Move(Vector3Int move) {
        if (!CanMove(move)) return false;
        _map.ScheduleCalculateLight(0.3f);
        var moved = await base.Move(move);
        if(moved) CheckWin();
        return moved;
    }
    void CheckWin() {
        Vector3Int distanceToOther = isGirl ? Boy.Position - Position : Girl.Position - Position;
        if (distanceToOther.sqrMagnitude == 1) {
            Debug.Log("Win");
            LoadingScreen.I.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            IsGameOver = true;
        }
    }
}

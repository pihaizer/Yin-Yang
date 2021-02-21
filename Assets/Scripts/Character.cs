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
    public Action Passed;
    public Action Failed;
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
        if (isGirl && _map.IsShadowed(Position)) return true;
        else if (!isGirl && _map.IsLit(Position)) return true;
        return false;
    }
    void GameOver() {
        IsGameOver = true;
        Failed?.Invoke();
        //LoadingScreen.I.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public async override Task<bool> Move(Vector3Int move) {
        if (!CanMove(move)) return false;
        if(_map.ContainsMovableObject(Position + move, out MovableObject movableObject))
            _map.ScheduleCalculateLight();
        var moved = await base.Move(move);
        if (moved) {
            if (OnWrongLight()) GameOver();
            else CheckWin();
        }
        return moved;
    }
    void CheckWin() {
        if (IsGameOver) return;
        if (DestinationPoint.ForGirl == Girl.Position &&
            DestinationPoint.ForBoy == Boy.Position) {
            IsGameOver = true;
            Passed?.Invoke();
            //LoadingScreen.I.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}

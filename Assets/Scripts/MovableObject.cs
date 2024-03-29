﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovableObject : GridObject {
    protected bool _moveOnCooldown = false;
    [HideInInspector]
    public bool movingActive = true;
    Vector3 _goalPosition;
    Collider2D _collider;

    public override void Init(Map map) {
        base.Init(map);
        if (TryGetComponent(out Collider2D collider)) _collider = collider; 
    }

    public bool CanMove(Vector3Int move) {
        if (!movingActive) return false;
        var tempPosition = Position + move;
        if (_map.ContainsStaticObject(tempPosition)) return false;
        else if (_map.ContainsMovableObject(tempPosition, out MovableObject movableObject)) {
            if (!movableObject.CanMove(move)) return false;
        }
        return true;
    }
    //returns true if 
    public async virtual Task<bool> Move(Vector3Int move) {
        if (!CanMove(move)) return false;
        if (_map.ContainsMovableObject(Position + move, out MovableObject movableObject))
            _ = movableObject.Move(move);
        _moveOnCooldown = true;
        Position += move;
        _goalPosition = _map.Grid.GetCellCenterWorld(Position);
        await DOTween.To(GetPosition, SetPosition, _goalPosition, 0.3f).AsyncWaitForCompletion();
        //await transform.DOMove(_goalPosition, 0.3f).AsyncWaitForCompletion();
        _moveOnCooldown = false;
        return true;
    }
    Vector3 GetPosition() => transform.position;
    void SetPosition(Vector3 newValue) {
        transform.position = newValue;
        if (_collider) _collider.offset = _goalPosition - newValue;
    }
}

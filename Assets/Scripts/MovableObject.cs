using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovableObject : MonoBehaviour {
    protected bool _moveOnCooldown = false;
    protected Map _map;

    public Vector3Int Position { get; private set; }
    public virtual void Init(Map map) {
        _map = map;
        Position = _map.Grid.WorldToCell(transform.position);
        transform.position = _map.Grid.GetCellCenterWorld(Position);
    }
    public bool CanMove(Vector3Int move) {
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
        await transform.DOMove(_map.Grid.GetCellCenterWorld(Position), 0.3f).AsyncWaitForCompletion();
        _moveOnCooldown = false;
        return true;
    }
}

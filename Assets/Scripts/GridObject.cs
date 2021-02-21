using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour {
    public virtual Vector3Int Position { get; protected set; }
    protected Map _map;
    public virtual void Init(Map map) {
        _map = map;
        Position = _map.Grid.WorldToCell(transform.position);
        transform.position = _map.Grid.GetCellCenterWorld(Position);
        if(TryGetComponent(out Collider2D collider)) {
            collider.enabled = false;
            collider.enabled = true;
        }
    }    
}

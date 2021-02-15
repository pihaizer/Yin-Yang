using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MovableObject {
    public Type type;
    public bool startActive = true;
    [Range(0, 30)]
    public float radius;

    public bool Active {
        get => _active;
        set => SetActive(value);
    }
    bool _active;

    public override void Init(Map map) {
        base.Init(map);
        _active = startActive;
        movingActive = type == Type.Movable;
        if (type == Type.Decor) GetComponent<SpriteRenderer>().sortingLayerName = "Decor";
    }
    public void ToggleActive() => SetActive(!_active);
    void SetActive(bool value) {
        if (value == _active) return;
        _active = value;
        _map.CalculateLight();
    }
    public enum Type {
        Decor,
        Movable,
        Static
    }
}

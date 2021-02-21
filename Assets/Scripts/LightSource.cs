using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MovableObject {
    public Type type;
    public bool startActive = true;
    [Range(0, 30)]
    public float radius;
    public Color shadowColor;

    public bool Active {
        get => _active;
        set => SetActive(value);
    }
    bool _active;

    public override void Init(Map map) {
        base.Init(map);
        Active = startActive;
        movingActive = type == Type.Movable;
        var sprite = GetComponent<SpriteRenderer>();
        var color = _active ? Color.white : shadowColor;
        sprite.color = color;
        if(TryGetComponent(out Animator animator)) {
            animator.enabled = true;
            animator.SetBool("playing", _active);
        }
    }
    public void ToggleActive() => SetActive(!_active);
    public void SetActive(bool value) {
        if (value == _active) return;
        _active = value;
        _map.ScheduleCalculateLight();
        var sprite = GetComponent<SpriteRenderer>();
        var color = _active ? Color.white : shadowColor;
        DOTween.To(() => sprite.color, (c) => sprite.color = c, color, 0.1f);
        GetComponent<Animator>()?.SetBool("playing", _active);
    }
    public enum Type {
        Decor,
        Movable,
        Static
    }
}

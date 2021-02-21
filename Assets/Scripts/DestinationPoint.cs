using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestinationPoint : GridObject {
    public Type type; 
    public static Vector3 ForGirl { get; private set; }
    public static Vector3 ForBoy { get; private set; }
    bool showing = true;
    public override void Init(Map map) {
        base.Init(map);
        if (type == Type.ForBoy) ForBoy = Position;
        if (type == Type.ForGirl) ForGirl = Position;
    }
    private void Update() {
        if (showing) {
            if (type == Type.ForBoy && Character.Boy.Position == Position) {
                GetComponentInChildren<SpriteRenderer>()?.DOFade(0, 0.3f);
                showing = false;
            }
            if (type == Type.ForGirl && Character.Girl.Position == Position) {
                GetComponentInChildren<SpriteRenderer>()?.DOFade(0, 0.3f);
                showing = false;
            }
        } else {
            if (type == Type.ForBoy && Character.Boy.Position != Position) {
                GetComponentInChildren<SpriteRenderer>()?.DOFade(1, 0.3f);
                showing = true;
            }
            if (type == Type.ForGirl && Character.Girl.Position != Position) {
                GetComponentInChildren<SpriteRenderer>()?.DOFade(1, 0.3f);
                showing = true;
            }
        }
    }
    public enum Type {
        ForGirl,
        ForBoy
    }
}

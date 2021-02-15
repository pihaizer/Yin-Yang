using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationPoint : GridObject {
    public Type type; 
    public static Vector3 ForGirl { get; private set; }
    public static Vector3 ForBoy { get; private set; }
    public override void Init(Map map) {
        base.Init(map);
        if (type == Type.ForBoy) ForBoy = Position;
        if (type == Type.ForGirl) ForGirl = Position;
    }
    public enum Type {
        ForGirl,
        ForBoy
    }
}

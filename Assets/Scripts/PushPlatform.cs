using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PushPlatform : GridObject {
    public UnityEvent<bool> OnPushChanged;
    bool _pushed = false;
    public void Update() {
        if (_pushed && _map.MovableObjects.All(mo => mo.Position != Position))
            TogglePush();
        if (!_pushed && _map.MovableObjects.Any(mo => mo.Position == Position))
            TogglePush();
    }
    void TogglePush() {
        _pushed = !_pushed;
        OnPushChanged.Invoke(_pushed);
    }
}

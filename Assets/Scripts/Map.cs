using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour {
    public Tilemap decorTilemap;
    public Tilemap lightTilemap;
    public Tilemap staticTilemap;
    public Tilemap transparentTilemap;
    public Tilemap movablesTilemap;
    public Tile shadowTile;
    public Color litColor;
    public Color shadowColor;
    public GameObject otherObjectsContainer;
    public GameObject movablePrefab;
    public static Map I { get; private set; }
    public Grid Grid { get; private set; }
    public List<GridObject> GridObjects;
    public List<MovableObject> MovableObjects { get; private set; } = new List<MovableObject>();

    BoundsInt _bounds;
    List<LightSource> _lights = new List<LightSource>();
    public void Awake() {
        if (I != null && I != this) Debug.LogError("Multiple maps!!!");
        I = this;
        _bounds = staticTilemap.cellBounds;
        var camera = Camera.main;
        camera.orthographicSize = _bounds.size.y / 2f;
        camera.transform.position = _bounds.center + Vector3.back;
        Grid = GetComponent<Grid>();

        TileBase[] allMovables = movablesTilemap.GetTilesBlock(_bounds);
        for (int x = 0; x < _bounds.size.x; x++) {
            for (int y = 0; y < _bounds.size.y; y++) {
                var pos = new Vector3Int(x, y, 0) + _bounds.position;
                TileBase tile = allMovables[x + y * _bounds.size.x];
                if (tile == null) continue;
                movablesTilemap.SetTile(pos, null);
                var movable = Instantiate(movablePrefab).GetComponent<MovableObject>();
                movable.transform.position = Grid.GetCellCenterWorld(pos);
                movable.transform.parent = otherObjectsContainer.transform;
                movable.GetComponent<SpriteRenderer>().sprite = (tile as Tile).sprite;
            }
        }
        foreach (var gridObject in otherObjectsContainer.GetComponentsInChildren<GridObject>()) {
            gridObject.Init(this);
            if (gridObject is MovableObject && (gridObject as MovableObject).movingActive)
                MovableObjects.Add(gridObject as MovableObject);
            if (gridObject is LightSource) _lights.Add(gridObject as LightSource);
        }
        ScheduleCalculateLight(0.5f);
    }
    public bool ContainsStaticObject(Vector3Int cell) {
        return staticTilemap.GetTile(cell) != null ||
            transparentTilemap.GetTile(cell) != null ||
            _lights.Any(l => l.type == LightSource.Type.Static && l.Position == cell);
    }
    public bool ContainsMovableObject(Vector3Int cell, out MovableObject movableObject) {
        movableObject = MovableObjects.Find(mo => mo.Position == cell);
        return movableObject != null;
    }
    public bool IsLit(Vector3Int cell) => lightTilemap.GetColor(cell) == litColor;
    public bool IsShadowed(Vector3Int cell) => lightTilemap.GetColor(cell) == shadowColor;
    public void ScheduleCalculateLight(float duration, int times = 6) =>
        StartCoroutine(ScheduleCalculateLightCoroutine(duration, times));

    IEnumerator ScheduleCalculateLightCoroutine(float duration, int times) {
        CalculateLight();
        for (int i = 0; i < times; i++) {
            yield return new WaitForSeconds(duration / times);
            CalculateLight();
        }
    }
    public void CalculateLight() {
        foreach (var point in _bounds.allPositionsWithin) {
            CalculateLightForCell(point);
        }
    }
    void CalculateLightForCell(Vector3Int cell) {
        if (_lights.All(light => !Sees(cell, light)))
            SetLightTile(cell, shadowColor);
        else
            SetLightTile(cell, litColor);
    }
    bool Sees(Vector3Int cell, LightSource light) {
        if (!light.Active) return false;
        var cellPos = Grid.GetCellCenterWorld(cell);
        var lightPos = Grid.GetCellCenterWorld(light.Position);
        var checkPoints = new List<Vector3>() {
            cellPos + new Vector3(1,1) * 0.25f,
            cellPos + new Vector3(-1,1) * 0.25f,
            cellPos + new Vector3(1,-1) * 0.25f,
            cellPos + new Vector3(-1,-1) * 0.25f
        };
        int hits = 0;
        foreach (var p in checkPoints) {
            var distance = Vector3.Distance(p, lightPos);
            if (light.radius > 0 && distance > light.radius) {
                hits++;
                continue;
            }
            var raycastHits = Physics2D.RaycastAll(p, lightPos - p, distance);
            if (raycastHits.Any(hit => hit.collider != null && hit.transform.tag == "LightBlock")) {
                hits++;
            }
        }
        return hits <= 3;
    }
    async void SetLightTile(Vector3Int cell, Color color) {
        if (lightTilemap.GetTile(cell) == null) {
            lightTilemap.SetTile(cell, shadowTile);
            lightTilemap.SetTileFlags(cell, TileFlags.None);
            lightTilemap.SetColor(cell, color);
            return;
        }
        if (color == shadowColor && lightTilemap.GetColor(cell) == litColor) {
            await DOTween.To(() => lightTilemap.GetColor(cell), (c) => lightTilemap.SetColor(cell, c), shadowColor, 0.1f).AsyncWaitForCompletion();
        } else if (color == litColor && lightTilemap.GetColor(cell) == shadowColor) {
            await DOTween.To(() => lightTilemap.GetColor(cell), (c) => lightTilemap.SetColor(cell, c), litColor, 0.1f).AsyncWaitForCompletion();
        }
    }
}

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour {
    public Tilemap staticTilemap;
    public Tilemap movablesTilemap;
    public Tilemap decorTilemap;
    public Tilemap decorLightSourcesTilemap;
    public Tilemap lightTilemap;
    public Tile shadowTile;
    public Color litColor;
    public Color shadowColor;
    public GameObject movableObjectsContainer;
    public GameObject movablePrefab;
    public GameObject staticLightPrefab;
    public static Map I { get; private set; }
    public Grid Grid { get; private set; }
    public List<MovableObject> MovableObjects { get; private set; } = new List<MovableObject>();

    BoundsInt _bounds;
    List<LightTilePos> _decorLights = new List<LightTilePos>();
    List<Vector3Int> _staticLights = new List<Vector3Int>();
    List<MovableObject> _movableLights = new List<MovableObject>();
    public void Awake() {
        if (I != null && I != this) Debug.LogError("Multiple maps!!!");
        I = this;
        _bounds = staticTilemap.cellBounds;
        var camera = Camera.main;
        camera.orthographicSize = _bounds.size.y / 2f;
        camera.transform.position = _bounds.center + Vector3.back;
        Grid = GetComponent<Grid>();

        TileBase[] allStatic = staticTilemap.GetTilesBlock(_bounds);
        for (int x = 0; x < _bounds.size.x; x++) {
            for (int y = 0; y < _bounds.size.y; y++) {
                var pos = new Vector3Int(x, y, 0) + _bounds.position;
                TileBase tile = allStatic[x + y * _bounds.size.x];
                if (tile == null) continue;
                if (tile is LightTile) {
                    staticTilemap.SetTile(pos, null);
                    var light = Instantiate(staticLightPrefab);
                    light.transform.position = Grid.GetCellCenterWorld(pos);
                    light.GetComponent<SpriteRenderer>().sprite = (tile as LightTile).sprite;
                    _staticLights.Add(pos);                    
                }
            }
        }
        staticTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        TileBase[] allMovables = movablesTilemap.GetTilesBlock(_bounds);
        for (int x = 0; x < _bounds.size.x; x++) {
            for (int y = 0; y < _bounds.size.y; y++) {
                var pos = new Vector3Int(x, y, 0) + _bounds.position;
                TileBase tile = allMovables[x + y * _bounds.size.x];
                if (tile == null) continue;
                movablesTilemap.SetTile(pos, null);
                var movable = Instantiate(movablePrefab).GetComponent<MovableObject>();
                movable.transform.position = Grid.GetCellCenterWorld(pos);
                movable.transform.parent = movableObjectsContainer.transform;
                movable.GetComponent<SpriteRenderer>().sprite = (tile as Tile).sprite;
                if (tile is LightTile) {
                    _movableLights.Add(movable);
                    Destroy(movable.GetComponent<Collider2D>());
                }
            }
        }
        TileBase[] allDecorLightSources = decorLightSourcesTilemap.GetTilesBlock(_bounds);
        for (int x = 0; x < _bounds.size.x; x++) {
            for (int y = 0; y < _bounds.size.y; y++) {
                TileBase tile = allDecorLightSources[x + y * _bounds.size.x];
                if (tile == null) continue;
                if (tile is LightTile) {
                    var tilePos = new LightTilePos() { tileBase = tile as LightTile, position = new Vector3Int(x, y, 0) + _bounds.position };
                    _decorLights.Add(tilePos);
                }
            }
        }
        foreach (var movable in movableObjectsContainer.GetComponentsInChildren<MovableObject>()) {
            movable.Init(this);
            MovableObjects.Add(movable);
        }
        ScheduleCalculateLight(0.5f);
    }
    public bool ContainsStaticObject(Vector3Int cell) {
        return staticTilemap.GetTile(cell) != null || _staticLights.Any(light => light == cell);
    }
    public bool ContainsMovableObject(Vector3Int cell, out MovableObject movableObject) {
        movableObject = MovableObjects.Find(mo => mo.Position == cell);
        return movableObject != null;
    }
    public bool IsLit(Vector3Int cell) {
        return lightTilemap.GetColor(cell) == litColor;
    }
    public void ScheduleCalculateLight(float duration, int times = 6) {
        StartCoroutine(ScheduleCalculateLightCoroutine(duration, times));
    }
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
        if (_decorLights.All(light => {
            var pointPos = Grid.GetCellCenterWorld(cell);
            var lightPos = Grid.GetCellCenterWorld(light.position);
            return !Sees(pointPos, lightPos);
        }) && _movableLights.All(light => {
            var pointPos = Grid.GetCellCenterWorld(cell);
            var lightPos = Grid.GetCellCenterWorld(light.Position);
            return !Sees(pointPos, lightPos);
        })&& _staticLights.All(light => {
            var pointPos = Grid.GetCellCenterWorld(cell);
            var lightPos = Grid.GetCellCenterWorld(light);
            return !Sees(pointPos, lightPos);
        }))
            SetLightTile(cell, shadowColor);
        else
            SetLightTile(cell, litColor);
    }
    bool Sees(Vector3 point, Vector3 other) {
        var checkPoints = new List<Vector3>() {
            point + new Vector3(1,1) * 0.25f,
            point + new Vector3(-1,1) * 0.25f,
            point + new Vector3(1,-1) * 0.25f,
            point + new Vector3(-1,-1) * 0.25f
        };
        int hits = 0;
        foreach (var p in checkPoints) {
            var raycastHits = Physics2D.RaycastAll(p, other - p, Vector3.Distance(p, other));
            if (raycastHits.Any(hit => hit.collider != null && hit.transform.tag == "LightBlock")) {
                //foreach (var hit in raycastHits) Debug.Log(hit.transform.name);
                hits++;
                //Debug.DrawLine(p, p + (other - p).normalized * Vector3.Distance(p, other), Color.green, 0.1f);
            } /*else*/
                //Debug.DrawLine(p, p + (other - p).normalized * Vector3.Distance(p, other), Color.red, 0.1f);
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
    [Serializable]
    struct LightTilePos {
        public LightTile tileBase;
        public Vector3Int position;
    }
    [Serializable]
    struct Line {
        public float a, b;
        public Line(float a, float b) {
            this.a = a;
            this.b = b;
        }
        public bool Above(Vector2 point) => a * point.x + b > point.y;
        public List<Vector2Int> AllPointsBetween(Line other, BoundsInt bounds) {
            var points = new List<Vector2Int>();

            return points;
        }
    }
}

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
    public Tilemap lightBlocksTilemap;
    public Tilemap mapEdgeTilemap;
    public Tilemap transparentTilemap;
    //public Tilemap movablesTilemap;
    public Tile shadowTile;
    public Color litColor;
    public Color shadowColor;
    public Color shadowBlockColor;
    public GameObject otherObjectsContainer;
    public GameObject movablePrefab;
    public Grid Grid { get; private set; }
    public List<GridObject> GridObjects;
    public List<MovableObject> MovableObjects { get; private set; } = new List<MovableObject>();

    public Action Failed;
    public Action Passed;

    BoundsInt _bounds;
    List<LightSource> _lights = new List<LightSource>();
    public void Init() {
        mapEdgeTilemap.CompressBounds();
        _bounds = mapEdgeTilemap.cellBounds;
        var camera = Camera.main;
        camera.orthographicSize = _bounds.size.y / 2f;
        camera.transform.position = _bounds.center + Vector3.back;
        Grid = GetComponent<Grid>();

        foreach (var gridObject in otherObjectsContainer.GetComponentsInChildren<GridObject>()) {
            gridObject.Init(this);
            if (gridObject is MovableObject && (gridObject as MovableObject).movingActive)
                MovableObjects.Add(gridObject as MovableObject);
            if (gridObject is LightSource) _lights.Add(gridObject as LightSource);
            if (gridObject is Character) {
                var character = gridObject as Character;
                character.Failed += () => Failed?.Invoke();
                character.Passed += () => Passed?.Invoke();
            }
        }
        CalculateLight();
        ScheduleCalculateLightCoroutine();
        //ScheduleCalculateLight(0.5f);
    }
    public bool ContainsStaticObject(Vector3Int cell) {
        return mapEdgeTilemap.GetTile(cell) != null ||
            lightBlocksTilemap.GetTile(cell) != null ||
            transparentTilemap.GetTile(cell) != null ||
            _lights.Any(l => l.type == LightSource.Type.Static && l.Position == cell);
    }
    public bool ContainsMovableObject(Vector3Int cell, out MovableObject movableObject) {
        movableObject = MovableObjects.Find(mo => mo.Position == cell);
        return movableObject != null;
    }
    public bool IsLit(Vector3Int cell) => lightTilemap.GetColor(cell) == litColor;
    public bool IsShadowed(Vector3Int cell) => lightTilemap.GetColor(cell) == shadowColor;
    public void ScheduleCalculateLight() =>
        StartCoroutine(ScheduleCalculateLightCoroutine());
    IEnumerator ScheduleCalculateLightCoroutine() {
        yield return null;
        CalculateLight();
    }
    public void CalculateLight() {
        foreach (var point in _bounds.allPositionsWithin) {
            if (lightBlocksTilemap.GetTile(point) != null)
                CalculateLightForLightBlockCell(point);
            else
                CalculateLightForCell(point);
        }
        foreach (var movable in MovableObjects) {
            CalculateLightForMovable(movable);
        }
    }
    void CalculateLightForCell(Vector3Int cell) {
        if (_lights.All(light => !Sees(cell, light)))
            SetLightTile(cell, shadowColor);
        else
            SetLightTile(cell, litColor);
    }
    void CalculateLightForLightBlockCell(Vector3Int cell) {
        if (_lights.All(light => {
            return !SeesFromLightBlock(cell, light);
        }
        ))
            SetLightTile(cell, shadowColor);
        else
            SetLightTile(cell, litColor);
    }
    void CalculateLightForMovable(MovableObject mo) {
        if (mo is Character) return;
        if (mo is LightSource) {
            var ls = mo as LightSource;
            SetMovableColor(mo, ls.Active ? Color.white : shadowBlockColor);
            return;
        }
        if (_lights.All(light => {
            if (light.Position.y >= mo.Position.y) return true;
            else {
                if (mo.TryGetComponent(out Collider2D c)) c.enabled = false;
                var sees = Sees(mo.Position, light);
                if (c) c.enabled = true;
                return !sees;
            }
        }))
            SetMovableColor(mo, shadowBlockColor);
        else
            SetMovableColor(mo, Color.white);
    }
    bool Sees(Vector3Int cell, LightSource light) {
        if (!light.Active) return false;
        var cellPos = Grid.GetCellCenterWorld(cell);
        var lightPos = Grid.GetCellCenterWorld(light.Position);
        var lightBlock = lightBlocksTilemap.GetTile(cell);
        var checkPoints = new List<Vector3>() {
            cellPos,
            cellPos + new Vector3(1,1) * 0.25f,
            cellPos + new Vector3(-1,1) * 0.25f,
            cellPos + new Vector3(1,-1) * 0.25f,
            cellPos + new Vector3(-1,-1) * 0.25f
        };
        //int hits = 0;
        foreach (var p in checkPoints) {
            var distance = Vector3.Distance(p, lightPos);
            if (light.radius > 0 && distance > light.radius) {
                //hits++;
                continue;
            }
            var raycastHits = Physics2D.RaycastAll(p, lightPos - p, distance);
            if (raycastHits.Any(hit => {
                Debug.DrawRay(hit.point, hit.normal, Color.red, 5f);
                return hit.collider != null && hit.transform.tag == "LightBlock";
            })) {

                //hits++;
                continue;
            }
            return true;
        }
        //return hits <= 4;
        return false;
    }
    bool SeesFromLightBlock(Vector3Int cell, LightSource light) {
        if (!light.Active) return false;
        var cellPos = Grid.GetCellCenterWorld(cell);
        var lightPos = Grid.GetCellCenterWorld(light.Position);
        var lightBlock = lightBlocksTilemap.GetTile(cell);
        var checkPoints = new List<Vector3>() {
            cellPos + new Vector3(1,-2.01f) * 0.25f,
            cellPos + new Vector3(-1,-2.01f) * 0.25f
        };
        //int hits = 0;
        foreach (var p in checkPoints) {
            var distance = Vector3.Distance(p, lightPos);
            if (light.radius > 0 && distance > light.radius) {
                //hits++;
                continue;
            }
            var raycastHits = Physics2D.RaycastAll(p, lightPos - p, distance);
            if (raycastHits.Any(hit => hit.collider != null && hit.transform.tag == "LightBlock")) {
                continue;
            }
            return true;
        }
        return false;
    }
    async void SetLightTile(Vector3Int cell, Color color) {
        if (lightTilemap.GetTile(cell) == null) {
            lightTilemap.SetTile(cell, shadowTile);
            lightTilemap.SetTileFlags(cell, TileFlags.None);
            lightTilemap.SetColor(cell, color);
            var blockCell = lightBlocksTilemap.GetTile(cell);
            if (blockCell != null) {
                lightBlocksTilemap.SetTileFlags(cell, TileFlags.None);
                lightBlocksTilemap.SetColor(cell, color == litColor ? Color.white : shadowBlockColor);
            }
            var transparentCell = transparentTilemap.GetTile(cell);
            if (transparentCell != null) {
                transparentTilemap.SetTileFlags(cell, TileFlags.None);
                transparentTilemap.SetColor(cell, color == litColor ? Color.white : shadowBlockColor);
            }
            return;
        }
        if (color != litColor && lightTilemap.GetColor(cell) == litColor) {
            var blockCell = lightBlocksTilemap.GetTile(cell);
            if (blockCell != null) DOTween.To(() => lightBlocksTilemap.GetColor(cell), (c) => lightBlocksTilemap.SetColor(cell, c), shadowBlockColor, 0.1f);
            var transparentCell = transparentTilemap.GetTile(cell);
            if (transparentCell != null) DOTween.To(() => transparentTilemap.GetColor(cell), (c) => transparentTilemap.SetColor(cell, c), shadowBlockColor, 0.1f);
            await DOTween.To(() => lightTilemap.GetColor(cell), (c) => lightTilemap.SetColor(cell, c), shadowColor, 0.1f).AsyncWaitForCompletion();
        } else if (color != shadowColor && lightTilemap.GetColor(cell) == shadowColor) {
            var blockCell = lightBlocksTilemap.GetTile(cell);
            if (blockCell != null) DOTween.To(() => lightBlocksTilemap.GetColor(cell), (c) => lightBlocksTilemap.SetColor(cell, c), Color.white, 0.1f);
            var transparentCell = transparentTilemap.GetTile(cell);
            if (transparentCell != null) DOTween.To(() => transparentTilemap.GetColor(cell), (c) => transparentTilemap.SetColor(cell, c), Color.white, 0.1f);
            await DOTween.To(() => lightTilemap.GetColor(cell), (c) => lightTilemap.SetColor(cell, c), litColor, 0.1f).AsyncWaitForCompletion();
        }
    }
    async void SetMovableColor(MovableObject mo, Color color) {
        var sprite = mo.GetComponent<SpriteRenderer>();
        if (!sprite) return;
        await DOTween.To(() => sprite.color, (c) => sprite.color = color, color, 0.1f).AsyncWaitForCompletion();
    }
}

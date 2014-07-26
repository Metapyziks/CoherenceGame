using UnityEngine;

using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public Puzzle Puzzle { get; private set; }

    public int Width { get { return Puzzle.Width; } }

    public int Height { get { return Puzzle.Height; } }

    public Material BlankMaterial;
    public Material WallMaterial;

    public Material SimpleBlankMaterial;
    public Material SimpleWallMaterial;
    
    public Material ComputronMaterial;
    public Material SimpleComputronMaterial;

    public Material OverviewBoundsMaterial;
    public Material DividerShadowMaterial;
    public Material BackPlaneMaterial;

    public Camera MainCamera;
    public Camera OverviewCamera;

    private GameObject _overviewBounds;
    private GameObject _dividerShadow;
    private GameObject _backPlane;

    private GameObject[] _tiles;

    private List<Computron> _computrons;

    private IEnumerator<Spin[]> _inputIter;

    private bool _touching;
    private bool _placing;
    private bool _dragging;
    private bool _makeSolid;

    public Tile[] InputTiles { get; private set; }

    public Tile[] OutputTiles { get; private set; }

    public float Delta { get; private set; }

    public int Steps { get; private set; }

    public Tile this[int x, int y]
    {
        get
        {
            if (x < 0) x = 0;
            else if (x >= Width) x = Width - 1;

            if (y < 0) y = 0;
            else if (y >= Height) y = Height - 1;

            return _tiles[x + y * Width].GetComponent<Tile>();
        }
    }
    
    void LoadPuzzle(Puzzle puzzle)
    {
        if (_tiles != null) {
            foreach (var tile in _tiles) {
                Destroy(tile);
            }

            foreach (var computron in _computrons) {
                Destroy(computron);
            }
        }

        Delta = 0f;
        Steps = 0;

        Puzzle = puzzle;

        _inputIter = Puzzle.GenerateInputs(null, 0).GetEnumerator();

        _tiles = new GameObject[Width * Height];
        _computrons = new List<Computron>();

        OverviewCamera.orthographicSize = Mathf.Max(Height / 2f, Width / 2f / OverviewCamera.aspect) + 0.5f;

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                var tile = _tiles[x + y * Width] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(tile.GetComponent<MeshCollider>());
                tile.transform.position = new Vector3(x - Width / 2f + .5f, y - Height / 2f + .5f);

                var tComp = tile.AddComponent<Tile>();
                tComp.Level = this;
                tComp.X = x;
                tComp.Y = y;
                tComp.IsEditable = x != 0 && y != 0 && x != Width - 1 && y != Height - 1;
                tComp.IsSolid = true;
            }
        }

        InputTiles = new Tile[Puzzle.InputCount];
        for (int i = 0; i < Puzzle.InputCount; ++i) {
            int y = Puzzle.InputLocations[i];

            InputTiles[i] = this[0, y];

            this[0, y].IsSolid = false;
            this[1, y].IsSolid = false;

            this[1, y].IsEditable = false;
        }

        OutputTiles = new Tile[Puzzle.OutputCount];
        for (int i = 0; i < Puzzle.OutputCount; ++i) {
            int y = Puzzle.OutputLocations[i];

            OutputTiles[i] = this[0, y];

            this[Width - 1, y].IsSolid = false;
            this[Width - 2, y].IsSolid = false;

            this[Width - 2, y].IsEditable = false;
        }

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                this[x, y].FindNeighbours();
            }
        }
        
        _backPlane.transform.localScale = new Vector3(
            OverviewCamera.orthographicSize * OverviewCamera.aspect * 2,
            OverviewCamera.orthographicSize * 2, 1);
    }

    void Start()
    {
        _overviewBounds = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_overviewBounds.GetComponent<MeshCollider>());
        _overviewBounds.layer = LayerMask.NameToLayer("Overview");
        _overviewBounds.transform.position = new Vector3(
            MainCamera.transform.position.x,
            MainCamera.transform.position.y,
            -2
        );
        _overviewBounds.transform.localScale = new Vector3(
            MainCamera.orthographicSize * MainCamera.aspect * 2,
            MainCamera.orthographicSize * 2,
            1
        );
        _overviewBounds.renderer.material = OverviewBoundsMaterial;

        _dividerShadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_dividerShadow.GetComponent<MeshCollider>());
        _dividerShadow.layer = LayerMask.NameToLayer("Main View");
        _dividerShadow.transform.localScale = new Vector3(0.25f, MainCamera.orthographicSize * 2f, 1f);
        _dividerShadow.renderer.material = DividerShadowMaterial;

        _backPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_backPlane.GetComponent<MeshCollider>());
        _backPlane.layer = LayerMask.NameToLayer("Overview");
        _backPlane.renderer.material = BackPlaneMaterial;
        _backPlane.renderer.sortingOrder = -1;

        LoadPuzzle(Puzzle.GetPuzzlesInCategory("Test").First());

        SetCameraPosition(MainCamera, new Vector2(-Width / 2f, InputTiles.Average(x => x.transform.position.y)));
    }

    public Computron CreateComputron(Tile tile, Direction dir, Spin state)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<MeshCollider>());
        quad.renderer.material = ComputronMaterial;

        var comp = quad.AddComponent<Computron>();
        comp.State = comp.NextState = state;
        comp.Direction = comp.NextDirection = dir;
        comp.Tile = tile;

        _computrons.Add(comp);

        return comp;
    }

    void SetCameraPosition(Camera camera, Vector2 position)
    {
        float width = camera.aspect * camera.orthographicSize + 0.5f;
        float height = camera.orthographicSize + 0.5f;

        if (position.x < -Width / 2f + width) position.x = -Width / 2f + width;
        if (position.x > Width / 2f - width) position.x = Width / 2f - width;

        if (position.y < -Height / 2f + height) position.y = -Height / 2f + height;
        if (position.y > Height / 2f - height) position.y = Height / 2f - height;

        camera.transform.position = new Vector3(position.x, position.y, -10);
        _overviewBounds.transform.position = new Vector3(position.x, position.y, -5);
        _dividerShadow.transform.position = new Vector3(position.x + width - 0.5f - 0.125f, position.y, -5);
    }

    bool IsPosWithinCameraBounds(Camera camera, Vector3 pos)
    {
        float l = camera.transform.position.x - camera.orthographicSize * camera.aspect;
        float r = camera.transform.position.x + camera.orthographicSize * camera.aspect;
        float t = camera.transform.position.y - camera.orthographicSize;
        float b = camera.transform.position.y + camera.orthographicSize;

        return pos.x >= l && pos.x <= r && pos.y >= t && pos.y <= b;
    }

    void Touch(Vector2 screenPos)
    {
        var levelPos = OverviewCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

        if (_dragging || (!_touching && IsPosWithinCameraBounds(OverviewCamera, levelPos))) {
            _dragging = true;
            SetCameraPosition(MainCamera, new Vector2(levelPos.x, levelPos.y));
            return;
        }

        levelPos = MainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

        if (_placing || (!_touching && IsPosWithinCameraBounds(MainCamera, levelPos))) {
            var tile = _tiles.OrderBy(t => (t.transform.position - levelPos).magnitude).First().GetComponent<Tile>();

            if (tile.IsEditable) {
                if (!_placing) {
                    _placing = true;
                    _makeSolid = !tile.IsSolid;
                }

                if (tile.IsSolid != _makeSolid) {
                    tile.IsSolid = _makeSolid;

                    for (int x = tile.X - 1; x <= tile.X + 1; ++x) {
                        for (int y = tile.Y - 1; y <= tile.Y + 1; ++y) {
                            this[x, y].FindNeighbours();
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if (Input.touchCount > 0) {
            Touch(Input.touches[0].position);
            _touching = true;
        } else if (Input.GetMouseButton(0)) {
            Touch(Input.mousePosition);
            _touching = true;
        } else {
            _touching = false;
            _placing = false;
            _dragging = false;
        }
    }

    void FixedUpdate()
    {
        Delta += 2f / 60f;

        if (Delta >= 1f) {
            Delta = 0f;
            ++Steps;

            var updates = new Dictionary<Tile, List<Computron>>();

            foreach (var comp in _computrons) {
                var tile = comp.Tile.GetNeighbour(comp.Direction);
                comp.Tile = tile;

                if (!updates.ContainsKey(tile)) {
                    updates.Add(tile, new List<Computron>());
                }

                updates[tile].Add(comp);
            }

            foreach (var pair in updates) {
                if (pair.Key.IsSolid) {
                    pair.Value.ForEach(x => x.Remove());
                } else {
                    pair.Key.ProcessComputrons(pair.Value);
                }
            }

            var removed = _computrons
                .Where(x => x.Removed)
                .ToArray();

            foreach (var comp in removed) {
                _computrons.Remove(comp);
                Destroy(comp.gameObject);
            }

            foreach (var comp in _computrons) {
                comp.Direction = comp.NextDirection;
                comp.State = comp.NextState;
            }

            if (Steps % (Puzzle.InputPeriod * 2) == 0 && _inputIter.MoveNext()) {
                var input = _inputIter.Current;

                for (int i = 0; i < input.Length; ++i) {
                    CreateComputron(InputTiles[i], Direction.Right, input[i]);
                }
            }
        }
    }
}

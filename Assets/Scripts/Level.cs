using UnityEngine;

using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public int Width;
    public int Height;

    public int Inputs;
    public int Outputs;

    public Material BlankMaterial;
    public Material WallMaterial;

    public Material SimpleBlankMaterial;
    public Material SimpleWallMaterial;
    
    public Material ComputronMaterial;
    public Material SimpleComputronMaterial;

    public Material OverviewBoundsMaterial;

    public Camera MainCamera;
    public Camera OverviewCamera;

    private GameObject _overviewBounds;

    private GameObject[] _tiles;

    private List<Computron> _computrons;

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

    void Start()
    {
        _tiles = new GameObject[Width * Height];
        _computrons = new List<Computron>();

        OverviewCamera.orthographicSize = Mathf.Max(Height / 2f, Width / 2f / OverviewCamera.aspect) + 0.5f;
        
        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                var tile = _tiles[x + y * Width] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(x - Width / 2f + .5f, y - Height / 2f + .5f);

                var tComp = tile.AddComponent<Tile>();
                tComp.Level = this;
                tComp.X = x;
                tComp.Y = y;
                tComp.IsEditable = x != 0 && y != 0 && x != Width - 1 && y != Height - 1;
                tComp.IsSolid = true;
            }
        }

        InputTiles = new Tile[Inputs];
        int from = Height / 2 - Inputs;
        for (int i = 0; i < Inputs; ++i) {
            int y = i * 2 + from;

            InputTiles[i] = this[0, y];

            this[0, y].IsSolid = false;
            this[1, y].IsSolid = false;

            this[1, y].IsEditable = false;
        }

        OutputTiles = new Tile[Outputs];
        from = Height / 2 - Outputs;
        for (int i = 0; i < Outputs; ++i) {
            int y = i * 2 + from;

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

        Delta = 0f;

        _overviewBounds = GameObject.CreatePrimitive(PrimitiveType.Quad);
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

        SetCameraPosition(MainCamera, new Vector2(-Width / 2f, InputTiles.Average(x => x.transform.position.y)));
    }

    public Computron CreateComputron(Tile tile, Direction dir, Spin state)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Quad);
        part.renderer.material = ComputronMaterial;

        var comp = part.AddComponent<Computron>();
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
    }

    void Update()
    {
        bool touched = false;
        Vector2 touchPos = Vector2.zero;

        if (Input.touchCount > 0) {
            touched = true;
            touchPos = Input.touches[0].position;
        } else if (Input.GetMouseButton(0)) {
            touched = true;
            touchPos = Input.mousePosition;
        } else {
            _placing = false;
            _dragging = false;
        }

        if (touched) {
            var levelPos = OverviewCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0));

            if (!_placing && (_dragging || (levelPos.x >= -Width / 2f && levelPos.y >= -Height / 2f
                && levelPos.x < Width / 2f && levelPos.y < Height / 2f))) {

                _dragging = true;
                SetCameraPosition(MainCamera, new Vector2(levelPos.x, levelPos.y));
            } else {
                levelPos = MainCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0));

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
    }

    void FixedUpdate()
    {
        Delta += 1f / 60f;

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
                GameObject.Destroy(comp.gameObject);
            }

            foreach (var comp in _computrons) {
                comp.Direction = comp.NextDirection;
                comp.State = comp.NextState;
            }

            if (Steps % 4 == 1) {
                int config = Steps / 4;
                for (int i = 0; i < Inputs; ++i) {
                    CreateComputron(InputTiles[i], Direction.Right,
                        ((config >> i) & 1) == 0 ? Spin.Down : Spin.Up);
                }
            }
        }
    }
}

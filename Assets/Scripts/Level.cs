using UnityEngine;

using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public int Width;
    public int Height;

    public Material BlankMaterial;
    public Material WallMaterial;

    public Material ComputronMaterial;

    public Camera MainCamera;

    private GameObject[] _tiles;

    private List<Computron> _computrons;

    private bool _touching;

    public float Delta { get; set; }

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

        var aspect = Screen.width / (float) Screen.height;
        var width = MainCamera.orthographicSize * aspect * 2;
        var height = MainCamera.orthographicSize * 2;

        var back = GameObject.CreatePrimitive(PrimitiveType.Quad);
        back.transform.position = new Vector3(0, 0, 1);
        back.transform.localScale = new Vector3(width, height);
        back.renderer.material = WallMaterial;
        back.AddComponent<Tile>().IsSolid = true;

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                var tile = _tiles[x + y * Width] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(x - Width / 2f + .5f, y - Height / 2f + .5f);

                var tComp = tile.AddComponent<Tile>();
                tComp.Level = this;
                tComp.X = x;
                tComp.Y = y;
                tComp.IsEditable = x != 0 && y != 0 && x != Width - 1 && y != Height - 1;
                tComp.IsSolid = !tComp.IsEditable;
            }
        }

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                this[x, y].FindNeighbours();
            }
        }

        Delta = 0.5f;

        CreateComputron(this[2, 3], Direction.Down, Spin.Up);
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
            _touching = false;
        }

        if (touched && !_touching && Camera.current != null) {
            _touching = true;

            var levelPos = Camera.current.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0));

            var tile = _tiles.OrderBy(t => (t.transform.position - levelPos).magnitude).First().GetComponent<Tile>();

            if (tile.IsEditable) {
                tile.IsSolid = !tile.IsSolid;

                for (int x = tile.X - 1; x <= tile.X + 1; ++x) {
                    for (int y = tile.Y - 1; y <= tile.Y + 1; ++y) {
                        this[x, y].FindNeighbours();
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
                pair.Key.ProcessComputrons(pair.Value);
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
        }
    }
}

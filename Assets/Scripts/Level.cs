using UnityEngine;

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public enum PulseMode
{
    None = 0,
    Single = 1,
    Continuous = 2
}

public class Level : MonoBehaviour
{
    private RenderTexture _overviewRT;

    private GameObject _overviewBounds;
    private GameObject _dividerShadow;

    private GameObject[] _tiles;
    private GameObject[] _arrows;

    private List<Computron> _computrons;

    private bool _overviewInvalid;

    private IEnumerator<Spin[]> _inputIter;
    private Spin[][] _inputQueue;
    private List<Spin[]> _pastInputs;
    private List<Spin[]> _pastOutputs;

    private bool _touching;
    private bool _placing;
    private bool _dragging;
    private bool _makeSolid;

    public Puzzle Puzzle { get; private set; }

    public int Width { get { return Puzzle.Width; } }

    public int Height { get { return Puzzle.Height; } }

    public Material OverviewMaterial;

    public Material BlankMaterial;
    public Material WallMaterial;

    public Material SimpleBlankMaterial;
    public Material SimpleWallMaterial;
    
    public Material ComputronMaterial;
    public Material SimpleComputronMaterial;

    public Material OverviewBoundsMaterial;
    public Material DividerShadowMaterial;
    public Material BackPlaneMaterial;

    public Material ArrowMaterial;

    public GameObject TilePrefab;

    public Camera MainCamera;
    public Camera OverviewBackCamera;
    public Camera OverviewCamera;

    public MenuPanel MenuPanel;

    public Tile[] InputTiles { get; private set; }

    public Tile[] OutputTiles { get; private set; }

    public bool IsRunning { get; private set; }

    public bool IsTesting { get; private set;}

    public float StepSpeed { get; set; }

    public PulseMode PulseMode { get; set; }

    public float Delta { get; private set; }

    public int Steps { get; private set; }

    public int ExpectedOutputs
    {
        get
        {
            return _inputQueue == null ? 0 : Puzzle.GetExpectedOutputCount(_inputQueue);
        }
    }

    public int CompletedOutputs
    {
        get
        {
            return _pastOutputs == null ? 0 : _pastOutputs.Count;
        }
    }

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
        
    public void SavePuzzle()
    {
        try {
            BinarySaveLoad.SaveArray(Puzzle.SaveKeyName,
                _tiles.Select(x => x.GetComponent<Tile>().IsSolid).ToArray());
        } catch {
            Debug.Log("Error encountered when trying to save \"" + Puzzle.SaveKeyName + "\"");
        }
    }

    public bool LoadSave()
    {
        try {
            var states = BinarySaveLoad.LoadBooleanArray(Puzzle.SaveKeyName, _tiles.Length);
            if (states == null) return false;

            for (int i = 0; i < _tiles.Length; ++i) {
                var tile = _tiles[i].GetComponent<Tile>();

                if (tile.IsEditable) tile.IsSolid = states[i];
            }

            for (int x = 0; x < Width; ++x) {
                for (int y = 0; y < Height; ++y) {
                    this[x, y].FindNeighbours();
                }
            }

            return true;
        } catch {
            Debug.Log("Error encountered when trying to load \"" + Puzzle.SaveKeyName + "\"");
        }

        return false;
    }

    public void ReloadPuzzle()
    {
        LoadPuzzle(Puzzle);
    }

    public void LoadPuzzle(string category, int index)
    {
        LoadPuzzle(Puzzle.GetPuzzlesInCategory(category)[index]);
    }
    
    public void LoadPuzzle(Puzzle puzzle)
    {
        if (IsTesting) StopTesting();

        if (Puzzle != null) {
            SavePuzzle();

            foreach (var arrow in _arrows) {
                Destroy(arrow.gameObject);
            }

            foreach (var computron in _computrons) {
                Destroy(computron.gameObject);
            }

            var oldTiles = _tiles;

            _tiles = new GameObject[puzzle.Width * puzzle.Height];

            for (int i = 0; i < oldTiles.Length; ++i) {
                if (i < _tiles.Length) {
                    _tiles[i] = oldTiles[i];
                } else {
                    Destroy(oldTiles[i].gameObject);
                }
            }
        }

        Delta = 0f;
        Steps = 0;

        IsRunning = false;

        Puzzle = puzzle;

        _inputIter = Puzzle.GenerateInputs(null, 0).GetEnumerator();

        _tiles = _tiles ?? new GameObject[Width * Height];
        _computrons = new List<Computron>();

        var mapAspect = MenuPanel.MenuCamera.aspect / MenuPanel.MapRelativeSize;
        
        OverviewBackCamera.aspect = mapAspect;
        OverviewBackCamera.orthographicSize = Mathf.Max(Height / 2f, Width / 2f / mapAspect);

        OverviewCamera.aspect = mapAspect;
        OverviewCamera.orthographicSize = OverviewBackCamera.orthographicSize;
        OverviewCamera.rect = new Rect(
            0.75f + 0.05f * 0.25f,
            (1f - MenuPanel.MapRelativeSize) + 0.05f * MenuPanel.MapRelativeSize,
            0.25f * 0.9f,
            MenuPanel.MapRelativeSize * 0.9f
        );

        int rtWidth = Mathf.RoundToInt(OverviewCamera.pixelWidth);
        int rtHeight = Mathf.RoundToInt(OverviewCamera.pixelHeight);

        if (_overviewRT != null && (_overviewRT.width != rtWidth || _overviewRT.height != rtHeight)) {
            _overviewRT.Release();
            Destroy(_overviewRT);

            _overviewRT = null;
        }

        if (_overviewRT == null) {
            _overviewRT = new RenderTexture(rtWidth, rtHeight, 0);
        }

        OverviewBackCamera.targetTexture = _overviewRT;
        OverviewMaterial.mainTexture = _overviewRT;

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                var tile = _tiles[x + y * Width];
                var pos = new Vector3(x - Width / 2f + .5f, y - Height / 2f + .5f);

                if (tile == null) {
                    tile = (GameObject) Instantiate(TilePrefab, pos, Quaternion.identity);
                    _tiles[x + y * Width] = tile;
                } else {
                    tile.transform.position = pos;
                }

                var tComp = tile.GetComponent<Tile>();
                tComp.Level = this;
                tComp.X = x;
                tComp.Y = y;
                tComp.IsEditable = x != 0 && y != 0 && x != Width - 1 && y != Height - 1;
                tComp.IsSolid = true;
            }
        }

        _arrows = new GameObject[Puzzle.InputCount + Puzzle.OutputCount];

        InputTiles = new Tile[Puzzle.InputCount];
        for (int i = 0; i < Puzzle.InputCount; ++i) {
            int y = Puzzle.InputLocations[i];

            InputTiles[i] = this[0, y];

            this[0, y].IsSolid = false;
            this[1, y].IsSolid = false;

            this[1, y].IsEditable = false;

            var arr = _arrows[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(arr.GetComponent<MeshCollider>());
            arr.transform.position = this[0, y].transform.position + new Vector3(0.5f, 0.0f);
            arr.layer = LayerMask.NameToLayer("Shared View");
            arr.renderer.sharedMaterial = ArrowMaterial;
            arr.renderer.sortingOrder = 0;
        }

        OutputTiles = new Tile[Puzzle.OutputCount];
        for (int i = 0; i < Puzzle.OutputCount; ++i) {
            int y = Puzzle.OutputLocations[i];

            OutputTiles[i] = this[Width - 1, y];

            this[Width - 1, y].IsSolid = false;
            this[Width - 2, y].IsSolid = false;

            this[Width - 2, y].IsEditable = false;

            var arr = _arrows[Puzzle.InputCount + i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(arr.GetComponent<MeshCollider>());
            arr.transform.position = this[Width - 1, y].transform.position - new Vector3(0.5f, 0.0f);
            arr.layer = LayerMask.NameToLayer("Shared View");
            arr.renderer.sharedMaterial = ArrowMaterial;
            arr.renderer.sortingOrder = 0;
        }

        if (!LoadSave()) {
            for (int x = 0; x < Width; ++x) {
                for (int y = 0; y < Height; ++y) {
                    this[x, y].FindNeighbours();
                }
            }
        }

        SetCameraPosition(MainCamera, new Vector2(-Width / 2f, InputTiles.Average(x => x.transform.position.y)));

        _overviewInvalid = true;
    }

    void Start()
    {
        PulseMode = PulseMode.Continuous;

        MainCamera.orthographicSize = 6 / MainCamera.aspect;

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
        _overviewBounds.renderer.sortingOrder = 4;

        _dividerShadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_dividerShadow.GetComponent<MeshCollider>());
        _dividerShadow.layer = LayerMask.NameToLayer("Main View");
        _dividerShadow.transform.localScale = new Vector3(0.25f, MainCamera.orthographicSize * 2f, 1f);
        _dividerShadow.renderer.material = DividerShadowMaterial;

        if (PlayerPrefs.HasKey("CategoryName")) {
            var catName = PlayerPrefs.GetString("CategoryName");

            if (Puzzle.GetCategories().Contains(catName)) {
                var index = PlayerPrefs.GetInt("PuzzleIndex");
                if (Puzzle.GetPuzzlesInCategory(catName).Length > index) {
                    if (index == 0 || Puzzle.GetPuzzlesInCategory(catName)[index - 1].Solved) {
                        LoadPuzzle(catName, index);
                    }
                }
            }
        }

        if (Puzzle == null) LoadPuzzle(Puzzle.GetPuzzlesInCategory("Test").First());

        StepSpeed = 2;
    }

    void OnApplicationQuit()
    {
        if (Puzzle != null) {
            SavePuzzle();

            PlayerPrefs.SetString("CategoryName", Puzzle.Category);
            PlayerPrefs.SetInt("PuzzleIndex", Puzzle.Index);
        }
    }

    public Computron CreateComputron(Tile tile, Direction dir, Spin state)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
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

                    _overviewInvalid = true;
                }
            }
        }
    }

    public void StartRunning()
    {
        if (_computrons.Count == 0) {
            Steps = -1;

            if (PulseMode != PulseMode.Continuous) {
                PulseMode = PulseMode.Single;
            }
        }

        IsRunning = true;
    }

    public void StopRunning()
    {
        IsRunning = false;
    }

    public void StartTesting()
    {
        if (IsTesting) return;

        foreach (var comp in _computrons) {
            Destroy(comp.gameObject);
        }

        _computrons.Clear();

        StepSpeed = 8f;
        IsTesting = true;
        PulseMode = PulseMode.Continuous;

        _inputQueue = Puzzle.GenerateInputs(null, 10).ToArray();
        _inputIter = _inputQueue.AsEnumerable<Spin[]>().GetEnumerator();

        _pastInputs = new List<Spin[]>();
        _pastOutputs = new List<Spin[]>();

        StartRunning();
    }

    public void StopTesting()
    {
        if (!IsTesting) return;

        if (_computrons.Count == 0 &&
            _pastInputs.Count == _inputQueue.Length &&
            CompletedOutputs == ExpectedOutputs &&
            Puzzle.ShouldAccept(_inputQueue, _pastOutputs)) {
            Puzzle.Solved = true;
        }

        foreach (var comp in _computrons) {
            Destroy(comp.gameObject);
        }

        _computrons.Clear();

        StepSpeed = 2f;
        IsTesting = false;

        _inputIter = Puzzle.GenerateInputs(null, 0).GetEnumerator();

        StopRunning();
    }

    void Update()
    {
        if (IsTesting) return;

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

        if (_overviewInvalid) {
            _overviewInvalid = false;
            OverviewBackCamera.Render();
        }
    }

    void FixedUpdate()
    {
        if (!IsRunning) return;

        Delta += StepSpeed / 60f;

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

            if (IsTesting && updates.Keys.Any(x => OutputTiles.Contains(x))) {
                _pastOutputs.Add(OutputTiles.Select(t => {
                    var c = _computrons.FirstOrDefault(x => x.Tile == t);
                    return c == null ? Spin.None : c.State;
                }).ToArray());
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

            if (Steps % (Puzzle.InputPeriod * 2) == 0 && PulseMode != PulseMode.None && _inputIter.MoveNext()) {
                var input = _inputIter.Current;

                if (IsTesting) _pastInputs.Add(input);

                if (PulseMode == PulseMode.Single) {
                    PulseMode = PulseMode.None;
                }

                for (int i = 0; i < input.Length; ++i) {
                    if (input[i] == Spin.None) continue;
                    CreateComputron(InputTiles[i], Direction.Right, input[i]);
                }
            }

            if (IsTesting && _computrons.Count == 0 && _pastInputs.Count == _inputQueue.Length) {
                StopTesting();
            }

            if (_computrons.Count == 0 && PulseMode == PulseMode.None) {
                StopRunning();
            }
        }
    }
}

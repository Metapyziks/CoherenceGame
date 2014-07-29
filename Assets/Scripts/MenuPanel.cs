using System.Collections;

using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    public Camera MenuCamera;

    public Material ButtonMaterial;

    public Level Level;

    private GameObject _backPlane;

    private Puzzle _oldPuzzle;

    private GUIText _titleTxt;
    private GUIText _descrTxt;

    private GUIText _passTxt;

    private Button _prevBtn;
    private Button _nextBtn;

    private Button _playBtn;
    private Button _pulseBtn;

    private Button _testBtn;

    private Button _cheatBtn;

    private bool _wasTouching;

    public bool IsPlayerTouching
    {
        get { return Input.touchCount > 0 || Input.GetMouseButton(0); }
    }

    public bool IsFirstTouch { get; private set; }

    public GUIText CreateText(Vector2 origin, TextAnchor anchor, TextAlignment align, int sortingOrder = 0)
    {
        var obj = new GameObject();
        obj.layer = LayerMask.NameToLayer("Menu View");

        var text = obj.AddComponent<GUIText>();
        text.anchor = anchor;
        text.alignment = align;

        PositionElement(text, origin);

        return text;
    }

    public Button CreateButton(Vector2 origin, Vector2 size, int sortingOrder = 0)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.renderer.sortingOrder = sortingOrder;
        quad.renderer.material = ButtonMaterial;

        var comp = quad.AddComponent<Button>();
        comp.MenuPanel = this;

        comp.RelativePosition = origin;
        comp.RelativeSize = size;

        return comp;
    }

    public Vector2 FindRelativeToWorldScale()
    {
        var a = MenuCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var b = MenuCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        return b - a;
    }

    public Vector2 FindRelativeToScreenScale()
    {
        var a = MenuCamera.ViewportToScreenPoint(new Vector3(0, 0, 0));
        var b = MenuCamera.ViewportToScreenPoint(new Vector3(1, 1, 0));

        return b - a;
    }

    public void PositionElement(GUIText elem, Vector2 origin)
    {
        var scale = FindRelativeToScreenScale();

        elem.pixelOffset = new Vector2(
            origin.x * scale.x,
            (1 - origin.y) * scale.y
        );
    }

    public void PositionElement(Button elem, Vector2 origin, Vector2 size)
    {
        var a = MenuCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var b = MenuCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        var scale = b - a;

        origin = new Vector2(
            a.x + origin.x * scale.x,
            b.y - origin.y * scale.y
        );

        size = new Vector2(
            size.x * scale.x,
            size.y * scale.y
        );

        elem.transform.position = new Vector3(origin.x, origin.y, 0);
        elem.transform.localScale = new Vector3(size.x, size.y, 1);
    }

    public float FindWidth(float relWidth)
    {
        return FindRelativeToScreenScale().x * relWidth;
    }

    public Vector2 GetCursorPosition()
    {
        var cp = Input.touchCount > 0
            ? Input.touches[0].position 
            : new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        var vp = MenuCamera.ScreenToViewportPoint(cp);

        return new Vector2(vp.x, 1f - vp.y);
    }

    void Start()
    {
        _backPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_backPlane.GetComponent<MeshCollider>());
        _backPlane.layer = LayerMask.NameToLayer("Menu View");
        _backPlane.renderer.material = Level.BackPlaneMaterial;
        _backPlane.renderer.sortingOrder = -1;
        _backPlane.transform.localScale = new Vector3(
            MenuCamera.orthographicSize * MenuCamera.aspect * 2,
            MenuCamera.orthographicSize * 2, 1);

        _titleTxt = CreateText(new Vector2(0.5f, 0.02f), TextAnchor.UpperCenter, TextAlignment.Center);
        _titleTxt.fontSize = 20;

        _descrTxt = CreateText(new Vector2(0.5f, 0.17f), TextAnchor.MiddleCenter, TextAlignment.Left);
        _descrTxt.fontSize = 14;

        _prevBtn = CreateButton(new Vector2(0.275f, 0.32f), new Vector2(0.425f, 0.1f));
        _prevBtn.Text = "Prev Puzzle";
        _prevBtn.Pressed += (sender, e) => {
            Level.LoadPuzzle(Level.Puzzle.Category, Level.Puzzle.Index - 1);
        };

        _nextBtn = CreateButton(new Vector2(0.725f, 0.32f), new Vector2(0.425f, 0.1f));
        _nextBtn.Text = "Next Puzzle";
        _nextBtn.Pressed += (sender, e) => {
            Level.LoadPuzzle(Level.Puzzle.Category, Level.Puzzle.Index + 1);
        };

        _playBtn = CreateButton(new Vector2(0.275f, 0.44f), new Vector2(0.425f, 0.1f));
        _playBtn.Pressed += (sender, e) => {
            if (Level.IsRunning) {
                Level.StopRunning();
            } else {
                Level.StartRunning();
            }
        };

        _pulseBtn = CreateButton(new Vector2(0.725f, 0.44f), new Vector2(0.425f, 0.1f));
        _pulseBtn.Text = "Single Input";
        _pulseBtn.Pressed += (sender, e) => {
            Level.PulseMode = Level.PulseMode == PulseMode.Continuous
                ? PulseMode.Single
                : PulseMode.Continuous;
        };

        _testBtn = CreateButton(new Vector2(0.5f, 0.56f), new Vector2(0.875f, 0.1f));
        _testBtn.Pressed += (sender, e) => {
            if (!Level.IsTesting) {
                Level.StartTesting();
            } else {
                Level.StopTesting();
            }
        };

        _cheatBtn = CreateButton(new Vector2(0.5f, 0.68f), new Vector2(0.875f, 0.1f));
        _cheatBtn.Text = "[CHEAT] Example Solution";
        _cheatBtn.Pressed += (sender, e) => {
            if (Level.Puzzle.Solution == null) return;

            PlayerPrefs.SetString(Level.Puzzle.SaveKeyName, Level.Puzzle.Solution);
            Level.LoadSave();
        };

        _passTxt = CreateText(new Vector2(0.5f, 0.8f), TextAnchor.MiddleCenter, TextAlignment.Center);
        _passTxt.fontSize = 24;
    }

    void Update()
    {
        if (IsPlayerTouching && !_wasTouching) {
            IsFirstTouch = true;
            _wasTouching = true;
        } else {
            IsFirstTouch = false;

            if (!IsPlayerTouching) _wasTouching = false;
        }

        if (Level != null && Level.Puzzle != _oldPuzzle && Level.Puzzle != null) {
            _oldPuzzle = Level.Puzzle;

            _titleTxt.text = Level.Puzzle.Name;
            _descrTxt.SetTextWithWrapping(Level.Puzzle.Description, FindWidth(0.95f));

            _prevBtn.CanPress = Level.Puzzle.Index > 0;

            _cheatBtn.CanPress = false; // Level.Puzzle.Solution != null;
        }

        _pulseBtn.CanPress = !Level.IsTesting;

        _nextBtn.CanPress = Level.Puzzle.Solved &&
            Level.Puzzle.Index < Puzzle.GetPuzzlesInCategory(Level.Puzzle.Category).Length - 1;

        _pulseBtn.Text = Level.PulseMode != PulseMode.Continuous ? "Single" : "Continuous";
        _playBtn.Text = Level.IsRunning ? "Pause" : "Play";
        _testBtn.Text = Level.IsTesting
            ? "Abort Test"
            : "Test Solution";

        _passTxt.text = Level.IsTesting
            ? "Testing (" + Level.CompletedOutputs + "/" + Level.ExpectedOutputs + ")"
            : Level.Puzzle.Solved ? "SOLVED" : "NOT SOLVED";
        _passTxt.fontSize = 24;
        _passTxt.material.color = Level.Puzzle.Solved
            ? new Color32(0x33, 0xff, 0x33, 0x7f)
            : new Color32(0xff, 0x33, 0x33, 0x7f);
    }
}

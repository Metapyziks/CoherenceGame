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

    private Button _testBtn;

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

        _descrTxt = CreateText(new Vector2(0.5f, 0.12f), TextAnchor.UpperCenter, TextAlignment.Left);
        _descrTxt.fontSize = 14;

        _testBtn = CreateButton(new Vector2(0.25f, 0.3f), new Vector2(0.45f, 0.1f));
    }

    void Update()
    {
        if (Level != null && Level.Puzzle != _oldPuzzle && Level.Puzzle != null) {
            _oldPuzzle = Level.Puzzle;

            _titleTxt.text = Level.Puzzle.Name;
            _descrTxt.SetTextWithWrapping(Level.Puzzle.Description, FindWidth(0.95f));
        }
    }
}

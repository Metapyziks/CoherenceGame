using UnityEngine;

using System.Collections;
using System;

public class Button : MonoBehaviour
{
    private int _colorID;

    private GUIText _text;

    private Vector2 _relPos;
    private Vector2 _relSize;

    private bool _held;

    public bool CanPress { get; set; }

    public MenuPanel MenuPanel { get; set; }

    public Color32 BaseColor = new Color32(0xff, 0xff, 0xff, 0xff);

    public Vector4 DisabledTint = new Vector4(0.3f, 0.3f, 0.3f, 1f);

    public Vector4 DefaultTint = new Vector4(0.4f, 0.4f, 0.4f, 1f);

    public Vector4 HoverTint = new Vector4(0.5f, 0.5f, 0.5f, 1f);

    public Vector4 PressedTint = new Vector4(0.6f, 0.6f, 0.6f, 1f);

    public Vector2 RelativeSize
    {
        get { return _relSize; }
        set
        {
            if (_relSize.Equals(value)) return;

            _relSize = value;

            MenuPanel.PositionElement(this, RelativePosition, _relSize);
        }
    }

    public Vector2 RelativePosition
    {
        get { return _relPos; }
        set
        {
            if (_relPos.Equals(value)) return;

            _relPos = value;

            MenuPanel.PositionElement(this, value, RelativeSize);

            if (_text != null) {
                MenuPanel.PositionElement(_text, value);
            }
        }
    }

    public event EventHandler Pressed;

    public String Text
    {
        get { return _text != null ? _text.text : null; }
        set
        {
            if (value == null && _text != null) {
                Destroy(_text);
                _text = null;
                return;
            }

            if (_text == null) {
                _text = MenuPanel.CreateText(_relPos, TextAnchor.MiddleCenter, TextAlignment.Center);
                _text.anchor = TextAnchor.MiddleCenter;
                _text.alignment = TextAlignment.Center;
                _text.fontSize = 16;

                MenuPanel.PositionElement(_text, RelativePosition);
            }

            if (!_text.text.Equals(value)) {
                _text.text = value;
            }
        }
    }

    public bool IsPlayerHovering
    {
        get
        {
            if (!CanPress) return false;

            var pos = MenuPanel.GetCursorPosition();

            return pos.x >= RelativePosition.x - RelativeSize.x * .5f
                && pos.x <= RelativePosition.x + RelativeSize.x * .5f
                && pos.y >= RelativePosition.y - RelativeSize.y * .5f
                && pos.y <= RelativePosition.y + RelativeSize.y * .5f;
        }
    }

    public bool IsPlayerTouching
    {
        get
        {
            return CanPress && MenuPanel.IsPlayerTouching && IsPlayerHovering;
        }
    }

    void Start()
    {
        CanPress = true;

        _colorID = Shader.PropertyToID("_Color");

        Destroy(GetComponent<MeshCollider>());

        gameObject.layer = LayerMask.NameToLayer("Menu View");
    }

    void Update()
    {
        if (MenuPanel.IsFirstTouch && IsPlayerTouching) {
            _held = true;
        } else {
            _held = _held && IsPlayerHovering;

            if (_held && !MenuPanel.IsPlayerTouching) {
                if (Pressed != null) Pressed(this, new EventArgs());
                _held = false;
            }
        }
    }

    Color32 TintColor(Color32 clr, Vector4 tint)
    {
        return new Color32(
            (byte) Mathf.Clamp(clr.r * tint.x, 0, 255),
            (byte) Mathf.Clamp(clr.g * tint.y, 0, 255),
            (byte) Mathf.Clamp(clr.b * tint.z, 0, 255),
            (byte) Mathf.Clamp(clr.a * tint.w, 0, 255)
        );
    }

    void OnWillRenderObject()
    {
        if (!CanPress) {
            renderer.material.SetColor(_colorID, TintColor(BaseColor, DisabledTint));
        } else if (IsPlayerTouching) {
            renderer.material.SetColor(_colorID, TintColor(BaseColor, PressedTint));
        } else if (IsPlayerHovering) {
            renderer.material.SetColor(_colorID, TintColor(BaseColor, HoverTint));
        } else {
            renderer.material.SetColor(_colorID, TintColor(BaseColor, DefaultTint));
        }
	}
}

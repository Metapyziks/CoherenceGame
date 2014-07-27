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

    public Color32 DisabledColor = new Color32(0x4f, 0x4f, 0x4f, 0xff);

    public Color32 DefaultColor = new Color32(0x66, 0x66, 0x66, 0xff);

    public Color32 HoverColor = new Color32(0x7f, 0x7f, 0x7f, 0xff);

    public Color32 PressedColor = new Color32(0x99, 0x99, 0x99, 0xff);

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

            _text.text = value;
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

    void OnWillRenderObject()
    {
        if (!CanPress) {
            renderer.material.SetColor(_colorID, DisabledColor);
        } else if (IsPlayerTouching) {
            renderer.material.SetColor(_colorID, PressedColor);
        } else if (IsPlayerHovering) {
            renderer.material.SetColor(_colorID, HoverColor);
        } else {
            renderer.material.SetColor(_colorID, DefaultColor);
        }
	}
}

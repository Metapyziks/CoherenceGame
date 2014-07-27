using UnityEngine;

using System.Collections;
using System;

public class Button : MonoBehaviour
{
    private int _colorID;

    private GUIText _text;

    private Vector2 _relPos;
    private Vector2 _relSize;

    public MenuPanel MenuPanel { get; set; }

    public Color32 DefaultColor = new Color32(0x66, 0x66, 0x66, 0x33);

    public Color32 HoverColor = new Color32(0x99, 0x99, 0x99, 0x33);

    public Color32 PressedColor = new Color32(0xcc, 0xcc, 0xcc, 0x33);

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

                MenuPanel.PositionElement(_text, RelativePosition);
            }

            _text.text = value;
        }
    }

    void Start()
    {
        _colorID = Shader.PropertyToID("_Color");

        Destroy(GetComponent<MeshCollider>());

        gameObject.layer = LayerMask.NameToLayer("Menu View");
    }

    void OnWillRenderObject()
    {
        renderer.material.SetColor(_colorID, DefaultColor);
	}
}

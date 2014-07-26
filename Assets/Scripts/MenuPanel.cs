using System.Collections;

using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    public Camera MenuCamera;

    public Level Level;

    private GameObject _backPlane;

    T CreateElement<T>(Vector2 origin, Vector2 size, int sortingOrder = 0)
        where T : Component
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_backPlane.GetComponent<MeshCollider>());
        quad.layer = LayerMask.NameToLayer("Menu View");
        quad.renderer.sortingOrder = sortingOrder;
        var comp = quad.AddComponent<T>();

        PositionQuad(quad, origin, size);

        return comp;
    }

    void PositionQuad(GameObject quad, Vector2 origin, Vector2 size)
    {
        Vector2 offset = new Vector2(
            MenuCamera.orthographicSize * MenuCamera.aspect,
            MenuCamera.orthographicSize
        );
        
        Vector2 scale = new Vector2(
            MenuCamera.orthographicSize * MenuCamera.aspect * 2,
            MenuCamera.orthographicSize * 2
        );

        origin = new Vector2(
            origin.x * scale.x - offset.x,
            origin.y * scale.y - offset.y
        );

        size = new Vector2(
            size.x * scale.x,
            size.y * scale.y
        );

        quad.transform.position = origin + size * 0.5f;
        quad.transform.localScale = size;
    }

    void OnGUI()
    {
        var topLeft = MenuCamera.ViewportToScreenPoint(new Vector3(0f, 1f));
        topLeft.y = Screen.height - topLeft.y;
        
        GUI.Button(new Rect(topLeft.x + 8, topLeft.y + 8, 100, 30), "Button");
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
    }
}

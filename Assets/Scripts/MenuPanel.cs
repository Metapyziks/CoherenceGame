using System.Collections;

using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    public Camera MenuCamera;

    public Level Level;

    private GameObject _backPlane;

    void Start()
    {
        _backPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(_backPlane.GetComponent<MeshCollider>());
        _backPlane.layer = LayerMask.NameToLayer("Menu View");
        _backPlane.renderer.material = Level.BackPlaneMaterial;
        _backPlane.renderer.sortingOrder = 2;
        _backPlane.transform.localScale = new Vector3(
            MenuCamera.orthographicSize * MenuCamera.aspect * 2,
            MenuCamera.orthographicSize * 2, 1);
    }
}

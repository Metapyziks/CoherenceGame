using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    private int _neighboursID;
    private int _colorID;
    private bool _isSolid;
    private bool _invalidMaterial;

    public Level Level;

    public int X;
    public int Y;

    public int Neighbours;
    public bool IsSolid
    {
        get { return _isSolid; }
        set
        {
            if (value != _isSolid) {
                _isSolid = value;
                _invalidMaterial = true;
            }
        }
    }

    void Start()
    {
        _neighboursID = Shader.PropertyToID("_Neighbours");
        _invalidMaterial = true;
    }

    void Update()
    {
        if (Level == null) return;

        if (_invalidMaterial) {
            _invalidMaterial = false;
            if (IsSolid) {
                renderer.material = Level.WallMaterial;
            } else if (!IsSolid) {
                renderer.material = Level.BlankMaterial;
            }
        }
    }

    private bool IsNeighbourEmpty(int x, int y)
    {
        return !Level[x, y].IsSolid;
    }

    public void FindNeighbours()
    {
        Neighbours = 0;

        var neighbours = new[,] {
            { -1, -1 },
            { 0, -1 },
            { 1, -1 },
            { 1, 0 },
            { 1, 1 },
            { 0, 1 },
            { -1, 1 },
            { -1, 0 }
        };

        for (int i = 0; i < 8; ++i) {
            Neighbours |= IsNeighbourEmpty(X + neighbours[i, 0], Y + neighbours[i, 1]) == IsSolid ? 1 << i : 0;
        }
    }

    void OnWillRenderObject()
    {
        renderer.material.SetFloat(_neighboursID, Neighbours / 255f);
    }
}

using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public int Neighbours;
    public bool IsSolid;

    private int _neighboursID;
    private int _colorID;

    void Start()
    {
        _neighboursID = Shader.PropertyToID("_Neighbours");
    }

    private static bool IsNeighbourEmpty(Level level, int x, int y)
    {
        return !level[x, y].GetComponent<Tile>().IsSolid;
    }

    public void FindNeighbours(Level level, int x, int y)
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
            Neighbours |= IsNeighbourEmpty(level, x + neighbours[i, 0], y + neighbours[i, 1]) == IsSolid ? 1 << i : 0;
        }
    }

    void OnWillRenderObject()
    {
        renderer.material.SetFloat(_neighboursID, Neighbours / 255f);
    }
}

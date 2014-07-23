using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public Tile GetNeighbour(Direction dir)
    {
        switch (dir) {
            case Direction.Right:
                return Level[X + 1, Y];
            case Direction.Down:
                return Level[X, Y - 1];
            case Direction.Left:
                return Level[X - 1, Y];
            case Direction.Up:
                return Level[X, Y + 1];
            default:
                return this;
        }
    }

    bool IsBlocked(Direction dir, IEnumerable<Computron> computrons)
    {
        return GetNeighbour(dir).IsSolid || computrons.Any(x => x.Direction == dir.GetBack());
    }

    public void ProcessComputrons(IEnumerable<Computron> computrons)
    {
        int count = computrons.Count();

        foreach (var comp in computrons) {
            if (count >= 4) {
                comp.Remove();
                continue;
            }
            
            var left = comp.Direction.GetLeft();
            var back = comp.Direction.GetBack();
            var right = comp.Direction.GetRight();

            if (IsBlocked(comp.Direction, computrons)) {
                if (count > 1 || (comp.GetLeftTile().IsSolid && comp.GetRightTile().IsSolid)) {
                    comp.Remove();
                } else if (comp.GetLeftTile().IsSolid) {
                    comp.Direction = right;
                } else if (comp.GetRightTile().IsSolid) {
                    comp.Direction = left;
                } else {
                    Level.CreateComputron(this, left, comp.State);
                    comp.Direction = right;
                }
            }
        }
    }
}

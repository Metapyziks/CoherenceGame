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

    public void ProcessComputrons(List<Computron> computrons)
    {
        int count = computrons.Count;

        var orig = computrons.ToArray();

        foreach (var comp in orig) {
            if (count >= 4) {
                comp.Remove();
                continue;
            }
            
            var left = comp.Direction.GetLeft();
            var back = comp.Direction.GetBack();
            var right = comp.Direction.GetRight();

            if (IsBlocked(comp.Direction, computrons)) {
                bool bLeft = IsBlocked(comp.Direction.GetLeft(), computrons);
                bool bRight = IsBlocked(comp.Direction.GetRight(), computrons);

                if (bLeft && bRight) {
                    comp.Remove();
                } else if (bLeft) {
                    comp.NextDirection = right;
                } else if (bRight) {
                    comp.NextDirection = left;
                } else {
                    comp.NextDirection = right;
                    comp.NextState = comp.State == Spin.Up ? Spin.Down : Spin.Up;

                    var pair = Level.CreateComputron(this, left, comp.State);
                    pair.NextState = comp.NextState;

                    computrons.Add(pair);
                }
            }
        }

        foreach (var direc in new[] {
            Direction.Right,
            Direction.Down,
            Direction.Left,
            Direction.Up
        }) {
            var matches = computrons.Where(x => x.NextDirection == direc).ToArray();
            if (matches.Length <= 1) continue;

            var res = matches.All(x => x.State == Spin.Down);

            matches[0].NextState = res ? Spin.Down : Spin.Up;

            for (int i = 1; i < matches.Length; ++i) {
                matches[i].Remove();
            }
        }
    }
}

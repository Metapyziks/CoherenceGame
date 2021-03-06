﻿using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tile : MonoBehaviour
{
    private int _neighboursID;
    
    private bool _isSolid;
    private bool _invalidMaterial;

    private GameObject _overviewDummy;

    public Level Level { get; set; }
    
    public int X { get; set; }
    public int Y { get; set; }

    public int Neighbours { get; set; }
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

    public bool IsEditable { get; set; }

    void Start()
    {
        _neighboursID = Shader.PropertyToID("_Neighbours");
        _invalidMaterial = true;
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
            {  0, -1 },
            {  1, -1 },
            {  1,  0 },
            {  1,  1 },
            {  0,  1 },
            { -1,  1 },
            { -1,  0 }
        };

        for (int i = 0; i < 8; ++i) {
            Neighbours |= IsNeighbourEmpty(X + neighbours[i, 0], Y + neighbours[i, 1]) == IsSolid ? 1 << i : 0;
        }

        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        if (Level == null) return;

        if (_overviewDummy == null) {
            _overviewDummy = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(_overviewDummy.GetComponent<MeshCollider>());
            _overviewDummy.layer = LayerMask.NameToLayer("Overview Back");
            _overviewDummy.renderer.sortingOrder = 0;
        }

        _overviewDummy.transform.position = gameObject.transform.position;

        if (_invalidMaterial) {
            _invalidMaterial = false;
            if (IsSolid) {
                renderer.material = Level.WallMaterial;
                renderer.sortingOrder = 3;
                _overviewDummy.renderer.sharedMaterial = Level.SimpleWallMaterial;
                _overviewDummy.renderer.sortingOrder = 3;
            } else if (!IsSolid) {
                renderer.material = Level.BlankMaterial;
                renderer.sortingOrder = 0;
                _overviewDummy.renderer.sharedMaterial = Level.SimpleBlankMaterial;
                _overviewDummy.renderer.sortingOrder = 0;
            }
        }
    }

    void OnWillRenderObject()
    {
        renderer.material.SetFloat(_neighboursID, Neighbours / 255f);
    }

    public void OnDestroy()
    {
        if (_overviewDummy != null) {
            Destroy(_overviewDummy);
        }
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
            var right = comp.Direction.GetRight();

            if (IsBlocked(comp.Direction, computrons)) {
                bool bLeft = IsBlocked(left, computrons);
                bool bRight = IsBlocked(right, computrons);

                if (bLeft && bRight) {
                    comp.Remove();
                } else if (bLeft) {
                    comp.NextDirection = right;
                } else if (bRight) {
                    comp.NextDirection = left;
                } else {
                    comp.NextDirection = right;

                    var pair = Level.CreateComputron(this, comp.Direction, comp.State);
                    pair.NextDirection = left;

                    computrons.Add(pair);
                }
            } else {
                var other = computrons.FirstOrDefault(x => x.State == Spin.Up
                    && (x.Direction == left || x.Direction == right));

                if (other != null) comp.Remove();
            }
        }

        foreach (var direc in new[] {
            Direction.Right,
            Direction.Down,
            Direction.Left,
            Direction.Up
        }) {
            var matches = computrons.Where(x => !x.Removed && x.NextDirection == direc).ToArray();
            if (matches.Length < 1) continue;

            var res = matches.Any(x => x.State == Spin.Up);

            matches[0].NextState = res ? Spin.Up : Spin.Down;

            if (computrons.Any(x => !x.Removed && x.NextDirection == direc.GetBack())) {
                matches[0].NextState = matches[0].NextState == Spin.Up ? Spin.Down : Spin.Up;
            }

            for (int i = 1; i < matches.Length; ++i) {
                matches[i].Remove();
            }
        }
    }
}

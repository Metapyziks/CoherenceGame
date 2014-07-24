using System.Collections;

using UnityEngine;

public enum Spin
{
    Up = 1,
    Down = 2
}

public enum Direction : byte
{
    Right = 0,
    Down = 1,
    Left = 2,
    Up = 3
}

public static class DirectionExtensions
{
    public static Direction GetRight(this Direction dir)
    {
        return (Direction) (((int) dir + 1) % 4);
    }

    public static Direction GetBack(this Direction dir)
    {
        return (Direction) (((int) dir + 2) % 4);
    }

    public static Direction GetLeft(this Direction dir)
    {
        return (Direction) (((int) dir + 3) % 4);
    }
}

public class Computron : MonoBehaviour
{
    private int _directionID;
    private int _stateID;

    public Spin State { get; set; }

    public Spin NextState { get; set; }

    public Direction Direction { get; set; }

    public Direction NextDirection { get; set; }

    public Tile Tile { get; set; }

    public Level Level { get { return Tile.Level; } }

    public bool Removed { get; private set; }

    void Start()
    {
        _directionID = Shader.PropertyToID("_Direction");
        _stateID = Shader.PropertyToID("_State");
    }

    Vector3 GetMovementVector()
    {
        switch (Direction) {
            case Direction.Right:
                return new Vector3(1, 0, 0);
            case Direction.Down:
                return new Vector3(0, -1, 0);
            case Direction.Left:
                return new Vector3(-1, 0, 0);
            case Direction.Up:
                return new Vector3(0, 1, 0);
            default:
                return new Vector3(0, 0, 0);
        }
    }

    public void Remove()
    {
        Removed = true;
    }

    void OnWillRenderObject()
    {
        renderer.material.SetFloat(_directionID, (float) Direction);
        renderer.material.SetFloat(_stateID, State == Spin.Up ? 1 : 0);

        transform.position = Tile.transform.position + GetMovementVector() * Level.Delta - new Vector3(0, 0, 0.5f);
    }
}

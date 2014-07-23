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

public abstract class ComputronState
{
    public abstract ComputronState Clone();

    public abstract Spin GetSubState(float i);
}

public class UnitState : ComputronState
{
    public Spin Spin { get; set; }

    public UnitState(Spin spin)
    {
        Spin = spin;
    }

    public override ComputronState Clone()
    {
        return new UnitState(Spin);
    }

    public override Spin GetSubState(float i)
    {
        return Spin;
    }
}

public class Superposition : ComputronState
{
    public ComputronState A { get; set; }
    public ComputronState B { get; set; }

    public Superposition(ComputronState a, ComputronState b)
    {
        A = a;
        B = b;
    }

    public override ComputronState Clone()
    {
        return new Superposition(A.Clone(), B.Clone());
    }

    public override Spin GetSubState(float i)
    {
        if (i < 0.5f) {
            return A.GetSubState(i * 2);
        } else {
            return B.GetSubState(i * 2 - 1);
        }
    }
}

public class Computron : MonoBehaviour
{
    private int _directionID;

    private int _stateQ1ID;
    private int _stateQ2ID;
    private int _stateQ3ID;
    private int _stateQ4ID;

    public ComputronState State { get; set; }

    public Direction Direction { get; set; }

    public Tile Tile { get; set; }

    public Level Level { get { return Tile.Level; } }

    public bool Removed { get; private set; }

    void Start()
    {
        _directionID = Shader.PropertyToID("_Direction");

        _stateQ1ID = Shader.PropertyToID("_StateQ1");
        _stateQ2ID = Shader.PropertyToID("_StateQ2");
        _stateQ3ID = Shader.PropertyToID("_StateQ3");
        _stateQ4ID = Shader.PropertyToID("_StateQ4");
    }

    Vector4 GetStateVector(int from)
    {
        return new Vector4(
            State.GetSubState((from + 0f) / 16f) == Spin.Up ? 1 : 0,
            State.GetSubState((from + 1f) / 16f) == Spin.Up ? 1 : 0,
            State.GetSubState((from + 2f) / 16f) == Spin.Up ? 1 : 0,
            State.GetSubState((from + 3f) / 16f) == Spin.Up ? 1 : 0);
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

        renderer.material.SetVector(_stateQ1ID, GetStateVector(0));
        renderer.material.SetVector(_stateQ2ID, GetStateVector(4));
        renderer.material.SetVector(_stateQ3ID, GetStateVector(8));
        renderer.material.SetVector(_stateQ4ID, GetStateVector(12));

        transform.position = Tile.transform.position + GetMovementVector() * Level.Delta - new Vector3(0, 0, 0.5f);
    }
}

using System.Collections;

using UnityEngine;

public enum Spin
{
    None = 0,
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
    public class Dummy : MonoBehaviour
    {
        public Computron Computron { get; set; }

        void Start()
        {
            Destroy(GetComponent<MeshCollider>());

            gameObject.layer = LayerMask.NameToLayer("Overview");
        }

        void Update()
        {
            transform.position = Computron.GetCurrentPosition();
        }

        void OnWillRenderObject()
        {
            if (Computron == null || Computron._isRemoved) return;

            renderer.material.SetFloat(Computron._stateID, Computron.State == Spin.Up ? 1 : 0);
            renderer.sortingOrder = Computron.State == Spin.Up ? 2 : 1;

            transform.position = Computron.GetCurrentPosition();
        }
    }

    private int _directionID;
    private int _stateID;

    private bool _isRemoved;

    private GameObject _overviewDummy;

    public Spin State { get; set; }

    public Spin NextState { get; set; }

    public Direction Direction { get; set; }

    public Direction NextDirection { get; set; }

    public Tile Tile { get; set; }

    public Level Level { get { return Tile.Level; } }

    public bool Removed { get { return _isRemoved || Tile.X == 0 || Tile.X == Level.Width - 1; } }

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Main View");

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

    Vector3 GetCurrentPosition()
    {
        return Tile.transform.position + GetMovementVector() * Level.Delta - new Vector3(0, 0, 2f);
    }

    public void Update()
    {
        transform.position = GetCurrentPosition();

        if (_overviewDummy == null) {
            _overviewDummy = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(_overviewDummy.GetComponent<MeshCollider>());
            _overviewDummy.AddComponent<Dummy>().Computron = this;
            _overviewDummy.renderer.material = Level.SimpleComputronMaterial;
            _overviewDummy.transform.position = transform.position;
        }
    }

    public void OnDestroy()
    {
        _isRemoved = true;
        if (_overviewDummy != null) {
            _overviewDummy.GetComponent<Dummy>().Computron = null;
            Destroy(_overviewDummy);
        }
    }
    
    public void Remove()
    {
        _isRemoved = true;
        Destroy(_overviewDummy);
        _overviewDummy = null;
    }

    void OnWillRenderObject()
    {
        renderer.material.SetFloat(_directionID, (float) Direction);
        renderer.material.SetFloat(_stateID, State == Spin.Up ? 1 : 0);

        renderer.sortingOrder = State == Spin.Up ? 2 : 1;

        transform.position = GetCurrentPosition();
    }
}

using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour
{
    public int Width;
    public int Height;

    public Material BlankMaterial;
    public Material WallMaterial;

    public Camera MainCamera;

    private GameObject[] _tiles;

	void Start ()
    {
        _tiles = new GameObject[Width * Height];

        var aspect = Screen.width / (float) Screen.height;
        var width = MainCamera.orthographicSize * aspect * 2;
        var height = MainCamera.orthographicSize * 2;

        var back = GameObject.CreatePrimitive(PrimitiveType.Quad);
        back.transform.position = new Vector3(0, 0, 1);
        back.transform.localScale = new Vector3(width, height);
        back.renderer.material = WallMaterial;
        back.AddComponent<Tile>().IsSolid = true;

        var rand = new System.Random();

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                bool solid = x == 0 || y == 0 || x == Width - 1 || y == Height - 1 || rand.NextDouble() < 0.25;

                var tile = _tiles[x + y * Width] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(x - Width / 2f + .5f, y - Height / 2f + .5f);
                tile.renderer.material = solid ? WallMaterial : BlankMaterial;
                tile.AddComponent<Tile>().IsSolid = solid;
            }
        }

        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                this[x, y].GetComponent<Tile>().FindNeighbours(this, x, y);
            }
        }
	}

    public GameObject this[int x, int y]
    {
        get
        {
            if (x < 0) x = 0;
            else if (x >= Width) x = Width - 1;

            if (y < 0) y = 0;
            else if (y >= Height) y = Height - 1;

            return _tiles[x + y * Width];
        }
    }
}

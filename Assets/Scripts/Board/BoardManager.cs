using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 8;

    private Tile[,] tiles;

    void Start()

    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        tiles = new Tile[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                tileObject.name = $"Tile {x},{y}";

                tileObject.transform.parent = transform;

                tileObject.transform.position = new Vector3(x, 0, y);

                Tile tile = tileObject.AddComponent<Tile>();

                tile.x = x;
                tile.y = y;

                tiles[x, y] = tile;
            }
        }
    }
}
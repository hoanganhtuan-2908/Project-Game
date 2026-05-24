using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 8;

    private Tile[,] tiles;

    private ChessPiece[,] chessPieces =
        new ChessPiece[8, 8];

    private ChessPiece selectedPiece;

    void Start()
    {
        GenerateBoard();
    }

    void Update()
    {
        HandleMouseInput();
    }

    void GenerateBoard()
    {
        tiles = new Tile[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                GameObject tileObject =
                    GameObject.CreatePrimitive
                    (
                        PrimitiveType.Cube
                    );

                tileObject.name =
                    $"Tile {x},{y}";

                tileObject.transform.parent =
                    transform;

                tileObject.transform.position =
                    new Vector3(x, 0, y);

                tileObject.transform.localScale =
                    new Vector3(1, 0.1f, 1);

                Tile tile =
                    tileObject.AddComponent<Tile>();

                tile.x = x;
                tile.y = y;

                tiles[x, y] = tile;
            }
        }
    }

    void HandleMouseInput()
    {
        // check mouse
        if (Mouse.current == null)
        {
            Debug.Log("MOUSE IS NULL");
            return;
        }

        // check camera
        if (Camera.main == null)
        {
            Debug.Log("MAIN CAMERA IS NULL");
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("CLICK");

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            Ray ray =
                Camera.main.ScreenPointToRay(mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("HIT: " + hit.transform.name);
            }
            else
            {
                Debug.Log("NOTHING HIT");
            }
        }
    }
    void MovePiece
    (
        ChessPiece piece,
        int x,
        int y
    )
    {
        piece.currentX = x;
        piece.currentZ = y;

        piece.transform.position =
            new Vector3(x, 0.5f, y);

        Debug.Log
        (
            piece.name +
            " moved to " +
            x + "," + y
        );
    }
}
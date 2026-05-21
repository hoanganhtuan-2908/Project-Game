using UnityEngine;

public class chess_manager : MonoBehaviour
{
   public Transform[,] board = new Transform[8,8];

    public GameObject piecePrefab;

    void Start()
    {
        SpawnPiece(piecePrefab, 0, 0);
        SpawnPiece(piecePrefab, 1, 0);
    }

    void SpawnPiece(GameObject piece, int x, int y)
    {
        float tileSize = 1f;

        Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);

        Instantiate(piece, pos, Quaternion.identity);
    }
}

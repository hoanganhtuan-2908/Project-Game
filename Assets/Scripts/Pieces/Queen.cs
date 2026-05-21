using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetLegalMoves
    (
        ref ChessPiece[,] board,
        int tileCountX,
        int tileCountY
    )
    {
        List<Vector2Int> legalMoves =
            new List<Vector2Int>();

        int[] directionsX =
        {
            1, -1, 0, 0,
            1, 1, -1, -1
        };

        int[] directionsZ =
        {
            0, 0, 1, -1,
            1, -1, 1, -1
        };

        for (int dir = 0; dir < 8; dir++)
        {
            int x = currentX;
            int z = currentZ;

            while (true)
            {
                x += directionsX[dir];
                z += directionsZ[dir];

                if (x < 0 ||
                    x >= tileCountX ||
                    z < 0 ||
                    z >= tileCountY)
                {
                    break;
                }

                legalMoves.Add
                (
                    new Vector2Int(x, z)
                );
            }
        }

        return legalMoves;
    }
}
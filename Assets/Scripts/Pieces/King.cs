using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
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

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                int targetX = currentX + x;
                int targetZ = currentZ + z;

                if (targetX >= 0 &&
                    targetX < tileCountX &&
                    targetZ >= 0 &&
                    targetZ < tileCountY)
                {
                    legalMoves.Add
                    (
                        new Vector2Int(targetX, targetZ)
                    );
                }
            }
        }

        return legalMoves;
    }

}
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Rook : Piece
{
    private Vector2Int[] directions = new Vector2Int[]
    {
         Vector2Int.left,
         Vector2Int.right,
         Vector2Int.up,
         Vector2Int.down
    };
    public override List<Vector2Int> GetLegalMovesForPiece()
    {
        GetLegalMoves.Clear();
        float range = Board.BOARD_SIZE;
        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = occupiedSquare + direction * i;
                Piece piece = Board.GetPieceOnSquare(nextCoords);
                if (!Board.CheckIfCoordinatedAreOnBoard(nextCoords))
                    break;
                if (piece == null)
                    TryToAddMove(nextCoords);
                else if (!piece.IsFromSameTeam(this))
                {
                    TryToAddMove(nextCoords);
                    break;
                }
                else if (piece.IsFromSameTeam(this))
                
                    break;
                  
            }
            
        }
        return GetLegalMoves;
    }
}


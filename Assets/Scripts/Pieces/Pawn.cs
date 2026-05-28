using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetLegalMovesForPiece()
    {
        GetLegalMoves.Clear();
        Vector2Int direction = Team == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        float range = HasMoved ? 1 : 2;
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = Board.GetPieceOnSquare(nextCoords);
            if (!Board.CheckIfCoordinatedAreOnBoard(nextCoords))
                break;
            if (piece = null)
                TryToAddMove(nextCoords);
            else 
                break;
        }
        Vector2Int[] takeDirections = new Vector2Int[] { new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y) };
        for (int i = 0; i < takeDirections.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + takeDirections[i];
            Piece piece = Board.GetPieceOnSquare(nextCoords);
            if (!Board.CheckIfCoordinatedAreOnBoard(nextCoords))
                continue;
            if (piece != null && !piece.IsFromSameTeam(this))
            {
                TryToAddMove(nextCoords);
            }

        }
        return GetLegalMoves;
    }
}

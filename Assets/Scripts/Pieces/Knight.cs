using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int( 1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2)
    };
    public override List<Vector2Int> GetLegalMovesForPiece()
    {
        GetLegalMoves.Clear();
        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + offsets[i];
            if (!Board.CheckIfCoordinatedAreOnBoard(nextCoords))
                continue;
            Piece piece = Board.GetPieceOnSquare(nextCoords);
            
            if (piece == null || !piece.IsFromSameTeam(this))
                TryToAddMove(nextCoords);
        }
        return GetLegalMoves;
    }
}

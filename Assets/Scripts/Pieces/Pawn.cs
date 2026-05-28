using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetLegalMovesForPiece()
    {
        GetLegalMoves.Clear();
        GetLegalMoves.Add(occupiedSquare + new Vector2Int(0, 1));
        return GetLegalMoves;

    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Rook : Piece
{
    public override List<Vector2Int> GetLegalMovesForPiece()
    {
        GetLegalMoves.Clear();
        GetLegalMoves.Add(occupiedSquare + new Vector2Int(0 ,1));
        return GetLegalMoves;

    }
}

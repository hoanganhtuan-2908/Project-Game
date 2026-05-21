using System.Collections.Generic;
using UnityEngine;

public enum PieceTeam
{
    White,
    Black
}

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public abstract class ChessPiece : MonoBehaviour
{
    public PieceTeam Team;
    public PieceType Type;

    // Logical position (0-7)
    public int currentX;
    public int currentZ; // Z for the second dimension in 3D

    //get legal move
    public abstract List<Vector2Int> GetLegalMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY);

    //handles the 3D visual movement
    public virtual void SetPosition(Vector3 worldPosition, bool updateLogicalPosition = false)
    {
        if (updateLogicalPosition)
        {
            transform.position = worldPosition;
        }
        else
        {
            // Later add code here to make the piece 
            // "slide" or "jump" smoothly to the new 3D spot
        }
    }

}

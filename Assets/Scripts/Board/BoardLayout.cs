using System;
using UnityEngine;

public enum TeamColor
{
    Black, White
}

public enum PieceType
{
    Pawn, Bishop, Knight, Rook, Queen, King
}


[CreateAssetMenu(menuName = "Scriptable Objects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    public class BoardSetup
    {
        public Vector2Int position;
        public TeamColor color;
        public PieceType type;
    }

    [SerializeField] private BoardSetup[] boards;


    public int GetPieceCount()
    {
        return boards.Length;
    }

    public Vector2Int GetPiecePositionAtIndex(int index)
    {
        return new Vector2Int(boards[index].position.x - 1, boards[index].position.y - 1);
    }

    public String GetPieceTypeAtIndex(int index)
    {
        return boards[index].type.ToString();
    }

    public TeamColor GetPieceTeamAtIndex(int index)
    {
        return boards[index].color;
    }

}

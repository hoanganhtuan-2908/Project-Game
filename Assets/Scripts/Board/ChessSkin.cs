using UnityEngine;

[CreateAssetMenu(fileName = "New Chess Skin", menuName = "Chess/Skin")]
public class ChessSkin : ScriptableObject
{
    public string skinName;

    [Header("Đội Trắng (White Team Prefabs)")]
    public GameObject whitePawn;
    public GameObject whiteRook;
    public GameObject whiteKnight;
    public GameObject whiteBishop;
    public GameObject whiteQueen;
    public GameObject whiteKing;

    [Header("Đội Đen (Black Team Prefabs)")]
    public GameObject blackPawn;
    public GameObject blackRook;
    public GameObject blackKnight;
    public GameObject blackBishop;
    public GameObject blackQueen;
    public GameObject blackKing;

    [Header("Chất liệu ghi đè (để trống nếu model đã có màu sẵn)")]
    public Material whiteMaterial;
    public Material blackMaterial;

    /// <summary>
    /// Trả về prefab tương ứng với loại quân cờ và đội.
    /// </summary>
    public GameObject GetPrefab(System.Type type, TeamColor team)
    {
        string typeName = type.Name;

        if (team == TeamColor.White)
        {
            switch (typeName)
            {
                case "Pawn":   return whitePawn;
                case "Rook":   return whiteRook;
                case "Knight": return whiteKnight;
                case "Bishop": return whiteBishop;
                case "Queen":  return whiteQueen;
                case "King":   return whiteKing;
            }
        }
        else
        {
            switch (typeName)
            {
                case "Pawn":   return blackPawn;
                case "Rook":   return blackRook;
                case "Knight": return blackKnight;
                case "Bishop": return blackBishop;
                case "Queen":  return blackQueen;
                case "King":   return blackKing;
            }
        }

        return null;
    }
}

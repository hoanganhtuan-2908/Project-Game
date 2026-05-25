using System;
using UnityEngine;

[RequireComponent(typeof(PieceCreator))]
public class ChessGameController : MonoBehaviour
{
    [SerializeField] private BoardLayout StartingBoardLayout;
    private PieceCreator piecesCreator;
    [SerializeField] private Board board;

    private void Awake()
    {
        piecesCreator = GetComponent<PieceCreator>();
    }

    void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        CreatePiecesFromBoardLayout(StartingBoardLayout);
    }

    private void CreatePiecesFromBoardLayout(BoardLayout Layout)
    {
        for (int i = 0; i < Layout.GetPieceCount(); i++)
        {
            Vector2Int squareCoords = Layout.GetPiecePositionAtIndex(i);
            string typeName = Layout.GetPieceTypeAtIndex(i);
            TeamColor team = Layout.GetPieceTeamAtIndex(i);


            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    private void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = piecesCreator.CreatePieceOfType(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material materialForTeam = piecesCreator.GetMaterialForTeam(team);
        newPiece.SetMaterial(materialForTeam);
    }
}

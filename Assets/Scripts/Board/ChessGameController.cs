using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(PieceCreator))]
public class ChessGameController : MonoBehaviour
{
    [SerializeField] private BoardLayout StartingBoardLayout;
    [SerializeField] private Board board;

    private PieceCreator piecesCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activeplayer;

    private void Awake()
    {
       SetDependencies();
       CreatePlayers(); 

    }
    private void SetDependencies()
    {
        piecesCreator = GetComponent<PieceCreator>();
    }
    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        board.SetDependencies(this);
        CreatePiecesFromLayout(StartingBoardLayout);
        activeplayer = whitePlayer;
        GenerateAllPosiblePlayerMoves(activeplayer);
    }

    private void CreatePiecesFromLayout(BoardLayout Layout)
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
    private void GenerateAllPosiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }
    public bool IsTeamTurnActive(TeamColor team)
    {
        return activeplayer.Team == team;
    }
    public void EndTurn()
    {
        GenerateAllPosiblePlayerMoves(activeplayer);
        GenerateAllPosiblePlayerMoves(GetOpponentToPlayer(activeplayer));
        ChangeActiveteam();

    }
    private void ChangeActiveteam()
    {
        activeplayer = activeplayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }


}

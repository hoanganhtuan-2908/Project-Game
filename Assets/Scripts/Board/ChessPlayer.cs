using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ChessPlayer
{
    public TeamColor Team { get; set; }
    public Board board { get; set; }

    public List<Piece> activePieces { get ; private set; }
    public ChessPlayer(TeamColor team, Board board)
    {
        this.Team = team;
        this.board = board;
        activePieces = new List<Piece>();
    }
    public void AddPice(Piece piece)
    {
        if(!activePieces.Contains(piece))
        {
            activePieces.Add(piece);
        }
    }
    public void RemovePiece(Piece piece)
    {
        if(activePieces.Contains(piece))
        {
            activePieces.Remove(piece);
        }
    }
    public void GenerateAllPossibleMoves()
    {
        foreach(var piece in activePieces)
        {
            if (board.HasPiece(piece))
                piece.SelectAvaliableSquares();

        }
    }
}

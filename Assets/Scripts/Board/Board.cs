using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;

    private void Awake()
    {
        CreateGrid();
    }
    public void SetDependencies(ChessGameController chessController)
    {
        this.chessController = chessController;
    }

    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public Vector3 CalcPositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }
    private Vector2Int CalculateCoordsfromposition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt((inputPosition.x / squareSize) + BOARD_SIZE / 2);
        int y = Mathf.FloorToInt((inputPosition.z / squareSize) + BOARD_SIZE / 2);
        return new Vector2Int(x, y);
    }
    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsfromposition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
                else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.Team))
                SelectedPiece(piece);
            else if (selectedPiece.CanMoveTo(coords))
                OnSelectedPieceMoved(coords, selectedPiece);
        }
        else
        {
            if ( piece != null && chessController.IsTeamTurnActive(piece.Team))
                SelectedPiece(piece);
        }
    }



    private void SelectedPiece(Piece piece)
    {
        selectedPiece = piece;
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
    }
    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece,null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();

    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    private void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newpiece,Piece oldpiece)
    {
        grid[newCoords.x, newCoords.y] = newpiece;
        grid[oldCoords.x, oldCoords.y] = oldpiece;
    }

    private Piece GetPieceOnSquare(Vector2Int coords)
    {
        if(CheckIfCoordinatedAreOnBoard(coords))
        
            return grid[coords.x, coords.y];
            return null;
    }

    private bool CheckIfCoordinatedAreOnBoard(Vector2Int coords)
    {
        if(coords.x < 0 || coords.x >= BOARD_SIZE || coords.y < 0 || coords.y >= BOARD_SIZE)
        
            return false;
            return true;
        
    }



    public bool HasPiece(Piece piece)
    {
       for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void SetPieceOnboard(Vector2Int coords, Piece piece)
    {
        if(CheckIfCoordinatedAreOnBoard(coords))
        {
            grid[coords.x, coords.y] = piece;
        }               
    }
}
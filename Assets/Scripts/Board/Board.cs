using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SquareSelectorCreator))]
public abstract class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;
    [SerializeField] private bool bottomLeftIsSquareCenter = true;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    protected virtual void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (squareSelector == null)
            squareSelector = GetComponent<SquareSelectorCreator>();
        if (grid == null)
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

    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    internal void OnSetSelectedPiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        selectedPiece = piece;
    }

    internal void OnSelectedPieceMoved(Vector2Int intCoords)
    {
        TryToTakeOppositePiece(intCoords);
        UpdateBoardOnPieceMove(intCoords, selectedPiece.occupiedSquare, selectedPiece, null);
        selectedPiece.MovePiece(intCoords);
        DeselectPiece();
        EndTurn();
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        // Use board anchor axes so mapping stays correct even when board is rotated.
        Vector3 fromBottomLeft = inputPosition - bottomLeftSquareTransform.position;
        float projectedX = Vector3.Dot(fromBottomLeft, bottomLeftSquareTransform.right);
        float projectedY = Vector3.Dot(fromBottomLeft, bottomLeftSquareTransform.forward);

        Debug.Log(string.Format("[Coords] inputPos {0}, bottomLeft {1}, fromBottomLeft {2}", 
            inputPosition, bottomLeftSquareTransform.position, fromBottomLeft));
        Debug.Log(string.Format("[Coords] projectedX {0}, projectedY {1}, squareSize {2}", 
            projectedX, projectedY, squareSize));

        int x = bottomLeftIsSquareCenter
            ? Mathf.RoundToInt(projectedX / squareSize)
            : Mathf.FloorToInt(projectedX / squareSize);
        int y = bottomLeftIsSquareCenter
            ? Mathf.RoundToInt(projectedY / squareSize)
            : Mathf.FloorToInt(projectedY / squareSize);
        
        Debug.Log(string.Format("[Coords] Final coords: ({0}, {1})", x, y));
        return new Vector2Int(x, y);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        if (chessController == null)
        {
            Debug.LogWarning("[Board] chessController is null. Did you call SetDependencies?");
            return;
        }
        if (!chessController.CanPerformMove())
        {
            Debug.Log("[Board] CanPerformMove returned false.");
            return;
        }

        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        Debug.Log(string.Format("[Board] Click at {0} -> coords {1}, piece {2}", inputPosition, coords, piece != null ? piece.name : "null"));
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(coords);
            else if (selectedPiece.CanMoveTo(coords))
                SelectedPieceMoved(coords);
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(coords);
        }
    }

    public abstract void SelectedPieceMoved(Vector2 coords);
    public abstract void SetSelectedPiece(Vector2 coords);



    private void SelectPiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        
        SetSelectedPiece(coords);
        List<Vector2Int> selection = selectedPiece.avaliableMoves;
        ShowSelectionSquares(selection);
    }


    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        EnsureInitialized();
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        EnsureInitialized();
        selectedPiece = null;
        squareSelector.ClearSelection();
    }



    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        EnsureInitialized();
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        EnsureInitialized();
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public bool HasPiece(Piece piece)
    {
        EnsureInitialized();
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


    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        EnsureInitialized();
        if (CheckIfCoordinatesAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece && !selectedPiece.IsFromSameTeam(piece))
        {
            TakePiece(piece);
        }
    }

    private void TakePiece(Piece piece)
    {
        if (piece == null)
            return;

        if (piece is King)
        {
            if (piece.team == TeamColor.Black)
            {
                Debug.Log("YOU WIN");

                if (WinUI.Instance != null)
                {
                    WinUI.Instance.ShowWin();
                }

                Time.timeScale = 0f;
            }
            else
            {
                Debug.Log("YOU LOSE");

                if (LossUI.Instance != null)
                    LossUI.Instance.ShowLoss();
            }
        }

        EnsureInitialized();
        grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
        chessController.OnPieceRemoved(piece);
        Destroy(piece.gameObject);
    }


    public void PromotePiece(Piece piece)
    {
        TakePiece(piece);
        chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen));
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }



}
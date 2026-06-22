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
    private bool captureOccurredThisTurn = false;
    private bool castleOccurredThisTurn = false;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    public ChessGameController ChessController => chessController;

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
        return bottomLeftSquareTransform.position
            + bottomLeftSquareTransform.right * coords.x * squareSize
            + bottomLeftSquareTransform.forward * coords.y * squareSize;
    }

    public Vector3 GetBoardCenter()
    {
        float centerOffset = (BOARD_SIZE - 1) * 0.5f * squareSize;
        return bottomLeftSquareTransform.position
            + bottomLeftSquareTransform.right * centerOffset
            + bottomLeftSquareTransform.forward * centerOffset;
    }

    internal void OnSetSelectedPiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        selectedPiece = piece;
    }

    internal void OnSelectedPieceMoved(Vector2Int intCoords)
    {
        captureOccurredThisTurn = false;
        castleOccurredThisTurn = false;
        Vector2Int fromSquare = selectedPiece.occupiedSquare;
        // Check if the move is a castling move before executing it
        if (selectedPiece is King king)
        {
            if (intCoords == king.leftCastlingMove || intCoords == king.rightCastlingMove)
            {
                castleOccurredThisTurn = true;
            }
        }
        // Check if the move is an en passant capture
        bool enPassantOccurredThisTurn = false;
        if (selectedPiece is Pawn && chessController != null && intCoords == chessController.enPassantTargetSquare)
        {
            enPassantOccurredThisTurn = true;
        }
        if (enPassantOccurredThisTurn)
        {
            // The captured pawn is on the same row as the capturing pawn, but same column as intCoords
            Vector2Int capturedPawnCoords = new Vector2Int(intCoords.x, selectedPiece.occupiedSquare.y);
            Piece capturedPawn = GetPieceOnSquare(capturedPawnCoords);
            if (capturedPawn != null)
            {
                TakePiece(capturedPawn);
                captureOccurredThisTurn = true;
            }
        }
        else
        {
            TryToTakeOppositePiece(intCoords);
        }
        UpdateBoardOnPieceMove(intCoords, selectedPiece.occupiedSquare, selectedPiece, null);
        // Record move state in game controller
        if (chessController != null)
        {
            chessController.RecordMove(selectedPiece, fromSquare, intCoords, captureOccurredThisTurn);
        }
        selectedPiece.MovePiece(intCoords);
        // Play FMOD audio based on turn action
        if (FMODAudioManager.Instance != null)
        {
            if (castleOccurredThisTurn)
            {
                FMODAudioManager.Instance.PlayCastle();
            }
            else if (captureOccurredThisTurn)
            {
                FMODAudioManager.Instance.PlayCapture();
            }
            else
            {
                if (chessController != null)
                {
                    if (chessController.IsActivePlayerLocal())
                    {
                        FMODAudioManager.Instance.PlaySelfMove();
                    }
                    else
                    {
                        FMODAudioManager.Instance.PlayOppMove();
                    }
                }
            }
        }
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
            else
            {
                // Played when the player tries to make an invalid move with a selected piece
                if (FMODAudioManager.Instance != null)
                {
                    FMODAudioManager.Instance.PlayIllegalMove();
                }
            }
        }
        else if(piece != null && chessController.IsTeamTurnActive(piece.team))
            SelectPiece(coords);
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
        captureOccurredThisTurn = true; // Set flag when capture happens

        if (piece is King)
        {
            TeamColor winnerTeam = piece.team == TeamColor.White ? TeamColor.Black : TeamColor.White;
            bool localPlayerWon = chessController != null && chessController.IsLocalPlayerWinner(winnerTeam.ToString());

            // Play FMOD win/lose theme immediately before timescale freeze
            if (FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.PlayGameFinishedTheme(localPlayerWon);
            }

            if (localPlayerWon)
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
                {
                    LossUI.Instance.ShowLoss();
                }

                Time.timeScale = 0f;
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
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayPromote();
        }
        chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen));
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }



}

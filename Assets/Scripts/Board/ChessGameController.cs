using System;
using UnityEngine;

[RequireComponent(typeof(PiecesCreator))]
public abstract class ChessGameController : MonoBehaviour
{
    protected const byte SET_GAME_STATE_EVENT_CODE = 1;

    [Header("Rules & FEN Tracking")]
    public Vector2Int enPassantTargetSquare = new Vector2Int(-1, -1);
    public int halfmoveClock = 0;
    public int fullmoveNumber = 1;

    [SerializeField] private BoardLayout startingBoardLayout;
    public abstract bool IsActivePlayerLocal();

    protected IChessUIManager UIManager;
    private CameraSetup cameraSetup;
    protected Board board;
    private PiecesCreator pieceCreator;
    protected ChessPlayer whitePlayer;
    protected ChessPlayer blackPlayer;
    protected ChessPlayer activePlayer;


    protected GameState state;

    private void Awake()
    {
        pieceCreator = GetComponent<PiecesCreator>();
    }

    internal void SetDependencies(CameraSetup cameraSetup, IChessUIManager UIManager, Board board)
    {
        this.cameraSetup = cameraSetup;
        this.UIManager = UIManager;
        this.board = board;
    }

    public void InitializeGame()
    {
        CreatePlayers();
    }


    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }


    public void StartNewGame()
    {
        enPassantTargetSquare = new Vector2Int(-1, -1);
        halfmoveClock = 0;
        fullmoveNumber = 1;

        UIManager.OnGameStarted();
        SetGameState(GameState.Init);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        TryToStartThisGame();

    }

    protected abstract void SetGameState(GameState state);
    public abstract void TryToStartThisGame();
    public abstract bool CanPerformMove();
    public abstract bool IsLocalPlayerWinner(String winnerTeam);

    internal bool IsGameInProgress()
    {
        return state == GameState.Play;
    }



    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }



    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        // Truyền thêm 'team' để PiecesCreator chọn đúng 1 trong 12 prefab của skin
        GameObject pieceObject = pieceCreator.CreatePiece(type, team);
        if (pieceObject == null)
        {
            Debug.LogError($"[ChessGameController] CreatePiece trả về null cho '{type.Name}' - '{team}'. Kiểm tra lại Active Skin trên PiecesCreator.");
            return;
        }

        Piece newPiece = pieceObject.GetComponent<Piece>();
        if (newPiece == null)
        {
            Debug.LogError($"[ChessGameController] Prefab '{pieceObject.name}' (loại: {type.Name}, đội: {team}) KHÔNG có component '{type.Name}' gắn vào! Hãy gắn script '{type.Name}.cs' vào root của prefab đó.");
            Destroy(pieceObject);
            return;
        }

        newPiece.SetData(squareCoords, team, board);

        // Nếu skin có Material ghi đè → áp dụng; nếu null → giữ màu sẵn của model
        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }



    internal void SetupCamera(TeamColor team)
    {
        if (cameraSetup == null)
        {
            Debug.LogError("cameraSetup is null in ChessGameController. Did you forget to assign CameraSetup in GameInitializer?");
            return;
        }

        cameraSetup.SetupCamera(team);
    }



    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        
        // If the moving player has put the opponent in check, play the checked SFX
        if (activePlayer.CheckIfIsAttackigPiece<King>())
        {
            if (FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.PlayChecked();
            }
        }
        ChangeActiveTeam();
    }

    private bool TryGetGameWinner(out ChessPlayer winner)
    {
        winner = null;
        ChessPlayer opponent = GetOpponentToPlayer(activePlayer);

        if (IsCheckmated(opponent, activePlayer))
        {
            winner = activePlayer;
            return true;
        }

        if (IsCheckmated(activePlayer, opponent))
        {
            winner = opponent;
            return true;
        }

        return false;
    }

    protected bool IsCheckmated(ChessPlayer defender, ChessPlayer attacker)
    {
        return attacker.CheckIfIsAttackigPiece<King>()
            && !defender.CanHidePieceFromAttack<King>(attacker);
    }

    private void EndGame(ChessPlayer winner)
    {
        Debug.Log("END GAME CALLED");

        SetGameState(GameState.Finished);

        UIManager.OnGameFinished(winner.team.ToString());
    }
    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    protected virtual void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }
    internal void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White)
            ? whitePlayer
            : blackPlayer;

        pieceOwner.RemovePiece(piece);

        if (piece is King)
        {
            Debug.Log("================================");
            Debug.Log("KING CAPTURED");
            Debug.Log($"{GetOpponentToPlayer(pieceOwner).team} WIN");
            Debug.Log("================================");

            EndGame(GetOpponentToPlayer(pieceOwner));
        }
    }



    public virtual void PauseGame()
    {
        if (state == GameState.Play)
        {
            SetGameState(GameState.Paused);
            UIManager.TogglePauseMenu(true);

            if (this is SingleplayerChessGameController)
            {
                Time.timeScale = 0f;
            }
        }
    }

    public virtual void ResumeGame()
    {
        if (state == GameState.Paused)
        {
            SetGameState(GameState.Play);
            UIManager.TogglePauseMenu(false);

            Time.timeScale = 1f;
        }
    }

    public bool IsSquareAttackedBy(Vector2Int square, TeamColor attackerTeam)
    {
        ChessPlayer attacker = (attackerTeam == TeamColor.White) ? whitePlayer : blackPlayer;
        int pawnDirection = (attackerTeam == TeamColor.White) ? 1 : -1;
        foreach (var piece in attacker.activePieces)
        {
            if (piece == null || !board.HasPiece(piece)) continue;
            if (piece is Pawn)
            {
                // Pawns attack diagonally forward
                int diffX = Mathf.Abs(square.x - piece.occupiedSquare.x);
                int diffY = square.y - piece.occupiedSquare.y;
                if (diffX == 1 && diffY == pawnDirection)
                {
                    return true;
                }
            }
            else
            {
                // Non-pawn pieces attack if the empty square is in their available moves
                if (piece.avaliableMoves.Contains(square))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void RecordMove(Piece piece, Vector2Int from, Vector2Int to, bool capture)
    {
        // 1. En Passant Target Square
        if (piece is Pawn && Mathf.Abs(to.y - from.y) == 2)
        {
            int directionY = (piece.team == TeamColor.White) ? 1 : -1;
            enPassantTargetSquare = new Vector2Int(from.x, from.y + directionY);
        }
        else
        {
            enPassantTargetSquare = new Vector2Int(-1, -1);
        }
        // 2. Halfmove Clock
        if (piece is Pawn || capture)
        {
            halfmoveClock = 0;
        }
        else
        {
            halfmoveClock++;
        }
        // 3. Fullmove Number (incremented after Black's move)
        if (activePlayer.team == TeamColor.Black)
        {
            fullmoveNumber++;
        }
    }
}

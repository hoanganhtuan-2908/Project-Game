
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(PiecesCreator))]
public abstract class ChessGameController : MonoBehaviour
{
    protected const byte SET_GAME_STATE_EVENT_CODE = 1;

    [SerializeField] private BoardLayout startingBoardLayout;


    protected ChessUIManager UIManager;
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

    internal void SetDependencies(CameraSetup cameraSetup, ChessUIManager UIManager, Board board)
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
        if (CheckIfGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    private bool CheckIfGameIsFinished()
    {
       
        return false;
    }

    private void EndGame()
    {
        SetGameState(GameState.Finished);
        UIManager.OnGameFinished(activePlayer.team.ToString());
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
            Debug.Log("YOU WIN");
            Debug.Log("================================");

            EndGame();
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == GameState.Play)
            {
                PauseGame();
            }
            else if (state == GameState.Paused)
            {
                ResumeGame();
            }
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
}
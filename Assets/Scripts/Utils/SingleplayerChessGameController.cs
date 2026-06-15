using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SingleplayerChessGameController : ChessGameController
{
    private TeamColor localPlayerTeam = TeamColor.White;

    public void SetLocalPlayerTeam(TeamColor team)
    {
        localPlayerTeam = team;
        Debug.Log($"[SingleplayerChessGameController] Local player team set to: {localPlayerTeam}");
    }

    protected override void SetGameState(GameState state)
    {
        this.state = state;
    }

    public override void TryToStartThisGame()
    {
        SetGameState(GameState.Play);

        // If the player is Black, the AI is White. Since White goes first, trigger AI move immediately.
        if (localPlayerTeam == TeamColor.Black)
        {
            StartCoroutine(MakeStockfishMoveCoroutine());
        }
    }

    public override bool CanPerformMove()
    {
        if (!IsGameInProgress())
            return false;
        
        // Prevent player moves during AI's turn
        if (activePlayer.team != localPlayerTeam)
            return false;

        return true;
    }

    protected override void ChangeActiveTeam()
    {
        base.ChangeActiveTeam();

        // Trigger AI move calculation when it is not the local player's turn
        if (activePlayer.team != localPlayerTeam && state == GameState.Play)
        {
            StartCoroutine(MakeStockfishMoveCoroutine());
        }
    }

    private IEnumerator MakeStockfishMoveCoroutine()
    {
        // 1. Generate FEN from current board layout
        string fen = GenerateFEN();
        Debug.Log($"[Stockfish AI] Generated FEN: {fen}");

        // 2. Get current difficulty level
        ChessLevel level = ChessLevel.Regular;
        if (UIManager != null)
        {
            level = UIManager.SelectedLevel;
        }

        // 3. Run Stockfish asynchronously in background thread
        bool isDone = false;
        string bestMoveString = "";

        Task.Run(() =>
        {
            bestMoveString = GetBestMoveFromStockfish(fen, level);
            isDone = true;
        });

        // 4. Yield control back to Unity until thread finishes (keeps UI active and smooth)
        while (!isDone)
        {
            yield return null;
        }

        // 5. Verify game state is still valid and it remains AI's turn
        if (state != GameState.Play || activePlayer.team == localPlayerTeam)
        {
            yield break;
        }

        // 6. Parse and execute the best move
        bool moveExecuted = false;
        if (!string.IsNullOrEmpty(bestMoveString))
        {
            string[] parts = bestMoveString.Split(' ');
            if (parts.Length >= 2 && parts[0] == "bestmove")
            {
                string move = parts[1];
                if (move.Length >= 4)
                {
                    int fromX = move[0] - 'a';
                    int fromY = move[1] - '1';
                    int toX = move[2] - 'a';
                    int toY = move[3] - '1';

                    Vector2Int fromCoords = new Vector2Int(fromX, fromY);
                    Vector2Int toCoords = new Vector2Int(toX, toY);

                    Piece piece = board.GetPieceOnSquare(fromCoords);
                    ChessPlayer aiPlayer = localPlayerTeam == TeamColor.White ? blackPlayer : whitePlayer;
                    if (piece != null && piece.team == aiPlayer.team)
                    {
                        piece.SelectAvaliableSquares();
                        if (piece.CanMoveTo(toCoords))
                        {
                            StartCoroutine(ExecuteAIMoveCoroutine(piece, toCoords));
                            moveExecuted = true;
                        }
                    }
                }
            }
        }

        // 7. Fallback to a random move if Stockfish failed or returned an invalid move
        if (!moveExecuted)
        {
            MakeFallbackMove();
        }
    }

    private IEnumerator ExecuteAIMoveCoroutine(Piece piece, Vector2Int targetCoords)
    {
        // Highlight the selected piece for 0.4s to simulate human thought
        board.SetSelectedPiece(new Vector2(piece.occupiedSquare.x, piece.occupiedSquare.y));
        yield return new WaitForSeconds(0.4f);

        // Confirm state remains valid before final move execution
        if (state == GameState.Play && activePlayer.team != localPlayerTeam)
        {
            board.SelectedPieceMoved(new Vector2(targetCoords.x, targetCoords.y));
        }
    }

    private void MakeFallbackMove()
    {
        Debug.LogWarning("[AI] Stockfish failed or returned invalid move. Executing fallback random move.");

        List<KeyValuePair<Piece, Vector2Int>> allMoves = new List<KeyValuePair<Piece, Vector2Int>>();
        ChessPlayer aiPlayer = localPlayerTeam == TeamColor.White ? blackPlayer : whitePlayer;
        foreach (var piece in aiPlayer.activePieces)
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvaliableSquares();
                foreach (var move in piece.avaliableMoves)
                {
                    allMoves.Add(new KeyValuePair<Piece, Vector2Int>(piece, move));
                }
            }
        }

        if (allMoves.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, allMoves.Count);
            var choice = allMoves[index];
            StartCoroutine(ExecuteAIMoveCoroutine(choice.Key, choice.Value));
        }
        else
        {
            Debug.LogError("[AI] No fallback moves available. Game has ended.");
        }
    }

    private string GetBestMoveFromStockfish(string fen, ChessLevel level)
    {
        string bestMove = "";
        try
        {
            string exePath = Path.Combine(Application.dataPath, "AIChessModel/stockfish/stockfish-windows-x86-64-avx2.exe");
            exePath = exePath.Replace('/', '\\');

            if (!File.Exists(exePath))
            {
                Debug.LogError($"[Stockfish] Executable not found at path: {exePath}");
                return "";
            }

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = startInfo })
            {
                process.Start();

                int skillLevel = 10;
                int movetime = 500;

                switch (level)
                {
                    case ChessLevel.Beginner:
                        skillLevel = 0;
                        movetime = 100;
                        break;
                    case ChessLevel.Regular:
                        skillLevel = 8;
                        movetime = 400;
                        break;
                    case ChessLevel.Pro:
                        skillLevel = 20;
                        movetime = 1000;
                        break;
                }

                process.StandardInput.WriteLine("uci");
                process.StandardInput.WriteLine("isready");

                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (line == "readyok")
                        break;
                }

                process.StandardInput.WriteLine($"setoption name Skill Level value {skillLevel}");
                process.StandardInput.WriteLine($"position fen {fen}");
                process.StandardInput.WriteLine($"go movetime {movetime}");
                process.StandardInput.Flush();

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (line.StartsWith("bestmove"))
                    {
                        bestMove = line;
                        break;
                    }
                }

                process.StandardInput.WriteLine("quit");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Stockfish] Exception while running process: {e.Message}");
        }

        return bestMove;
    }

    private string GenerateFEN()
    {
        System.Text.StringBuilder fen = new System.Text.StringBuilder();

        // 1. Piece placement
        for (int y = 7; y >= 0; y--)
        {
            int emptySquares = 0;
            for (int x = 0; x < 8; x++)
            {
                Piece piece = board.GetPieceOnSquare(new Vector2Int(x, y));
                if (piece == null)
                {
                    emptySquares++;
                }
                else
                {
                    if (emptySquares > 0)
                    {
                        fen.Append(emptySquares);
                        emptySquares = 0;
                    }
                    char pieceChar = GetPieceChar(piece);
                    fen.Append(pieceChar);
                }
            }
            if (emptySquares > 0)
            {
                fen.Append(emptySquares);
            }
            if (y > 0)
            {
                fen.Append('/');
            }
        }

        // 2. Active color
        fen.Append(activePlayer.team == TeamColor.White ? " w " : " b ");

        // 3. Castling rights
        string castling = "";
        Piece whiteKing = whitePlayer.GetPiecesOfType<King>().FirstOrDefault();
        if (whiteKing != null && !whiteKing.hasMoved)
        {
            Piece rightRook = board.GetPieceOnSquare(new Vector2Int(7, 0));
            if (rightRook != null && rightRook is Rook && !rightRook.hasMoved)
                castling += "K";
            Piece leftRook = board.GetPieceOnSquare(new Vector2Int(0, 0));
            if (leftRook != null && leftRook is Rook && !leftRook.hasMoved)
                castling += "Q";
        }
        Piece blackKing = blackPlayer.GetPiecesOfType<King>().FirstOrDefault();
        if (blackKing != null && !blackKing.hasMoved)
        {
            Piece rightRook = board.GetPieceOnSquare(new Vector2Int(7, 7));
            if (rightRook != null && rightRook is Rook && !rightRook.hasMoved)
                castling += "k";
            Piece leftRook = board.GetPieceOnSquare(new Vector2Int(0, 7));
            if (leftRook != null && leftRook is Rook && !leftRook.hasMoved)
                castling += "q";
        }
        if (string.IsNullOrEmpty(castling))
            castling = "-";
        fen.Append(castling);

        // 4. En passant target square (default to -)
        fen.Append(" - ");

        // 5. Halfmove clock and Fullmove number
        fen.Append("0 1");

        return fen.ToString();
    }

    private char GetPieceChar(Piece piece)
    {
        char c = ' ';
        if (piece is Pawn) c = 'p';
        else if (piece is Knight) c = 'n';
        else if (piece is Bishop) c = 'b';
        else if (piece is Rook) c = 'r';
        else if (piece is Queen) c = 'q';
        else if (piece is King) c = 'k';

        if (piece.team == TeamColor.White)
            c = char.ToUpper(c);

        return c;
    }
}
public interface IChessUIManager
{
    void OnGameStarted();
    void OnGameFinished(string winner);
    void TogglePauseMenu(bool isPaused);
}

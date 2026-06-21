using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerChessGameController : ChessGameController, IOnEventCallback
{

    private const byte SURRENDER_EVENT_CODE = 2;
    private NetworkManager networkManager;
    private ChessPlayer localPlayer;

    public void SetNetworkManager(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void SetLocalPlayer(TeamColor team)
    {
        localPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
    }

    public bool IsLocalPlayersTurn()
    {
        return localPlayer == activePlayer;
    }

    protected override void SetGameState(GameState state)
    {
        object[] content = new object[] { (int)state };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(SET_GAME_STATE_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void Surrender()
    {
        if (localPlayer == null || state != GameState.Play) return;

        // The opponent wins when local player surrenders
        TeamColor winnerTeam = localPlayer.team == TeamColor.White ? TeamColor.Black : TeamColor.White;
        object[] content = new object[] { (int)winnerTeam };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(SURRENDER_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SET_GAME_STATE_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameState state = (GameState)data[0];
            this.state = state;
        }
        else if (eventCode == SURRENDER_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            TeamColor winnerTeam = (TeamColor)data[0];
            this.state = GameState.Finished;
            UIManager.OnGameFinished(winnerTeam.ToString());
        }
    }

    public override void PauseGame()
    {
        // Disable pausing in multiplayer
    }

    public override void ResumeGame()
    {
        // Disable pausing in multiplayer
    }

    public override void TryToStartThisGame()
    {

        if (networkManager.IsRoomFull())
        {
            SetGameState(GameState.Play);
        }

    }


    public override bool CanPerformMove()
    {
        if (!IsGameInProgress() || !IsLocalPlayersTurn())
            return false;
        return true;
    }

    public override bool IsLocalPlayerWinner(string winnerTeam)
    {
        if (localPlayer != null)
        {
            return winnerTeam == localPlayer.team.ToString();
        }
        return false;
    }
}
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MultiplayerBoard : Board
{
    private PhotonView photonView;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Transform anchor = GameObject.Find("MultiplayerBoardAnchor")?.transform;

        if (anchor != null)
        {
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("MultiplayerBoardAnchor not found. MultiplayerBoard may be misaligned.");
        }
    }

    public override void SelectedPieceMoved(Vector2 coords)
    {
        Debug.Log("[Multiplayer] RPC move sent");
        // Apply locally immediately so selectedPiece is valid for the sender
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSelectedPieceMoved(intCoords);
        // Sync to other clients only (sender already applied above)
        photonView.RPC(nameof(RPC_OnSelectedPieceMoved), RpcTarget.Others, coords);
    }

    public override void SetSelectedPiece(Vector2 coords)
    {
        Debug.Log("[Multiplayer] RPC select sent");
        // Apply locally immediately so selectedPiece is valid for the sender
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSetSelectedPiece(intCoords);
        // Sync to other clients only (sender already applied above)
        photonView.RPC(nameof(RPC_SetSelectedPiece), RpcTarget.Others, coords);
    }

    [PunRPC]
    private void RPC_SetSelectedPiece(Vector2 coords)
    {
        Debug.Log("[Multiplayer] ON RPC select");
        Vector2Int intCoords = new Vector2Int(
            Mathf.RoundToInt(coords.x),
            Mathf.RoundToInt(coords.y)
        );
        OnSetSelectedPiece(intCoords);
    }

    [PunRPC]
    private void RPC_OnSelectedPieceMoved(Vector2 coords)
    {
        Debug.Log("[Multiplayer] ON RPC move");
        Vector2Int intCoords = new Vector2Int(
            Mathf.RoundToInt(coords.x),
            Mathf.RoundToInt(coords.y)
        );
        OnSelectedPieceMoved(intCoords);
    }
}
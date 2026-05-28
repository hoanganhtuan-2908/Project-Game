using UnityEngine;
using UnityEngine.InputSystem;

public class ColliderInputReciever : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit.point);

                Piece piece = hit.collider.GetComponent<Piece>();

                if (piece != null)
                {
                    Vector2Int targetPos =
                        piece.CurrentPosition + Vector2Int.up;

                    piece.MovePiece(targetPos);
                }
            }
        }
    }
}
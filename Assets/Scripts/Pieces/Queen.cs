using UnityEngine;

public class Queen : Piece
{
    void Start()
    {
        Debug.Log("Queen script is working!");
    }

    public bool CanMove(int targetX, int targetY)
    {
        int dx = Mathf.Abs(targetX - x);
        int dy = Mathf.Abs(targetY - y);

        if (x == targetX || y == targetY)
        {
            return true;
        }

        if (dx == dy)
        {
            return true;
        }

        return false;
    }
}
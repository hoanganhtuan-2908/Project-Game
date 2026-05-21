using UnityEngine;

public class King : Piece
{
    void Start()
    {
        Debug.Log("King script is working!");
    }

    public bool CanMove(int targetX, int targetY)
    {
        int dx = Mathf.Abs(targetX - x);
        int dy = Mathf.Abs(targetY - y);

        if (dx <= 1 && dy <= 1)
        {
            return true;
        }

        return false;
    }
}
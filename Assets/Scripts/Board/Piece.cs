using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IOjectTweener))]
[RequireComponent(typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    private MaterialSetter MaterialSetter;
    public Board Board { get; set; }
    public Vector2Int CurrentPosition { get; set; }
    public TeamColor Team { get; set; }
    public bool HasMoved { get;private set; }

    public List<Vector2Int> GetLegalMoves;

    private IOjectTweener Tweener;

    public abstract List<Vector2Int> GetLegalMovesForPiece();

    public void Awake()
    {
        GetLegalMoves = new List<Vector2Int>();
        Tweener = GetComponent<IOjectTweener>();
        MaterialSetter = GetComponent<MaterialSetter>();
        HasMoved = false;
    }

    public void SetMaterial(Material material)
    {
        if(MaterialSetter == null)
        {
            MaterialSetter = GetComponent<MaterialSetter>();
        }
        MaterialSetter.SetMaterial(material);
    }

    public bool IsFromSameTeam(Piece otherPiece)
    {
        return Team == otherPiece.Team;
    }

    public bool CanMoveTo(Vector2Int coords)
    {
        return GetLegalMovesForPiece().Contains(coords);
    }

    public virtual void MovePiece(Vector2Int coords)
    {
    }

    protected void TryToAddMove(Vector2Int coords)
    {
        GetLegalMoves.Add(coords);
    }

    internal void SetData(Vector2Int coords, TeamColor team, Board board)
    {
        this.Team = team;
        CurrentPosition = coords;
        this.Board = board;
        transform.position = Board.CalcPositionFromCoords(coords);
    }
}

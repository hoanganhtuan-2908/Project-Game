using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    private MaterialSetter MaterialSetter;
    public Board Board { get; set; }
    public Vector2Int CurrentPosition { get; set; }
    public TeamColor Team { get; set; }
    public bool HasMoved { get;private set; }
    public Vector2Int occupiedSquare { get; internal set; }

    public List<Vector2Int> GetLegalMoves;

    private IObjectTweener Tweener;
   

    public abstract List<Vector2Int> GetLegalMovesForPiece();

    public void Awake()
    {
        GetLegalMoves = new List<Vector2Int>();
        Tweener = GetComponent<IObjectTweener>();
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
        return GetLegalMoves.Contains(coords);
    }

    public virtual void MovePiece(Vector2Int coords)
    {

        Vector3 targetPosition = Board.CalcPositionFromCoords(coords);
        occupiedSquare = coords;
        HasMoved = true;
        Tweener.MoveTo(transform, targetPosition);
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

    public void SelectAvaliableSquares()
    {
        GetLegalMovesForPiece();
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[RequireComponent(typeof(Board))]
public class BoardInputHandler : MonoBehaviour, InputHandler
{
    private Board Board;
    private void Awake()
    {
        Board = GetComponent<Board>();
    }
    public void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action callback)
    {
        Board.OnSquareSelected(inputPosition);
    }
}

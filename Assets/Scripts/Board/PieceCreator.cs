using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceCreator : MonoBehaviour
{
    [SerializeField] private GameObject[] piecePrefabs;
    [SerializeField] private Material BlackMaterial;
    [SerializeField] private Material WhiteMaterial;


    public Dictionary<string, GameObject> NameToPieceDictionary = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var piece in piecePrefabs)
        {
            NameToPieceDictionary.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
        }
        
    }

    public GameObject CreatePieceOfType(Type pieceType)
    {
        GameObject prefab = NameToPieceDictionary[pieceType.ToString()];
        if(prefab)
        {
            GameObject newPiece = Instantiate(prefab);
            return newPiece;
        }
            return null;

    }

    public Material GetMaterialForTeam(TeamColor team)
    {
        return team == TeamColor.White ? WhiteMaterial : BlackMaterial;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile", order = 1)]
public class Tile : ScriptableObject
{
    public int id;

    public GameObject prefab;
    [Range(0, 3)] public int rotation;

    public Tile[] upNeighbours;
    public Tile[] rightNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;

    public Tile[] this[int key]
    {
        get
        {
            switch(key)
            {
                case 0:
                    return upNeighbours;
                case 1:
                    return rightNeighbours;
                case 2:
                    return downNeighbours;
                case 3:
                    return leftNeighbours;
                default:
                    return upNeighbours;
            }
        }

        set
        {
            switch (key)
            {
                case 0:
                    upNeighbours = value;
                    break;
                case 1:
                    rightNeighbours = value;
                    break;
                case 2:
                    downNeighbours = value;
                    break;
                case 3:
                    leftNeighbours = value;
                    break;
                default:
                    upNeighbours = value;
                    break;
            }
        }
    }
}

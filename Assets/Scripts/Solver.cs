using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public Tile[] baseTilesSOS;
    //Note (Tautvydas): We're not adding to the default Tile SO's, only to new SO's that are tied to each scene
    private Dictionary<(int, int), Tile> baseTiles = new Dictionary<(int, int), Tile>();
    public int width;
    public Dictionary<(int, int), TileObject> positionedTiles = new Dictionary<(int, int), TileObject>();
    [SerializeField] private Transform mapGO;
    [SerializeField] private int mapX = 100;
    [SerializeField] private int mapY = 100;
    [SerializeField] private Tile[,] tilemap;
    [SerializeField] private int spacing = 5;
    [SerializeField] private bool clearBaseTiles = false;
    private TileObject[] allTileObjs;

    private void Awake()
    {
        foreach(var baseTile in baseTilesSOS)
        {
            if (baseTiles.ContainsKey((baseTile.id, baseTile.rotation)))
                Debug.Log(baseTile.name);

            baseTiles.Add((baseTile.id, baseTile.rotation), baseTile);
            if(clearBaseTiles)
            {
                baseTile.upNeighbours = new Tile[0];
                baseTile.rightNeighbours = new Tile[0];
                baseTile.downNeighbours = new Tile[0];
                baseTile.leftNeighbours = new Tile[0];
            }
        }

        allTileObjs = GetComponentsInChildren<TileObject>(true);
    }

    void Start()
    {
        SolveVariants();

        AssetDatabase.Refresh();
        foreach(Tile tile in baseTiles.Values)
        {
            EditorUtility.SetDirty(tile);
        }
        AssetDatabase.SaveAssets();
    }

    public void SolveVariants()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var tileObject = transform.GetChild(i).GetComponent<TileObject>();
            int x = Mathf.RoundToInt(tileObject.transform.position.x / spacing);
            int y = Mathf.RoundToInt(-tileObject.transform.position.z / spacing);

            if (positionedTiles.ContainsKey((x, y)))
                Debug.Log("" + x + " " + y + tileObject.name);

            positionedTiles.Add((x, y), tileObject);
        }

        var height = positionedTiles.Count / width;

        foreach(TileObject current in allTileObjs)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                int x = Mathf.RoundToInt(current.transform.position.x / spacing);
                int y = Mathf.RoundToInt(-current.transform.position.z / spacing);

                int rotation = Mathf.RoundToInt((current.transform.localEulerAngles.y + 360) % 360 / 90); //Note (Tautvydas): do not use rotation of SO, only base SO's with rotation 0 are assigned
                Tile baseTile = baseTiles[(current.tileSO.id, rotation % 4)];
                int dY = 0, dX = 0;

                switch (dir)
                {
                    case 0:
                        dY = -1;
                        break;
                    case 1:
                        dX = 1;
                        break;
                    case 2:
                        dY = 1;
                        break;
                    case 3:
                        dX = -1;
                        break;
                }

                if (positionedTiles.ContainsKey((x + dX, y + dY)))
                {
                    TileObject adjacent = positionedTiles[(x + dX, y + dY)];
                    int adjRotation = Mathf.RoundToInt((adjacent.transform.localEulerAngles.y + 360) % 360 / 90); //Note (Tautvydas): do not use rotation of SO, only base SO's with rotation 0 are assigned

                    int index = Contains(baseTile[dir], adjacent.tileSO.id, adjRotation % 4);
                    if (index == -1)
                    {
                        Tile baseAdjacent = baseTiles[(adjacent.tileSO.id, adjRotation % 4)];

                        AddToNeighbours(baseTile, dir, baseAdjacent);
                    }
                }
            }
        }
    }

    private void AddToNeighbours(Tile baseTile, int dir, Tile toAdd)
    {
        Tile[] tiles = new Tile[baseTile[dir].Length + 1];
        
        for(int i = 0; i < tiles.Length - 1; i++)
        {
            tiles[i] = baseTile[dir][i];
        }

        tiles[tiles.Length - 1] = toAdd;
        baseTile[dir] = tiles;
    }

    public int Contains(Tile[] tiles, int id, int rotation)
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            var tile = tiles[i];
            if (tile.id == id && tile.rotation == rotation)
                return i;
        }
        return -1;
    }

    public TileObject GetTile(TileObject[] tiles, int width, int x, int y)
    {
        return tiles[y * width + x];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public Tile[] baseTilesSOS;
    //Note (Tautvydas): We're not adding to the default Tile SO's, only to new SO's that are tied to each scene
    private Dictionary<(int, int), Tile> baseTiles = new Dictionary<(int, int), Tile>();
    public Dictionary<(int, int), TileObject> positionedTiles = new Dictionary<(int, int), TileObject>();
    [SerializeField] private Transform mapGO;
    [SerializeField] private int mapX = 100;
    [SerializeField] private int mapY = 100;
    [SerializeField] private int spacing = 10;
    [SerializeField] private bool clearBaseTiles = false;
    [SerializeField] private TileObject defaultTile;
    private TileObject[] allTileObjs;

    [SerializeField] private TileObject[] previewTiles;
    private int previewIndex = 0;
    private TileObject previewCurrent;

    private bool active = false;

    private TileObject current;

    private void Awake()
    {
        SolverAwake();
    }
    
    private void SolverAwake()
    {
        foreach (var baseTile in baseTilesSOS)
        {
            if (baseTiles.ContainsKey((baseTile.id, baseTile.rotation)))
                Debug.Log(baseTile.name);

            baseTiles.Add((baseTile.id, baseTile.rotation), baseTile);
            if (clearBaseTiles)
            {
                baseTile.upNeighbours = new Tile[0];
                baseTile.rightNeighbours = new Tile[0];
                baseTile.downNeighbours = new Tile[0];
                baseTile.leftNeighbours = new Tile[0];
            }
        }

        if (previewTiles.Length > 0)
        {
            previewTiles[0].gameObject.SetActive(true);
            for (int i = 1; i < previewTiles.Length; i++)
            {
                previewTiles[i].gameObject.SetActive(false);
            }
            previewIndex = 0;
            previewCurrent = previewTiles[previewIndex];
        }

        allTileObjs = GetComponentsInChildren<TileObject>(true);

        for (int i = 0; i < transform.childCount; i++)
        {
            var tileObject = transform.GetChild(i).GetComponent<TileObject>();

            if (tileObject.tileSO.id == -1)
                continue;

            int x = Mathf.RoundToInt(tileObject.transform.position.x / spacing);
            int y = Mathf.RoundToInt(tileObject.transform.position.z / spacing);

            if (positionedTiles.ContainsKey((x, y)))
                Debug.Log("" + x + " " + y + tileObject.name);

            positionedTiles.Add((x, y), tileObject);
        }

        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                if (!positionedTiles.ContainsKey((x, y)))
                {
                    Instantiate(defaultTile, new Vector3(x * spacing, 0, y * spacing), Quaternion.identity);
                }
            }
        }
    }

    private void Update()
    {
        current = GetTileObjectUnderCursor();

        // Place
        if (current != null && Input.GetMouseButtonDown(0))
        {
            (int, int) position = GetTileObjectPosition(current);
            GameObject newTile = Instantiate(baseTiles[(previewCurrent.tileSO.id, 0)].prefab, new Vector3(position.Item1 * spacing, 0, position.Item2 * spacing), Quaternion.identity);
            newTile.transform.parent = transform;
            positionedTiles[position] = newTile.GetComponent<TileObject>();
            Destroy(current.gameObject);
            current = null;
        }

        // Rotate
        if (current != null && Input.GetMouseButtonDown(1))
        {
            current.transform.localEulerAngles += new Vector3(0, 90, 0);
        }

        // Remove
        if (current != null && Input.GetKeyDown(KeyCode.X) && current.tileSO.id != -1)
        {
            (int, int) position = GetTileObjectPosition(current);
            Instantiate(defaultTile, new Vector3(position.Item1 * spacing, 0, position.Item2 * spacing), Quaternion.identity);
            positionedTiles.Remove(position);
            Destroy(current.gameObject);
            current = null;
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            previewIndex = (previewIndex + 1) % previewTiles.Length;
            previewCurrent.gameObject.SetActive(false);
            previewCurrent = previewTiles[previewIndex];
            previewCurrent.gameObject.SetActive(true);
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            previewIndex = (((previewIndex - 1) % previewTiles.Length) + previewTiles.Length) % previewTiles.Length;
            previewCurrent.gameObject.SetActive(false);
            previewCurrent = previewTiles[previewIndex];
            previewCurrent.gameObject.SetActive(true);
        }

        if (!active && Input.GetKeyDown(KeyCode.Space))
        {
            active = true;
            SolveVariants();

            AssetDatabase.Refresh();
            foreach (Tile tile in baseTiles.Values)
            {
                EditorUtility.SetDirty(tile);
            }
            AssetDatabase.SaveAssets();
            active = false;
        }
    }

    private (int, int) GetTileObjectPosition(TileObject tileObject)
    {
        int x = Mathf.RoundToInt(tileObject.transform.position.x / spacing);
        int y = Mathf.RoundToInt(tileObject.transform.position.z / spacing);

        return (x, y);
    }

    public void SolveVariants()
    {
        foreach (var baseTile in baseTilesSOS)
        {
            if (clearBaseTiles)
            {
                baseTile.upNeighbours = new Tile[0];
                baseTile.rightNeighbours = new Tile[0];
                baseTile.downNeighbours = new Tile[0];
                baseTile.leftNeighbours = new Tile[0];
            }
        }

        allTileObjs = GetComponentsInChildren<TileObject>(true);

        foreach (TileObject current in allTileObjs)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                int x = Mathf.RoundToInt(current.transform.position.x / spacing);
                int y = Mathf.RoundToInt(current.transform.position.z / spacing);

                int rotation = Mathf.RoundToInt((current.transform.localEulerAngles.y + 360) % 360 / 90); //Note (Tautvydas): do not use rotation of SO, only base SO's with rotation 0 are assigned
                Tile baseTile = baseTiles[(current.tileSO.id, rotation % 4)];
                int dY = 0, dX = 0;

                switch (dir)
                {
                    case 0:
                        dY = 1;
                        break;
                    case 1:
                        dX = 1;
                        break;
                    case 2:
                        dY = -1;
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

    TileObject GetTileObjectUnderCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var unit = hitData.collider.GetComponentInParent<TileObject>();
            return unit;
        }

        return null;
    }
}

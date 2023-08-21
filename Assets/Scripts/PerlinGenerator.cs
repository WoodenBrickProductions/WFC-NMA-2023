using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinGenerator : MonoBehaviour
{
    public int dimensions;                  // Height and width dimensions of generated tilemap
    public Tile[] generationTiles;          // Tiles used for generation
    public List<Cell> tilemapComponents;    // Every cell of the map
    public Cell cellObj;                    // Default Cell object for creation
    public float spacing = 10;              // Distance between tilemap cells
    public bool skipWait = false;

    public float xMapScaling = 100;
    public float yMapScaling = 100;

    public float xPlacementScaling = 100;
    public float yPlacementScaling = 100;


    void Awake()
    {
        tilemapComponents = new List<Cell>();

        InitializeCells();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skipWait = true;
        }
    }

    // Initialization of tilemap cells
    void InitializeCells()
    {
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector3(x * spacing, 0, y * spacing), Quaternion.identity);
                newCell.CreateCell(false, generationTiles);
                tilemapComponents.Add(newCell);
            }
        }

        StartCoroutine(perlinGenerate());
    }

    WaitForSeconds wait = new WaitForSeconds(0.01f);

    IEnumerator perlinGenerate()
    {
        for (int i = 0; i < tilemapComponents.Count; i++)
        {
            Cell cell = tilemapComponents[i];
            float x = cell.transform.position.x / spacing;
            float y = cell.transform.position.z / spacing;
            
            float baseMap = Mathf.PerlinNoise(1 / xMapScaling, 1 / yMapScaling);
            float basePlacement = Mathf.PerlinNoise(1 / xPlacementScaling, 1 / yPlacementScaling);

            int tile = (int) ((((Mathf.PerlinNoise(x / spacing / xPlacementScaling, y / spacing / yPlacementScaling) - basePlacement) * 1000) % generationTiles.Length) + generationTiles.Length) % generationTiles.Length;
            float height = (Mathf.PerlinNoise(x / spacing / xMapScaling, y / spacing / yMapScaling) - baseMap) * 1000;

            Instantiate(generationTiles[tile].prefab, cell.transform.position + Vector3.up * height, Quaternion.Euler(new Vector3(0, 0, 0)));
            
            if(!skipWait)
            {
                yield return wait;
            }
        }

        yield return 0;
    }

}

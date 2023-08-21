using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGenerator : MonoBehaviour
{
    public int dimensions;                  // Height and width dimensions of generated tilemap
    public Tile[] generationTiles;          // Tiles used for generation
    public List<Cell> tilemapComponents;    // Every cell of the map
    public Cell cellObj;                    // Default Cell object for creation
    public float spacing = 10;              // Distance between tilemap cells
    public float maxHeight = 5;
    public bool skipWait = false;

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

        StartCoroutine(RandomGenerate());
    }

    WaitForSeconds wait = new WaitForSeconds(0.001f);

    IEnumerator RandomGenerate()
    {
        for (int i = 0; i < tilemapComponents.Count; i++)
        {
            Cell cell = tilemapComponents[i];

            int tile = UnityEngine.Random.Range(0, generationTiles.Length);
            float height = UnityEngine.Random.Range(0.0f, maxHeight);

            Instantiate(generationTiles[tile].prefab, cell.transform.position + Vector3.up * height, Quaternion.Euler(new Vector3(0, 0, 0)));
            
            if(!skipWait)
            {
                yield return wait;
            }
        }

        yield return 0;
    }
}

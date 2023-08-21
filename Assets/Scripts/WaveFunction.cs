using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunction : MonoBehaviour
{
    public int dimensions;                  // Height and width dimensions of generated tilemap
    public Tile[] generationTiles;          // Tiles used for generation
    public List<Cell> tilemapComponents;    // Every cell of the map
    public Cell cellObj;                    // Default Cell object for creation
    public float spacing = 10;              // Distance between tilemap cells
    int iterations = 0;                     // Iteration of tilemap generation
    public Tile fallbackTile;               // Tile used for in case there are no tile options for a given Cell

    // Initialization of Scene
    void Awake()
    {
        tilemapComponents = new List<Cell>();

        SanitizeGenerationTiles(generationTiles);

        InitializeCells();
    }

    // Take out tiles from generationTiles that do not have any rules for placement
    private void SanitizeGenerationTiles(Tile[] generationTiles)
    {
        int count = 0;
        for (int i = 0; i < generationTiles.Length; i++)
        {
            Tile tile = generationTiles[i];
            if (
                tile.upNeighbours.Length != 0 ||
                tile.rightNeighbours.Length != 0 ||
                tile.downNeighbours.Length != 0 ||
                tile.leftNeighbours.Length != 0)
            {
                // We found a tile that has rules for placement
                count++;
            }
        }

        if (count == generationTiles.Length)
        {
            // All generationTIles have placement rules, we don't need to make a new array
            return;
        }

        // Not all tiles are generation safe, so we're going to make a new array
        Tile[] newTiles = new Tile[count];

        count = 0;
        for (int i = 0; i < generationTiles.Length; i++)
        {
            Tile tile = generationTiles[i];
            if (
                tile.upNeighbours.Length != 0 ||
                tile.rightNeighbours.Length != 0 ||
                tile.downNeighbours.Length != 0 ||
                tile.leftNeighbours.Length != 0)
            {
                // This tile is safe to use for generation
                newTiles[count++] = tile;
            }
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

        StartCoroutine(CheckEntropy());
    }

///////////////////////////////// ------- START HERE --------- //////////////////////////////////////////

    IEnumerator CheckEntropy()
    {
        List<Cell> tempTilemap = new List<Cell>(tilemapComponents);

        RemoveAllCollapsed(tempTilemap);
        SortTilemapByLength(tempTilemap);

        int arrayLength = tempTilemap[0].tileOptions.Length;
        int stopIndex = -1;

        for (int i = 1; i < tempTilemap.Count; i++)
        {
            if (tempTilemap[i].tileOptions.Length > arrayLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            // Remove cells that have a higher entropy than the lowest found
            KeepCountOfCells(tempTilemap, stopIndex);
        }

        // Used to create delay in visualization
        yield return new WaitForSeconds(0.01f);

        CollapseCell(tempTilemap);
    }

    // Keep only the first "count" amount of cells from the tempTilemap
    private void KeepCountOfCells(List<Cell> tempTilemap, int count)
    {
        Cell[] newTilemap = new Cell[count];
        for(int i = 0; i < count; i++)
        {
            newTilemap[i] = tempTilemap[i];
        }

        tempTilemap.Clear();
        tempTilemap.AddRange(newTilemap);

    }

    private void SortTilemapByLength(List<Cell> tempTilemap)
    {
        // For speed and saving time, I'm using the built in sorting function to sort them from the least tileOptions in the Cell to the most
        tempTilemap.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });
    }

    private void RemoveAllCollapsed(List<Cell> tempTilemap)
    {
        //tempTilemap.RemoveAll(c => c.collapsed); 
        for(int i = tempTilemap.Count - 1; i > -1; i--)
        {
            Cell cell = tempTilemap[i];
            if (cell.collapsed)
            {
                tempTilemap.RemoveAt(i);
            }
        }
    }

    void CollapseCell(List<Cell> tempTilemap)
    {
        if(tempTilemap.Count > 0 && tempTilemap[0].tileOptions.Length <= 1)
        {
            // All these cells only have one options what they can be, so we're going to place all of them at once
            
            for(int i = 0; i < tempTilemap.Count; i++)
            {
                Cell cellToCollapse = tempTilemap[i];
                CollapseSingularCell(cellToCollapse);
            }
        }
        else
        {
            // Picking a random Cell from the tempTilemap
            int randIndex = UnityEngine.Random.Range(0, tempTilemap.Count);
            Cell cellToCollapse = tempTilemap[randIndex];

            CollapseSingularCell(cellToCollapse);
        }

        UpdateGeneration();
    }

    // Update neighboring tiles to align with the rules of neighboring tiles
    // TODO: Clean this up
    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(tilemapComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                if (tilemapComponents[index].collapsed)
                {
                    //Debug.Log("called");
                    newGenerationCell[index] = tilemapComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in generationTiles)
                    {
                        options.Add(t);
                    }

                    //update above
                    if (y > 0)
                    {
                        Cell up = tilemapComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(generationTiles, obj => obj == possibleOptions);
                            var valid = generationTiles[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < dimensions - 1)
                    {
                        Cell right = tilemapComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(generationTiles, obj => obj == possibleOptions);
                            var valid = generationTiles[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1)
                    {
                        Cell down = tilemapComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(generationTiles, obj => obj == possibleOptions);
                            var valid = generationTiles[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        Cell left = tilemapComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(generationTiles, obj => obj == possibleOptions);
                            var valid = generationTiles[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        tilemapComponents = newGenerationCell;

        if (iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }

    }

    void CollapseSingularCell(Cell cellToCollapse)
    {
        // This cell is now collapsed and we'll remove it from the list when doing generation
        cellToCollapse.collapsed = true;

        Tile selectedTile;

        if (cellToCollapse.tileOptions.Length > 0)
        {
            // Choosing a random tile from the tileOptions
            int randomIndex = UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length);
            selectedTile = cellToCollapse.tileOptions[randomIndex];
        }
        else
        {
            // If this cell has no options of what can be placed, we'll place the fallbackTile
            selectedTile = fallbackTile;
        }

        // TODO: is this needed?
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        
        // Asking Unity to place the tile on the tilemap with the specified rotation;
        Instantiate(foundTile.prefab, cellToCollapse.transform.position, Quaternion.Euler(new Vector3(0, foundTile.rotation * 90, 0)));
        
        // Since this cell was collapsed we're increasing the iteration count, so we know when to stop the generation
        iterations++;
    }

    // Remove invalid tiles from the optionList that are not found in validOptions
    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var tile = optionList[x];
            // Check if the tile is not inside the list of valid options
            if (!validOption.Contains(tile))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}
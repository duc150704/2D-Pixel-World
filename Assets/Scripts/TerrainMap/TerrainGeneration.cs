using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public PlayerController player;
    public CamController camera1;
    public GameObject tileDrop;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Genneration Settings")]
    public int chunkSize = 16;
    public int worldSize = 100;
    public float heightAddition = 25;
    public bool generateCaves = true;

    //[Header("Ore Settings")]
    public OreClass[] ores;

    [Header("Noise Settings")]
    public float terrainFreq = 0.5f;
    public float caveFreq = 0.05f;
    public Texture2D caveNoiseTexture;



    private GameObject[] worldChuncks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private List<GameObject> worldTileObjects = new List<GameObject>();
    private List<TileClass> worldTileClasses = new List<TileClass>();

    private BiomeClass curBiome;
    private Color[] biomeCols;

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        for(int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].bioCol;
        }

        //DrawTextures();
        DrawBiomeMap();
        DrawCavesAndOres();

        CreateChunks();
        GenerateTerrain();

        camera1.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, camera1.transform.position.z));
        camera1.worldSize = worldSize;
        player.Spawn();

        RefreshChunks();
    }

    private void Update()
    {
        RefreshChunks();
    }

    void RefreshChunks()
    {
        for(int i = 0; i < worldChuncks.Length; i++)
        {
            if(Vector2.Distance(new Vector2(i * chunkSize, 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 4f)
                worldChuncks[i].SetActive(false);
            else
                worldChuncks[i].SetActive(true);
        }
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public void DrawCavesAndOres()
    {
        float o;
        float v;
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        for(int x = 0; x < caveNoiseTexture.width; x++)
        {
            for(int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else
                    caveNoiseTexture.SetPixel(x, y, Color.black);

                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);

                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        }

        caveNoiseTexture.Apply();
    }

    public void DrawTextures()
    {
        for (int i = 0; i < biomes.Length; i++)
        {

            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for(int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTextures(biomes[i].ores[o].frequency, ores[o].size, ores[o].spreadTexture);
            }
        }
    }
    public void GenerateNoiseTextures(float frequency, float limit ,Texture2D noiseTexture)
    {
        float v;
        for(int x = 0; x < noiseTexture.width; x++)
        {
            for(int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
        noiseTexture.Apply();
        biomeMap.Apply();
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChuncks = new GameObject[numChunks];
        for(int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChuncks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {

        if(System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y)) > 0)
        {
            return biomes[System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y))];
        }

        return curBiome;
    }
    public void GenerateTerrain()
    {
        TileClass tileClass;
        for(int x = 0; x < worldSize; x++)
        {
            //curBiome = GetCurrentBiome(x, 0);
            float height;

            for(int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;
                if (x == worldSize / 2)
                    player.spawnPos = new Vector2(x, height + 2); // Spawn nguoi choi
                if (y >= height)
                    break;
                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileClass = curBiome.tileAtlas.stone;
                    
                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawHeight)
                        tileClass = tileAtlas.coal;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawHeight)
                        tileClass = tileAtlas.iron;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawHeight)
                        tileClass = tileAtlas.gold;
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawHeight)
                        tileClass = tileAtlas.diamond;
                }
                else if(y < height - 1)
                {
                    tileClass = curBiome.tileAtlas.dirt;
                }
                else
                {
                    //layer tren cung
                    tileClass = curBiome.tileAtlas.grass;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileClass, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileClass, x, y);
                }
                if(y > height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);
                    if (t == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            if (curBiome.biomeName == "Desert")
                            {//Tao xuong rong
                                GenerateCactus(curBiome.tileAtlas, Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                            }
                            else
                            {//Tao cay
                                GenerateTree(Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                            }
                                        
                        }        
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.tallGrassChance);
                        //Tao co
                        if (i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if(curBiome.tileAtlas.tallGrass != null)
                                    PlaceTile(curBiome.tileAtlas.tallGrass, x, y + 1);
                            }
                        }
                    }
                }
            }
        }
    }

    void GenerateCactus(TileAtlas atlas ,int treeHeight, int x, int y)
    {

        //Than cay
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(atlas.log, x, y + i);
        }
    }
    void GenerateTree(int treeHeight,int x, int y)
    {
        
        //Than cay
        for(int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log, x, y + i);
        }
        //La cay
        PlaceTile(tileAtlas.leaf, x, y + treeHeight);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight + 1);
    }

    public void RemoveTile(int x, int y)
    {
        if (worldTiles.Contains(new Vector2Int(x, y)) && x <= worldSize && x >= 0 && y >= 0 && y <= worldSize)
        {
            Destroy(worldTileObjects[worldTiles.IndexOf(new Vector2(x, y))]);

            //roi vat pham khi dao
            if (worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].tileDrop)
            {
                GameObject newTileDrop = Instantiate(tileDrop, new Vector2(x, y + 0.5f), Quaternion.identity);
                newTileDrop.GetComponent<SpriteRenderer>().sprite = worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].tileSprites[0];
            }
            worldTileObjects.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
            worldTileClasses.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
            worldTiles.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
        }
    }

    public void CheckTile(TileClass tile, int x, int y, bool backgroundElement)
    {
        if (x <= worldSize && x >= 0 && y >= 0 && y <= worldSize)
        {
            if (!worldTiles.Contains(new Vector2Int(x, y)))
            {
                //place the tile regardless
                PlaceTile(tile, x, y);
            }
            else
            {
                if (worldTileClasses[worldTiles.IndexOf(new Vector2Int(x, y))].inBackground)
                {
                    //overwrite existing tile
                    RemoveTile(x, y);
                    PlaceTile(tile, x, y);
                }
            }
        }
    }
    public void PlaceTile(TileClass tile, int x, int y)
    {
        bool backgroundElement = tile.inBackground;

        if (x <= worldSize && x >= 0 && y >= 0 && y <= worldSize)
        {
            GameObject newTile = new GameObject();

            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;
            newTile.transform.parent = worldChuncks[chunkCoord].transform;

            newTile.AddComponent<SpriteRenderer>();
            if (!backgroundElement)
            {
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }

            int spriteIndex = Random.Range(0, tile.tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite =  tile.tileSprites[spriteIndex];
            if(tile.inBackground)
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
            else
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;

            newTile.name = tile.tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
            worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
            worldTileObjects.Add(newTile);
            worldTileClasses.Add(tile);
        }
    }
}

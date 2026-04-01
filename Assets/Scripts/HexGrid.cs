using System;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public static HexGrid instance;
    public GameObject hexPrefab;

    public int height = 11;
    public int width = 11;

    public float hexSize = 1f;

    public HexTile[,] tiles;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new HexTile[width, height];
        
        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                float x = hexSize * (q + r * 0.5f);
                float y = hexSize * (r * 0.866f);

                GameObject tile = Instantiate(hexPrefab,
                    new Vector3(x, y, 0),
                    Quaternion.identity,
                    transform);

                HexTile hex = tile.GetComponent<HexTile>();

                hex.q = q;
                hex.r = r;

                tiles[q, r] = hex;
            }
        }
    }

    public HexTile getHexTile(int q, int r)
    {
        if (q < 0 || q >= width || r < 0 || r >= height)
        {
            return null;
        }
        
        return tiles[q, r];
    }


}

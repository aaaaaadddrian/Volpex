using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public GameObject hexPrefab;

    public int height = 11;
    public int width = 11;

    public float hexSize = 1f;
    
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
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
            }
        }
    }


}

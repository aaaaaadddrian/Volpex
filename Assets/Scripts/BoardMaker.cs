using System.Collections.Generic;
using UnityEngine;

public class BoardMaker : MonoBehaviour{
    public GameObject Hex;
    public float offset = 0.5f;
    public float hexDist = 0.25f;
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        SetupBoard();
    }

    // Update is called once per frame
    void Update(){
        
    }

    private void SetupBoard()
    {
        var rows = 7;
        var cols = 7;
        float x = offset;
        float y = 0;
        
        var hexes = new Dictionary<Vector2Int, GameObject>(rows * cols);
        for (var row = 1; row < rows; row++)
        {
            
            for (var col = 1; col < cols; col++)
            {
                var position = new Vector2Int(row, col);
                var hex = Instantiate(Hex, new Vector3(x, y, 0), Quaternion.identity, transform);
                hexes[position] = hex;
                x += hexDist;
                y += offset;
            }

            x = offset + row * hexDist;
            y = offset + row * hexDist;
        }

       
    }
}

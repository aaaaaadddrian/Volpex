using System.Collections.Generic;
using UnityEngine;

public class WinChecker : MonoBehaviour
{

    public static WinChecker instance;

    void Awake()
    {
        instance = this;
    }

    public bool checkWin(int player)
    {
        if (player == 1)
        {
            return checkRedWin();
        }

        if (player == 0)
        {
            return checkBlueWin();
        }

        return false;
    }

    bool checkRedWin()
    {
        HashSet<HexTile> visited = new HashSet<HexTile>();
        Stack<HexTile> stack = new Stack<HexTile>();

        for (int r = 0; r < HexGrid.instance.height; r++)
        {
            HexTile tile = HexGrid.instance.getHexTile(0, r);

            if (tile != null && tile.owner == 1)
            {
                stack.Push(tile);
                visited.Add(tile);
            }
        }

        while (stack.Count > 0)
        {
            HexTile current = stack.Pop();

            if (current.q == HexGrid.instance.width - 1)
            {
                return true;
            }

            foreach (HexTile neighbor in getNeighbors(current))
            {
                if (neighbor != null && neighbor.owner == 1 && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }

        return false;
    }

    bool checkBlueWin()
    {
        HashSet<HexTile> visited = new HashSet<HexTile>();
        Stack<HexTile> stack = new Stack<HexTile>();

        for (int q = 0; q < HexGrid.instance.width; q++)
        {
            HexTile tile = HexGrid.instance.getHexTile(q, 0);

            if (tile != null && tile.owner == 0)
            {
                stack.Push(tile);
                visited.Add(tile);
            }
        }

        while (stack.Count > 0)
        {
            HexTile current = stack.Pop();

            if (current.r == HexGrid.instance.height - 1)
            {
                return true;
            }

            foreach (HexTile neighbor in getNeighbors(current))
            {
                if (neighbor != null && neighbor.owner == 0 && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }

        return false;
    }

    List<HexTile> getNeighbors(HexTile tile)
    {
        List<HexTile> neighbors = new List<HexTile>();

        int q = tile.q;
        int r = tile.r;

        bool isEvenRow = (r % 2 == 0);

        if (isEvenRow)
        {

            neighbors.Add(HexGrid.instance.getHexTile(q - 1, r)); 
            neighbors.Add(HexGrid.instance.getHexTile(q + 1, r)); 
            neighbors.Add(HexGrid.instance.getHexTile(q - 1, r - 1)); 
            neighbors.Add(HexGrid.instance.getHexTile(q, r - 1)); 
            neighbors.Add(HexGrid.instance.getHexTile(q - 1, r + 1));
            neighbors.Add(HexGrid.instance.getHexTile(q, r + 1)); 
        }
        else
        {
            neighbors.Add(HexGrid.instance.getHexTile(q - 1, r)); 
            neighbors.Add(HexGrid.instance.getHexTile(q + 1, r)); 
            neighbors.Add(HexGrid.instance.getHexTile(q, r - 1)); 
            neighbors.Add(HexGrid.instance.getHexTile(q + 1, r - 1));
            neighbors.Add(HexGrid.instance.getHexTile(q, r + 1)); 
            neighbors.Add(HexGrid.instance.getHexTile(q + 1, r + 1));
        }
        
        return neighbors;
    }
}


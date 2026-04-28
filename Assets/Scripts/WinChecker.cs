using System.Collections.Generic;
using UnityEngine;

public class WinChecker : MonoBehaviour
{

    public static WinChecker instance;

    void Awake()
    {
         if (instance != null && instance != this) { Destroy(gameObject); return; }
         instance = this;
    }

    public bool CheckWin(int player)
        {
            var grid = HexGrid.instance;
            var stack = new Stack<HexTile>();
            var visited = new HashSet<HexTile>();
    
            
            for (int q = 0; q < grid.width; q++)
            for (int r = 0; r < grid.height; r++)
            {
                HexTile tile = grid.getHexTile(q, r);
                if (tile == null || tile.owner != player) continue;
    
                bool onStartEdge = (player == 1) ? (q == 0) : (r == 0);
                if (onStartEdge) stack.Push(tile);
            }
    
            while (stack.Count > 0)
            {
                HexTile current = stack.Pop();
                if (!visited.Add(current)) continue;
    
                bool onGoalEdge = (player == 1)
                    ? (current.q == grid.width - 1)
                    : (current.r == grid.height - 1);
    
                if (onGoalEdge) return true;
    
                foreach (HexTile neighbor in GetNeighbors(current))
                {
                    if (neighbor.owner == player && !visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }
            return false;
        }
    
        
        internal List<HexTile> GetNeighbors(HexTile tile)
        {
            var neighbors = new List<HexTile>(6);
            int q = tile.q, r = tile.r;
            var grid = HexGrid.instance;
    
            (int dq, int dr)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1) };
            foreach (var (dq, dr) in dirs)
            {
                HexTile n = grid.getHexTile(q + dq, r + dr);
                if (n != null) neighbors.Add(n);   // null-guarded
            }
            return neighbors;
        }
}


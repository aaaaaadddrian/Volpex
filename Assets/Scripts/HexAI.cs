using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexAI : MonoBehaviour
{
   public static HexAI instance;
   
    const float UCT_C = 0.7f;

    const float RAVE_K = 500f;
    
    const int EXPAND_THRESHOLD = 3;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }
    
    public void TakeTurn(int player) => StartCoroutine(TakeTurnCoroutine(player));

    IEnumerator TakeTurnCoroutine(int player)
    {
        int thinkMs = GameSettings.instance != null
            ? GameSettings.instance.aiThinkingTimeMs : 1500;
        float delay = GameManager.instance.IsAIControlled(1 - player) ? 0.4f : 0.8f;
        yield return new WaitForSeconds(delay);

        HexTile best = ChooseMove(player, thinkMs);
        if (best != null) best.setOwner(player);

        GameManager.instance.OnAIMoveDone(player);
    }
    
    HexTile ChooseMove(int player, int thinkMs)
    {
        int[,] board = SnapshotBoard();
        int w = board.GetLength(0), h = board.GetLength(1);
        List<int[]> empty = GetEmptyTiles(board);
        if (empty.Count == 0) return null;

   
        foreach (var m in empty)
        {
            board[m[0], m[1]] = player;
            if (BoardCheckWin(board, player))
                return HexGrid.instance.getHexTile(m[0], m[1]);
            board[m[0], m[1]] = -1;
        }

   
        int opp = 1 - player;
        foreach (var m in empty)
        {
            board[m[0], m[1]] = opp;
            if (BoardCheckWin(board, opp))
            {
                board[m[0], m[1]] = -1;
                return HexGrid.instance.getHexTile(m[0], m[1]);
            }
            board[m[0], m[1]] = -1;
        }

        // 3. Block opponent virtual connection threat
        int[] threatBlock = FindThreatBlock(board, opp, w, h);
        if (threatBlock != null)
        {
            // Only block if this isn't also our best MCTS move anyway —
            // verify via a quick Dijkstra score comparison
            int[] mctsMove = RunMCTS(board, player, thinkMs / 2, w, h);
            float blockScore = DijkstraScore(board, player, threatBlock[0],
                                             threatBlock[1], w, h);
            float mctsScore  = DijkstraScore(board, player, mctsMove[0],
                                             mctsMove[1], w, h);

            int[] chosen = blockScore >= mctsScore ? threatBlock : mctsMove;
            return HexGrid.instance.getHexTile(chosen[0], chosen[1]);
        }

        // 4. Full MCTS
        int[] best = RunMCTS(board, player, thinkMs, w, h);
        return HexGrid.instance.getHexTile(best[0], best[1]);
    }

    int[] FindThreatBlock(int[,] board, int opp, int w, int h)
    {
        List<int[]> empty = GetEmptyTiles(board);
        int[]  bestMove  = null;
        float  bestGain  = 0f;

        foreach (var m in empty)
        {
            // How much does this empty tile reduce opp's path if they play here?
            board[m[0], m[1]] = opp;
            float pathBefore = DijkstraPathLength(board, opp, w, h);
            board[m[0], m[1]] = -1;

            float pathAfter  = DijkstraPathLength(board, opp, w, h);
            float gain = pathAfter - pathBefore; // positive = big threat

            if (gain > bestGain)
            {
                bestGain = gain;
                bestMove = m;
            }
        }

        // Only report as a threat if the gain is significant
        return bestGain > 1.5f ? bestMove : null;
    }

    int[] RunMCTS(int[,] board, int player, int thinkMs, int w, int h)
    {
        var root = new TreeNode(null, -1, -1, player);
        root.Expand(board, w, h);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < thinkMs)
        {
            int[,] simBoard = CopyBoard(board);

            // --- SELECTION ---
            TreeNode node = root;
            while (node.IsFullyExpanded && node.Children.Count > 0)
            {
                node = SelectChild(node);
                simBoard[node.Q, node.R] = node.Player;
            }

            // --- EXPANSION ---
            if (!node.IsFullyExpanded && node.Visits >= EXPAND_THRESHOLD)
            {
                TreeNode child = node.ExpandNext(simBoard, w, h);
                if (child != null)
                {
                    simBoard[child.Q, child.R] = child.Player;
                    node = child;
                }
            }

            // --- SIMULATION ---
            var aiMoves   = new List<(int q, int r)>();
            bool aiWins   = Simulate(simBoard, 1 - node.Player, player,
                                     aiMoves, w, h);

            // --- BACKPROPAGATION ---
            Backpropagate(node, aiWins, aiMoves, player);
        }

        // Return the child of root with the most visits (most robust choice)
        TreeNode bestChild = null;
        int mostVisits = -1;
        foreach (var c in root.Children)
        {
            if (c.Visits > mostVisits) { mostVisits = c.Visits; bestChild = c; }
        }

        return bestChild != null
            ? new[] { bestChild.Q, bestChild.R }
            : new[] { 0, 0 };
    }
    
    TreeNode SelectChild(TreeNode parent)
    {
        float logParent = parent.Visits > 0 ? Mathf.Log(parent.Visits) : 0f;
        TreeNode best   = null;
        float bestScore = float.MinValue;

        foreach (var child in parent.Children)
        {
            if (child.Visits == 0)
            {
                best = child;
                break; // always try unvisited children first
            }

            float mctsRate = child.Wins / (float)child.Visits;
            float raveRate = child.RaveVisits > 0
                ? child.RaveWins / (float)child.RaveVisits : 0f;

            float beta  = Mathf.Sqrt(RAVE_K / (3f * child.Visits + RAVE_K));
            float score = (1f - beta) * mctsRate + beta * raveRate
                        + UCT_C * Mathf.Sqrt(logParent / child.Visits);

            if (score > bestScore) { bestScore = score; best = child; }
        }
        return best;
    }
    void Backpropagate(TreeNode node, bool aiWins,
                       List<(int q, int r)> aiMoves, int aiPlayer)
    {
        var aiMoveSet = new HashSet<(int, int)>(aiMoves);
        TreeNode current = node;

        while (current != null)
        {
            current.Visits++;
            if (aiWins) current.Wins++;

            // RAVE update for all children of this node
            if (current.Children != null)
            {
                foreach (var child in current.Children)
                {
                    if (child.Player == aiPlayer &&
                        aiMoveSet.Contains((child.Q, child.R)))
                    {
                        child.RaveVisits++;
                        if (aiWins) child.RaveWins++;
                    }
                }
            }

            current = current.Parent;
        }
    }

    bool Simulate(int[,] board, int nextTurn, int aiPlayer,
                  List<(int q, int r)> aiMoves, int w, int h)
    {
        var empty = GetEmptyTiles(board);
        int turn  = nextTurn;

        while (empty.Count > 0)
        {
            int[] move = PickPlayoutMove(empty, board, turn, w, h);
            board[move[0], move[1]] = turn;

            if (turn == aiPlayer)
                aiMoves.Add((move[0], move[1]));

            empty.Remove(move);
            turn = 1 - turn;
        }

        return BoardCheckWin(board, aiPlayer);
    }

    int[] PickPlayoutMove(List<int[]> empty, int[,] board, int player, int w, int h)
    {
        // Tier 1: virtual connection completion
        foreach (var m in empty)
            if (IsVirtualConnectionCompletion(board, m[0], m[1], player, w, h))
                return m;

        // Tier 2: weighted sampling
        float[] weights = new float[empty.Count];
        float total = 0f;

        for (int i = 0; i < empty.Count; i++)
        {
            int q = empty[i][0], r = empty[i][1];

            float axisCentrality = player == 0
                ? (h - 1 - Mathf.Abs(r - (h - 1) * 0.5f))
                : (w - 1 - Mathf.Abs(q - (w - 1) * 0.5f));

            float adjacency = CountAdjacent(board, q, r, player, w, h) * 3f;

            float dijkstraGain = DijkstraScore(board, player, q, r, w, h);

            float score = Mathf.Max(1f + axisCentrality + adjacency
                                       + dijkstraGain * 2f, 0.1f);
            weights[i] = score;
            total += score;
        }

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < empty.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return empty[i];
        }
        return empty[empty.Count - 1];
    }
    
    float DijkstraPathLength(int[,] board, int player, int w, int h)
    {
        var dist = new float[w, h];
        for (int q = 0; q < w; q++)
        for (int r = 0; r < h; r++)
            dist[q, r] = float.MaxValue;

        // Min-heap: (cost, q, r)
        var pq = new SortedSet<(float cost, int q, int r)>(
            Comparer<(float, int, int)>.Create((a, b) =>
            {
                int c = a.Item1.CompareTo(b.Item1);
                if (c != 0) return c;
                c = a.Item2.CompareTo(b.Item2);
                return c != 0 ? c : a.Item3.CompareTo(b.Item3);
            }));

        // Seed from start edge
        for (int q = 0; q < w; q++)
        for (int r = 0; r < h; r++)
        {
            bool isStart = player == 0 ? r == 0 : q == 0;
            if (!isStart) continue;

            float cost = board[q, r] == player ? 0f
                       : board[q, r] == -1     ? 1f
                       : float.MaxValue;

            if (cost < float.MaxValue)
            {
                dist[q, r] = cost;
                pq.Add((cost, q, r));
            }
        }

        (int dq, int dr)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1) };

        while (pq.Count > 0)
        {
            var (cost, q, r) = pq.Min;
            pq.Remove(pq.Min);

            if (cost > dist[q, r]) continue;

            // Check if we reached the goal edge
            bool isGoal = player == 0 ? r == h - 1 : q == w - 1;
            if (isGoal) return cost;

            foreach (var (dq, dr) in dirs)
            {
                int nq = q + dq, nr = r + dr;
                if (nq < 0 || nq >= w || nr < 0 || nr >= h) continue;

                float moveCost = board[nq, nr] == player ? 0f
                               : board[nq, nr] == -1     ? 1f
                               : float.MaxValue;

                if (moveCost == float.MaxValue) continue;

                float newDist = dist[q, r] + moveCost;
                if (newDist < dist[nq, nr])
                {
                    dist[nq, nr] = newDist;
                    pq.Add((newDist, nq, nr));
                }
            }
        }

        return float.MaxValue; // no path exists
    }
    
    float DijkstraScore(int[,] board, int player, int q, int r, int w, int h)
    {
        float before = DijkstraPathLength(board, player, w, h);
        board[q, r] = player;
        float after  = DijkstraPathLength(board, player, w, h);
        board[q, r] = -1;
        return Mathf.Max(0f, before - after);
    }
    
    bool IsVirtualConnectionCompletion(int[,] board, int q, int r,
                                        int player, int w, int h)
    {
        (int dq, int dr)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1) };
        int friendlyNeighbours = 0;
        var seen = new HashSet<(int, int)>();

        foreach (var (dq, dr) in dirs)
        {
            int nq = q + dq, nr = r + dr;
            if (nq < 0 || nq >= w || nr < 0 || nr >= h) continue;
            if (board[nq, nr] == player && seen.Add((nq, nr)))
                friendlyNeighbours++;
        }
        return friendlyNeighbours >= 2;
    }


    int CountAdjacent(int[,] board, int q, int r, int player, int w, int h)
    {
        (int dq, int dr)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1) };
        int count = 0;
        foreach (var (dq, dr) in dirs)
        {
            int nq = q + dq, nr = r + dr;
            if (nq >= 0 && nq < w && nr >= 0 && nr < h && board[nq, nr] == player)
                count++;
        }
        return count;
    }

    int[,] SnapshotBoard()
    {
        var grid = HexGrid.instance;
        var snap = new int[grid.width, grid.height];
        for (int q = 0; q < grid.width; q++)
        for (int r = 0; r < grid.height; r++)
        {
            HexTile t = grid.getHexTile(q, r);
            snap[q, r] = t != null ? t.owner : -1;
        }
        return snap;
    }

    int[,] CopyBoard(int[,] src)
    {
        var dst = new int[src.GetLength(0), src.GetLength(1)];
        System.Array.Copy(src, dst, src.Length);
        return dst;
    }

    List<int[]> GetEmptyTiles(int[,] board)
    {
        int w = board.GetLength(0), h = board.GetLength(1);
        var list = new List<int[]>();
        for (int q = 0; q < w; q++)
        for (int r = 0; r < h; r++)
            if (board[q, r] == -1) list.Add(new[] { q, r });
        return list;
    }

    bool BoardCheckWin(int[,] board, int player)
    {
        int w = board.GetLength(0), h = board.GetLength(1);
        var stack   = new Stack<(int q, int r)>();
        var visited = new HashSet<(int, int)>();

        for (int q = 0; q < w; q++)
        for (int r = 0; r < h; r++)
        {
            bool isStart = player == 0 ? r == 0 : q == 0;
            if (board[q, r] == player && isStart) stack.Push((q, r));
        }

        (int dq, int dr)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1) };
        while (stack.Count > 0)
        {
            var (q, r) = stack.Pop();
            if (!visited.Add((q, r))) continue;

            bool isGoal = player == 0 ? r == h - 1 : q == w - 1;
            if (isGoal) return true;

            foreach (var (dq, dr) in dirs)
            {
                int nq = q + dq, nr = r + dr;
                if (nq < 0 || nq >= w || nr < 0 || nr >= h) continue;
                if (board[nq, nr] == player && !visited.Contains((nq, nr)))
                    stack.Push((nq, nr));
            }
        }
        return false;
    }
}

class TreeNode
{
    public TreeNode Parent;
    public List<TreeNode> Children = new List<TreeNode>();

    public int Q, R;       // the move this node represents
    public int Player;     // whose move it was

    public int   Visits;
    public float Wins;

    public int   RaveVisits;
    public float RaveWins;

    List<int[]> _untriedMoves;
    public bool IsFullyExpanded => _untriedMoves == null || _untriedMoves.Count == 0;

    public TreeNode(TreeNode parent, int q, int r, int player)
    {
        Parent = parent;
        Q = q; R = r;
        Player = player;
    }

    // Populate the untried moves list from the current board state
    public void Expand(int[,] board, int w, int h)
    {
        _untriedMoves = new List<int[]>();
        for (int q = 0; q < w; q++)
        for (int r = 0; r < h; r++)
            if (board[q, r] == -1) _untriedMoves.Add(new[] { q, r });
    }

    
    public TreeNode ExpandNext(int[,] board, int w, int h)
    {
        if (_untriedMoves == null) Expand(board, w, h);
        if (_untriedMoves.Count == 0) return null;

        
        int idx  = UnityEngine.Random.Range(0, _untriedMoves.Count);
        int[] m  = _untriedMoves[idx];
        _untriedMoves.RemoveAt(idx);

        var child = new TreeNode(this, m[0], m[1], 1 - Player);
        child.Expand(board, w, h);
        Children.Add(child);
        return child;
    }
}

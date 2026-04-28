using UnityEngine;

public class HexTile : MonoBehaviour
{
    public int q;
    public int r;
    public int owner = -1;
    
    public SpriteRenderer sprite;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void setOwner(int player)
    {
        owner = player;
        sprite.color = player == 0 ? Color.blue : Color.red;
    }
    
    public void UpdateVisual()
    {
        sprite.color = owner == 0 ? Color.blue : Color.red;
    }

    public void OnMouseDown()
    {
        if (GameManager.instance.gameOver) return;
        if (GameManager.instance.isAITurn) return;
        if (owner != -1) return;

        int player = GameManager.instance.currentPlayer;

        
        if (GameManager.instance.IsAIControlled(player)) return;

        setOwner(player);

        if (WinChecker.instance.CheckWin(player))
        {
            GameManager.instance.declareWinner(player);
            return;
        }

        GameManager.instance.endTurn();
    }
}

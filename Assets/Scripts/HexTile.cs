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
        if (player == 0)
        {
            sprite.color = Color.blue;
        }

        if (player == 1)
        {
            sprite.color = Color.red;
        }
    }

    public void OnMouseDown()
    {
        if (GameManager.instance.gameOver)
        {
            return;
        }
        
        if (owner != -1)
        {
            return;
        }
        
        int player = GameManager.instance.currentPlayer;
    
        setOwner(player);

        if (WinChecker.instance.checkWin(player))
        {
            GameManager.instance.declareWinner(player);
            return;
        }
        
        GameManager.instance.endTurn();
    }
}

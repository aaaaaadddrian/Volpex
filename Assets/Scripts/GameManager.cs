using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentPlayer = 0;
    public bool gameOver = false;

    void Awake()
    {
        instance = this;
    }

    public void endTurn()
    {

        if (gameOver)
        {
            return;
        }
        
        currentPlayer = 1 - currentPlayer;
    }

    public void declareWinner(int player)
    {
        gameOver = true;
        Debug.Log((player == 0 ? "Blue" : "Red") + " wins!");
    }
    
}

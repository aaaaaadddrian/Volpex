using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentPlayer = 0;

    void Awake()
    {
        instance = this;
    }

    public void endTurn()
    {
        currentPlayer = 1 - currentPlayer;
    }
    
}

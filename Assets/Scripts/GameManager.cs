using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentPlayer = 1;
    public bool gameOver = false;
    public bool isAITurn = false;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }
    
    void Start()
    {
        TriggerAIIfNeeded();
    }
    
    public bool IsAIControlled(int player)
    {
        if (GameSettings.instance == null) return false;
        return player == 0
            ? GameSettings.instance.player0Type == 1
            : GameSettings.instance.player1Type == 1;
    }
    
    void TriggerAIIfNeeded()
    {
        if (gameOver) return;
        if (!IsAIControlled(currentPlayer)) return;

        isAITurn = true;
        HexAI.instance.TakeTurn(currentPlayer);
    }

    public void endTurn()
    {
        if (gameOver) return;

        currentPlayer = 1 - currentPlayer;
        TriggerAIIfNeeded();

        if (!IsAIControlled(currentPlayer))
            isAITurn = false;
    }

    public void OnAIMoveDone(int player)
    {
        if (WinChecker.instance.CheckWin(player))
        {
            declareWinner(player);
            return;
        }

        isAITurn = false;
        currentPlayer = 1 - player;
        TriggerAIIfNeeded();

        if (!IsAIControlled(currentPlayer))
            isAITurn = false;
    }

    public void declareWinner(int player)
    {
        gameOver = true;
        UIBehavior.instance.showWinner(player);
    }
    
}

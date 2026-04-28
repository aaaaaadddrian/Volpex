using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance;

    // 0 = Human, 1 = AI
    public int player0Type = 0; // Blue
    public int player1Type = 1; // Red  (default: Human vs AI)

    public int aiThinkingTimeMs = 1500;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIBehavior : MonoBehaviour
{
    public static UIBehavior instance;

    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        winnerPanel.SetActive(false);
    }

    public void showWinner(int player)
    {
        winnerPanel.SetActive(true);

        if (player == 0)
        {
            winnerText.text = "BLUE WINS";
            winnerText.color = new Color(0.2f, 0.6f, 1f);
        }
        else
        {
            winnerText.text = "RED WINS";
            winnerText.color = new Color(1f, 0.3f, 0.3f);
        }
    }

    public void PlayAgain()
    {
        // Reload the gameplay scene fresh
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    
}

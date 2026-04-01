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
        instance = this;
        winnerPanel.SetActive(false);
    }

    public void showWinner(int player)
    {
        winnerPanel.SetActive(true);

        if (player == 0)
        {
            winnerText.text = "BLUE WINS";
            winnerText.color = Color.blue;
        }
        else
        {
            winnerText.text = "RED WINS!";
            winnerText.color = Color.red;
        }
    }

    public void playAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void mainMenu()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    
}

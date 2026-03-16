using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject instructionPanel;
    public GameObject settingsPanel;
    public CanvasGroup instructionCanvasGroup;
    public MenuManager instance;
    

    void Start()
    {
        if (instance && this.instance != null)
        {
            Destroy(instance);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Scenes/gameplay");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // helps test in editor
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ShowInstructions()
    {
        instructionPanel.SetActive(true);
        StartCoroutine(FadeInstructions());
    }

    IEnumerator FadeInstructions()
    {
        instructionCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(4);

        float duration = 2f;
        float time = 0;

        while (time < duration)
        {
            instructionCanvasGroup.alpha =
                Mathf.Lerp(1, 0, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        instructionCanvasGroup.alpha = 0;
        instructionPanel.SetActive(false);
    }
}

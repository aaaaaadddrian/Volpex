using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Panels")]
    public GameObject instructionPanel;
    public GameObject settingsPanel;
    public CanvasGroup instructionCanvasGroup;

    [Header("Settings UI")]
    public TMP_Dropdown gameModeDropdown;   
    public Slider thinkTimeSlider;          
    public TextMeshProUGUI thinkTimeLabel;  
    public GameObject aiSettingsGroup;      

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // destroy the NEW duplicate, keep the original
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Ensure GameSettings exists (attach to same GameObject or a separate one)
        if (GameSettings.instance == null)
            gameObject.AddComponent<GameSettings>();

        // Initialise UI to match current settings
        thinkTimeSlider.minValue = 500;
        thinkTimeSlider.maxValue = 5000;
        thinkTimeSlider.value = GameSettings.instance.aiThinkingTimeMs;
        UpdateThinkLabel(thinkTimeSlider.value);

        thinkTimeSlider.onValueChanged.AddListener(OnThinkTimeChanged);
        gameModeDropdown.onValueChanged.AddListener(OnGameModeChanged);

        // Set dropdown to match saved setting
        int mode = GetModeIndex(
            GameSettings.instance.player0Type,
            GameSettings.instance.player1Type);
        gameModeDropdown.value = mode;
        UpdateAISettingsVisibility(mode);
    }

    // Dropdown option index → player types
    void OnGameModeChanged(int index)
    {
        switch (index)
        {
            case 0: // Human vs AI  (Red human, Blue AI)
                GameSettings.instance.player1Type = 0; // Red = Human
                GameSettings.instance.player0Type = 1; // Blue = AI
                break;
            case 1: // Human vs Human
                GameSettings.instance.player1Type = 0;
                GameSettings.instance.player0Type = 0;
                break;
            case 2: // AI vs AI
                GameSettings.instance.player1Type = 1;
                GameSettings.instance.player0Type = 1;
                break;
        }
        UpdateAISettingsVisibility(index);
    }

    void OnThinkTimeChanged(float value)
    {
        GameSettings.instance.aiThinkingTimeMs = Mathf.RoundToInt(value);
        UpdateThinkLabel(value);
    }

    void UpdateThinkLabel(float ms) =>
        thinkTimeLabel.text = $"AI Think Time: {ms / 1000f:0.0}s";

    void UpdateAISettingsVisibility(int mode) =>
        aiSettingsGroup.SetActive(mode != 1); // hide for Human vs Human

    int GetModeIndex(int p0, int p1)
    {
        if (p0 == 1 && p1 == 0) return 0; // Human vs AI
        if (p0 == 0 && p1 == 0) return 1; // Human vs Human
        return 2;                          // AI vs AI
    }

    public void StartGame() => SceneManager.LoadScene("Scenes/gameplay");
    public void QuitGame()  { Application.Quit(); Debug.Log("Quit"); }
    public void OpenSettings()  => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    public void ShowInstructions()
    {
        instructionPanel.SetActive(true);
        StartCoroutine(FadeInstructions());
    }

    IEnumerator FadeInstructions()
    {
        instructionCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(4f);
        float t = 0f;
        while (t < 2f)
        {
            instructionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 2f);
            t += Time.deltaTime;
            yield return null;
        }
        instructionCanvasGroup.alpha = 0f;
        instructionPanel.SetActive(false);
    }
    
    
}

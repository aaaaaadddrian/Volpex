using TMPro;
using UnityEngine;

public class TitleChanger : MonoBehaviour
{

    public float speed = 1.0f;
    private TextMeshProUGUI title;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        title = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        Color blue = Color.blue;
        Color red = Color.red;

        title.color = Color.Lerp(blue, red, t);
    }
}

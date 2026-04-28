using UnityEngine;
using UnityEngine.EventSystems;

public class SingletonEventSystem : MonoBehaviour
{
    void Awake()
    {
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (systems.Length > 1)
        {
            Destroy(gameObject); // destroy this one, the DontDestroyOnLoad one survives
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

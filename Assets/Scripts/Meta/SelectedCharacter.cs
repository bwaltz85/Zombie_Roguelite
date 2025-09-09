using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectedCharacter : MonoBehaviour
{
    public static SelectedCharacter I;

    [Header("Runtime")]
    public CharacterData selected;

    [Header("Config")]
    public string gameplaySceneName = "Game"; // <-- set to your gameplay scene name

    void Awake()
    {
        if (I == null) { I = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void Choose(CharacterData cd)
    {
        selected = cd;
        SceneManager.LoadScene(gameplaySceneName);
    }
}

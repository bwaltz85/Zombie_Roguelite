using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectUI_Simple : MonoBehaviour
{
    [Header("Selection (single)")]
    [Tooltip("Player prefab that will be spawned in the Game scene.")]
    public GameObject playerPrefab;

    [Tooltip("CharacterData (ScriptableObject) applied by PlayerSetup in the Game scene).")]
    public CharacterData characterData;

    private const string GAME_SCENE_NAME = "Game"; // your scene name

    public void OnPlayPressed()
    {
        if (playerPrefab == null || characterData == null)
        {
            Debug.LogError("[CharacterSelectUI_Simple] Assign playerPrefab and characterData in the inspector before pressing Play.");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(GAME_SCENE_NAME))
        {
            Debug.LogError($"[CharacterSelectUI_Simple] Scene '{GAME_SCENE_NAME}' is not in Build Settings. Add it and try again.");
            return;
        }

        SelectedChoice.PlayerPrefab = playerPrefab;
        SelectedChoice.CharacterData = characterData;
        SelectedChoice.LogState("OnPlayPressed (before LoadScene)");

        Time.timeScale = 1f;
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }
}

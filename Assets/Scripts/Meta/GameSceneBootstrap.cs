using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    [Header("References")]
    public PlayerSetup playerSetup;        // drag your Player root here
    public CharacterData fallbackCharacter; // used if no selection was made
    public EnemySpawner enemySpawner;      // optional: auto-wire player
    public bool autoStartRun = true;       // calls GameLoop.StartRun

    void Start()
    {
        // Find PlayerSetup if not assigned
        if (!playerSetup) playerSetup = FindFirstObjectByType<PlayerSetup>();
        if (!playerSetup)
        {
            Debug.LogError("[GameSceneBootstrap] No PlayerSetup found in scene.");
            return;
        }

        // Get chosen character or fallback
        CharacterData chosen = SelectedCharacter.I ? SelectedCharacter.I.selected : null;
        if (!chosen) chosen = fallbackCharacter;

        if (chosen)
        {
            playerSetup.ApplyData(chosen);
        }
        else
        {
            Debug.LogWarning("[GameSceneBootstrap] No character selected and no fallback; player will use default stats.");
        }

        // Auto-wire EnemySpawner's player (for around-player spawning)
        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner && enemySpawner.player == null)
            enemySpawner.player = playerSetup.transform;

        // Start the run
        if (autoStartRun && GameLoop.I != null)
            GameLoop.I.StartRun();

        // Resume time if you paused elsewhere
        Time.timeScale = 1f;
    }
}

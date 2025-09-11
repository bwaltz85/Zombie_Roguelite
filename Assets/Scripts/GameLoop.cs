using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameLoop : MonoBehaviour
{
    public static GameLoop I { get; private set; }

    [Header("State")]
    public GameState State = GameState.Menu;

    [Header("Options")]
    [Tooltip("If true, this object persists between scene loads.")]
    public bool dontDestroyOnLoad = true;

    float runStartTime;
    float pausedTimeAccumulated;
    float lastPauseTime;

    /// <summary>
    /// Time since the current run started, excluding pause time.
    /// Returns 0 if not currently playing.
    /// </summary>
    public float Elapsed
    {
        get
        {
            if (State == GameState.Menu || State == GameState.GameOver)
                return 0f;

            float now = Time.time;
            float baseElapsed = now - runStartTime;

            if (State == GameState.Paused)
                return (lastPauseTime - runStartTime) - pausedTimeAccumulated;

            return baseElapsed - pausedTimeAccumulated;
        }
    }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        if (Time.timeScale <= 0f) Time.timeScale = 1f;
    }

    // --- Public API ---

    /// <summary> Begin a run: time unpaused, state -> Playing. </summary>
    public void StartRun()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        runStartTime = Time.time;
        pausedTimeAccumulated = 0f;
        lastPauseTime = 0f;
        Debug.Log("[GameLoop] Run started (state=Playing).");
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        lastPauseTime = Time.time;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        pausedTimeAccumulated += Time.time - lastPauseTime;
        lastPauseTime = 0f;
        Time.timeScale = 1f;
    }

    public void EndRunToMenu()
    {
        State = GameState.Menu;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        State = GameState.GameOver;
        Time.timeScale = 0f;
    }
}

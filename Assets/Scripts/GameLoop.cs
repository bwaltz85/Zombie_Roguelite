using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // new Input System

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameLoop : MonoBehaviour
{
    public static GameLoop I;

    [Header("Events")]
    public UnityEvent onStart;
    public UnityEvent onPause;
    public UnityEvent onResume;
    public UnityEvent onGameOver;

    public GameState State { get; private set; } = GameState.Menu;
    public float Elapsed { get; private set; }

    // Runtime-created input actions (no generated Controls class required)
    private InputAction pauseAction;
    private InputAction restartAction;

    void Awake()
    {
        if (I == null) I = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // Create input actions programmatically
        pauseAction = new InputAction(name: "Pause", type: InputActionType.Button, binding: "<Keyboard>/escape");
        restartAction = new InputAction(name: "Restart", type: InputActionType.Button, binding: "<Keyboard>/r");

        pauseAction.performed += _ =>
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        };

        restartAction.performed += _ =>
        {
            if (State == GameState.GameOver) Restart();
        };
    }

    void OnEnable()
    {
        pauseAction?.Enable();
        restartAction?.Enable();
    }

    void OnDisable()
    {
        restartAction?.Disable();
        pauseAction?.Disable();
    }

    void Update()
    {
        if (State == GameState.Playing) Elapsed += Time.deltaTime;
    }

    public void StartRun()
    {
        Elapsed = 0f;
        State = GameState.Playing;
        Time.timeScale = 1f;
        onStart?.Invoke();
        Debug.Log("[GameLoop] Run started.");
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        onPause?.Invoke();
        Debug.Log("[GameLoop] Paused.");
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        onResume?.Invoke();
        Debug.Log("[GameLoop] Resumed.");
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        Time.timeScale = 0f;
        onGameOver?.Invoke();
        Debug.Log("[GameLoop] Game Over. Press R to Restart.");
    }

    public void Restart()
    {
        State = GameState.Menu;
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
